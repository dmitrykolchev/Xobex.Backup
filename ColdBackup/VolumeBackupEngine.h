#pragma once

// --- Вспомогательная структура для параметров кластеризации ---
struct VolumeClusterInfo {
    uint32_t bytesPerSector = 0;
    uint32_t sectorsPerCluster = 0;
    uint64_t totalClusters = 0;
};

class VolumeBackupEngine {
public:
    static bool QueryVolumeClusterInfo(HANDLE hVolume, const std::wstring& volumePath, VolumeClusterInfo& outInfo) {
        DWORD bytesReturned = 0;

        // 1. Попытка получить точные данные NTFS напрямую через дескриптор устройства
        NTFS_VOLUME_DATA_BUFFER ntfsData{};
        if (DeviceIoControl(hVolume, FSCTL_GET_NTFS_VOLUME_DATA, nullptr, 0,
            &ntfsData, sizeof(ntfsData), &bytesReturned, nullptr)) {
            outInfo.bytesPerSector = ntfsData.BytesPerSector;
            outInfo.sectorsPerCluster = ntfsData.BytesPerCluster / ntfsData.BytesPerSector;
            outInfo.totalClusters = ntfsData.TotalClusters.QuadPart;
            return true;
        }

        // 2. Fallback для FAT32/exFAT через GetDiskFreeSpaceW (до блокировки тома)
        std::wstring rootPath = volumePath;
        if (rootPath.rfind(L"\\\\.\\", 0) == 0) {
            rootPath = rootPath.substr(4);
        }
        if (rootPath.back() != L'\\') {
            rootPath += L'\\';
        }

        DWORD sectorsPerCluster = 0, bytesPerSector = 0, freeClusters = 0, totalClusters = 0;
        if (GetDiskFreeSpaceW(rootPath.c_str(), &sectorsPerCluster, &bytesPerSector, &freeClusters, &totalClusters)) {
            outInfo.bytesPerSector = bytesPerSector;
            outInfo.sectorsPerCluster = sectorsPerCluster;
            outInfo.totalClusters = totalClusters;
            return true;
        }

        // 3. Fallback геометрии диска через IOCTL_DISK_GET_DRIVE_GEOMETRY_EX
        DISK_GEOMETRY_EX geometry{};
        if (DeviceIoControl(hVolume, IOCTL_DISK_GET_DRIVE_GEOMETRY_EX, nullptr, 0,
            &geometry, sizeof(geometry), &bytesReturned, nullptr)) {
            outInfo.bytesPerSector = geometry.Geometry.BytesPerSector;
            outInfo.sectorsPerCluster = 8; // Стандартное значение по умолчанию (4KB кластер при 512B секторе)
            outInfo.totalClusters = geometry.DiskSize.QuadPart / (outInfo.bytesPerSector * outInfo.sectorsPerCluster);
            return true;
        }

        return false;
    }

    static bool CreateBackup(const std::wstring& volumePath, const std::wstring& imagePath) {
        std::wcout << L"[*] Opening source volume: " << volumePath << std::endl;

        // 1. Открытие тома
        SafeHandle hVolume(CreateFileW(
            volumePath.c_str(),
            GENERIC_READ | GENERIC_WRITE,
            FILE_SHARE_READ | FILE_SHARE_WRITE,
            nullptr,
            OPEN_EXISTING,
            FILE_FLAG_NO_BUFFERING | FILE_FLAG_WRITE_THROUGH | FILE_FLAG_BACKUP_SEMANTICS,
            nullptr
        ));

        if (!hVolume.isValid()) {
            std::cerr << "[-] Failed to open volume. Error: " << GetLastError() << std::endl;
            return false;
        }

        // 2. Сброс системных буферов файловой системы
        FlushFileBuffers(hVolume.get());

        // 3. Получение параметров кластеризации (на открытом смонтированном томе)
        VolumeClusterInfo clusterInfo{};
        if (!QueryVolumeClusterInfo(hVolume.get(), volumePath, clusterInfo)) {
            std::cerr << "[-] Failed to query volume cluster info. Error: " << GetLastError() << std::endl;
            return false;
        }

        uint64_t volumeLength = VolumeManager::GetVolumeLength(hVolume.get());
        const uint32_t clusterSize = clusterInfo.bytesPerSector * clusterInfo.sectorsPerCluster;

        // 4. Считывание карты занятых кластеров ДО вызова FSCTL_LOCK_VOLUME
        std::cout << "[*] Fetching volume bitmap (pre-lock)..." << std::endl;
        STARTING_LCN_INPUT_BUFFER inputLcn{};
        inputLcn.StartingLcn.QuadPart = 0;

        DWORD bitmapBufferSize = sizeof(VOLUME_BITMAP_BUFFER) + static_cast<DWORD>(clusterInfo.totalClusters / 8) + 65536;
        std::vector<uint8_t> bitmapRaw(bitmapBufferSize);
        DWORD bytesReturned = 0;

        if (!DeviceIoControl(hVolume.get(), FSCTL_GET_VOLUME_BITMAP, &inputLcn, sizeof(inputLcn),
            bitmapRaw.data(), bitmapBufferSize, &bytesReturned, nullptr)) {
            std::cerr << "[-] FSCTL_GET_VOLUME_BITMAP failed. Error: " << GetLastError() << std::endl;
            return false;
        }

        auto* bitmap = reinterpret_cast<PVOLUME_BITMAP_BUFFER>(bitmapRaw.data());
        uint64_t totalBitmapClusters = bitmap->BitmapSize.QuadPart;
        std::cout << "[+] Bitmap acquired successfully. Total clusters: " << totalBitmapClusters << std::endl;

        // 5. Захват эксклюзивной блокировки для безопасного прямого чтения блоков
        std::cout << "[*] Acquiring exclusive volume lock for streaming..." << std::endl;
        bool isLocked = VolumeManager::TryLockVolumeForBackup(hVolume.get());
        if (isLocked) {
            std::cout << "[+] Exclusive volume lock acquired." << std::endl;
        }
        else {
            std::cout << "[!] Warning: Active handles detected. Proceeding in shared unbuffered mode..." << std::endl;
        }

        // 6. Инициализация выходного файла образа
        SafeHandle hImage(CreateFileW(
            imagePath.c_str(),
            GENERIC_WRITE,
            0,
            nullptr,
            CREATE_ALWAYS,
            FILE_ATTRIBUTE_NORMAL | FILE_FLAG_SEQUENTIAL_SCAN,
            nullptr
        ));

        if (!hImage.isValid()) {
            std::cerr << "[-] Failed to create output image file. Error: " << GetLastError() << std::endl;
            if (isLocked) {
                DWORD dummy = 0;
                DeviceIoControl(hVolume.get(), FSCTL_UNLOCK_VOLUME, nullptr, 0, nullptr, 0, &dummy, nullptr);
            }
            return false;
        }

        ImageHeader header{
            IMAGE_MAGIC,
            IMAGE_VERSION,
            clusterInfo.bytesPerSector,
            clusterInfo.sectorsPerCluster,
            totalBitmapClusters,
            volumeLength
        };

        DWORD written = 0;
        WriteFile(hImage.get(), &header, sizeof(header), &written, nullptr);

        // 7. Потоковое чтение занятых экстентов (Direct I/O, 4MB Chunks)
        const size_t chunkSize = 4 * 1024 * 1024;
        const size_t clustersPerChunk = chunkSize / clusterSize;
        AlignedBuffer readBuffer(chunkSize);

        uint64_t currentLcn = 0;
        const uint8_t* bitBuffer = bitmap->Buffer;
        uint64_t totalAllocatedClustersCopied = 0;

        std::cout << "[*] Streaming allocated extents..." << std::endl;

        while (currentLcn < totalBitmapClusters) {
            if (!(bitBuffer[currentLcn / 8] & (1 << (currentLcn % 8)))) {
                currentLcn++;
                continue;
            }

            uint64_t startLcn = currentLcn;
            while (currentLcn < totalBitmapClusters && (bitBuffer[currentLcn / 8] & (1 << (currentLcn % 8)))) {
                currentLcn++;
            }
            uint64_t extentLength = currentLcn - startLcn;

            uint64_t offsetInExtent = 0;
            while (offsetInExtent < extentLength) {
                uint64_t batchClusters = (std::min)(extentLength - offsetInExtent, static_cast<uint64_t>(clustersPerChunk));
                uint64_t batchLcn = startLcn + offsetInExtent;
                uint64_t bytesToRead = batchClusters * clusterSize;

                LARGE_INTEGER seekPos;
                seekPos.QuadPart = batchLcn * clusterSize;
                SetFilePointerEx(hVolume.get(), seekPos, nullptr, FILE_BEGIN);

                DWORD bytesRead = 0;
                if (!ReadFile(hVolume.get(), readBuffer.get(), static_cast<DWORD>(bytesToRead), &bytesRead, nullptr)) {
                    std::cerr << "\n[-] Direct Read error at offset " << seekPos.QuadPart << " Error: " << GetLastError() << std::endl;
                    if (isLocked) {
                        DWORD dummy = 0;
                        DeviceIoControl(hVolume.get(), FSCTL_UNLOCK_VOLUME, nullptr, 0, nullptr, 0, &dummy, nullptr);
                    }
                    return false;
                }
                std::cout << std::format("\r{:#018X}", batchLcn);

                BlockRecordHeader blockHeader{
                    BlockType::ClusterExtent,
                    static_cast<uint64_t>(seekPos.QuadPart),
                    bytesRead
                };

                WriteFile(hImage.get(), &blockHeader, sizeof(blockHeader), &written, nullptr);
                WriteFile(hImage.get(), readBuffer.get(), bytesRead, &written, nullptr);

                offsetInExtent += batchClusters;
                totalAllocatedClustersCopied += batchClusters;
            }
        }

        // 8. Сохранение Backup Boot Sector (последний сектор тома)
        if (volumeLength > clusterInfo.bytesPerSector) {
            LARGE_INTEGER lastSectorOffset;
            lastSectorOffset.QuadPart = volumeLength - clusterInfo.bytesPerSector;

            SetFilePointerEx(hVolume.get(), lastSectorOffset, nullptr, FILE_BEGIN);
            DWORD bytesRead = 0;
            if (ReadFile(hVolume.get(), readBuffer.get(), clusterInfo.bytesPerSector, &bytesRead, nullptr)) {
                BlockRecordHeader backupVbrHdr{
                    BlockType::RawByteOffset,
                    static_cast<uint64_t>(lastSectorOffset.QuadPart),
                    bytesRead
                };
                WriteFile(hImage.get(), &backupVbrHdr, sizeof(backupVbrHdr), &written, nullptr);
                WriteFile(hImage.get(), readBuffer.get(), bytesRead, &written, nullptr);
            }
        }

        // 9. Завершающий маркер потока
        BlockRecordHeader endMarker{ BlockType::EndOfStream, 0, 0 };
        WriteFile(hImage.get(), &endMarker, sizeof(endMarker), &written, nullptr);

        // 10. Снятие блокировки
        if (isLocked) {
            DWORD dummy = 0;
            DeviceIoControl(hVolume.get(), FSCTL_UNLOCK_VOLUME, nullptr, 0, nullptr, 0, &dummy, nullptr);
        }

        std::cout << "\n[+] Image creation complete!" << std::endl;
        std::cout << "    Total clusters backed up: " << totalAllocatedClustersCopied
            << " / " << totalBitmapClusters
            << " (" << (totalAllocatedClustersCopied * clusterSize / (1024 * 1024)) << " MB)" << std::endl;
        return true;
    }
};
