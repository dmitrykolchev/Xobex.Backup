#pragma once

class VolumeRestoreEngine {
public:
    static bool RestoreBackup(const std::wstring& imagePath, const std::wstring& targetVolumePath) {
        std::wcout << L"[*] Target volume: " << targetVolumePath << std::endl;

        // 1. Открытие файла образа
        SafeHandle hImage(CreateFileW(
            imagePath.c_str(),
            GENERIC_READ,
            FILE_SHARE_READ,
            nullptr,
            OPEN_EXISTING,
            FILE_ATTRIBUTE_NORMAL | FILE_FLAG_SEQUENTIAL_SCAN,
            nullptr
        ));

        if (!hImage.isValid()) {
            std::cerr << "[-] Failed to open backup image. Error: " << GetLastError() << std::endl;
            return false;
        }

        // 2. Чтение и валидация заголовка образа
        ImageHeader header{};
        DWORD bytesRead = 0;
        if (!ReadFile(hImage.get(), &header, sizeof(header), &bytesRead, nullptr) ||
            bytesRead != sizeof(header) ||
            header.Magic != IMAGE_MAGIC) {
            std::cerr << "[-] Invalid image format or corrupt header." << std::endl;
            return false;
        }

        if (header.Version != IMAGE_VERSION) {
            std::cerr << "[-] Unsupported image version: " << header.Version << std::endl;
            return false;
        }

        std::cout << "[*] Image metadata:" << std::endl;
        std::cout << "    Bytes per sector:   " << header.BytesPerSector << std::endl;
        std::cout << "    Sectors per cluster:" << header.SectorsPerCluster << std::endl;
        std::cout << "    Total clusters:     " << header.TotalClusters << std::endl;
        std::cout << "    Source volume size: " << header.VolumeLengthBytes << " bytes" << std::endl;

        // 3. Открытие целевого тома в unbuffered режиме с Backup Semantics
        SafeHandle hTarget(CreateFileW(
            targetVolumePath.c_str(),
            GENERIC_READ | GENERIC_WRITE,
            FILE_SHARE_READ | FILE_SHARE_WRITE,
            nullptr,
            OPEN_EXISTING,
            FILE_FLAG_NO_BUFFERING | FILE_FLAG_WRITE_THROUGH | FILE_FLAG_BACKUP_SEMANTICS,
            nullptr
        ));

        if (!hTarget.isValid()) {
            std::cerr << "[-] Failed to open target volume for writing. Error: " << GetLastError() << std::endl;
            return false;
        }

        // 4. Захват блокировки и принудительное размонтирование ФС для прямой записи
        std::cout << "[*] Locking and dismounting target volume..." << std::endl;
        if (!VolumeManager::LockAndDismountForRestore(hTarget.get())) {
            std::cerr << "[-] Failed to exclusively lock and dismount target volume. Error: " << GetLastError() << std::endl;
            return false;
        }

        // 5. Проверка вместимости целевого раздела
        uint64_t targetLength = VolumeManager::GetVolumeLength(hTarget.get());
        if (targetLength < header.VolumeLengthBytes) {
            std::cerr << "[-] Target partition size (" << targetLength
                << " B) is smaller than original volume (" << header.VolumeLengthBytes << " B)!" << std::endl;
            DWORD dummy = 0;
            DeviceIoControl(hTarget.get(), FSCTL_UNLOCK_VOLUME, nullptr, 0, nullptr, 0, &dummy, nullptr);
            return false;
        }

        // 6. Буфер для Direct I/O (16MB)
        const size_t maxBufferSize = 16 * 1024 * 1024;
        AlignedBuffer writeBuffer(maxBufferSize);

        std::cout << "[*] Restoring blocks to disk..." << std::endl;
        BlockRecordHeader record{};
        uint64_t totalBytesRestored = 0;

        while (ReadFile(hImage.get(), &record, sizeof(record), &bytesRead, nullptr) && bytesRead == sizeof(record)) {
            if (record.Type == BlockType::EndOfStream) {
                break;
            }

            if (record.DataSize > maxBufferSize) {
                std::cerr << "[-] Block size (" << record.DataSize << " B) exceeds maximum buffer limit." << std::endl;
                DWORD dummy = 0;
                DeviceIoControl(hTarget.get(), FSCTL_UNLOCK_VOLUME, nullptr, 0, nullptr, 0, &dummy, nullptr);
                return false;
            }

            // Валидация требований выравнивания для FILE_FLAG_NO_BUFFERING
            if ((record.TargetOffset % header.BytesPerSector != 0) ||
                (record.DataSize % header.BytesPerSector != 0)) {
                std::cerr << "[-] Corrupted record: Target offset or size is not sector-aligned!" << std::endl;
                DWORD dummy = 0;
                DeviceIoControl(hTarget.get(), FSCTL_UNLOCK_VOLUME, nullptr, 0, nullptr, 0, &dummy, nullptr);
                return false;
            }

            // Чтение полезной нагрузки из файла образа
            if (!ReadFile(hImage.get(), writeBuffer.get(), static_cast<DWORD>(record.DataSize), &bytesRead, nullptr) ||
                bytesRead != record.DataSize) {
                std::cerr << "[-] Corrupted image stream: unexpected EOF or read failure." << std::endl;
                DWORD dummy = 0;
                DeviceIoControl(hTarget.get(), FSCTL_UNLOCK_VOLUME, nullptr, 0, nullptr, 0, &dummy, nullptr);
                return false;
            }

            // Прямая запись на блочное устройство
            LARGE_INTEGER writeOffset;
            writeOffset.QuadPart = record.TargetOffset;
            SetFilePointerEx(hTarget.get(), writeOffset, nullptr, FILE_BEGIN);

            DWORD written = 0;
            if (!WriteFile(hTarget.get(), writeBuffer.get(), static_cast<DWORD>(record.DataSize), &written, nullptr) ||
                written != record.DataSize) {
                std::cerr << "\n[-] Direct Write error at offset " << record.TargetOffset
                    << " Error: " << GetLastError() << std::endl;
                DWORD dummy = 0;
                DeviceIoControl(hTarget.get(), FSCTL_UNLOCK_VOLUME, nullptr, 0, nullptr, 0, &dummy, nullptr);
                return false;
            }

            totalBytesRestored += record.DataSize;
        }

        // 7. Принудительное размонтирование и снятие блокировки
        // Это заставляет ядро Windows обнаружить новую файловую систему при следующем обращении
        DWORD dummy = 0;
        DeviceIoControl(hTarget.get(), FSCTL_DISMOUNT_VOLUME, nullptr, 0, nullptr, 0, &dummy, nullptr);
        DeviceIoControl(hTarget.get(), FSCTL_UNLOCK_VOLUME, nullptr, 0, nullptr, 0, &dummy, nullptr);

        std::cout << "\n[+] Restore operation finished successfully." << std::endl;
        std::cout << "    Total data written: " << (totalBytesRestored / (1024 * 1024)) << " MB" << std::endl;
        return true;
    }
};
