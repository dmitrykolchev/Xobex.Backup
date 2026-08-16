// ssd_refresh.cpp
// ------------------------------------------------------------------------
// Эксклюзивная поблочная перезапись тома "на месте" (read -> write того же
// LBA-диапазона) для форсирования FTL SSD к переносу логических адресов
// в свежие физические NAND-страницы, + двухпроходная верификация
// содержимого через SHA-256 (Windows CNG / BCrypt).
//
// МЕХАНИЗМ РЕФРЕША: SSD FTL реализует out-of-place update (log-structured),
// поэтому запись в уже отображённый LBA всегда создаёт НОВУЮ физическую
// страницу и помечает старую как invalid — это сбрасывает V_th-дрейф без
// изменения содержимого файловой системы.
//
// МЕХАНИЗМ ВЕРИФИКАЦИИ: read-immediately-after-write НЕ доказывает
// durability записи — SSD может отдать данные из SLC write-кэша, а не
// с физической TLC/QLC-страницы. Поэтому:
//   Проход 1 (MainLoop): хешируем буфер перед записью, копим
//                         {offset, length, hash} в памяти.
//   Проход 2 (VerifyPass, ПОСЛЕ завершения всего рефреша): повторное
//                         чтение тех же диапазонов, пересчёт хэша,
//                         сравнение. К этому моменту SLC-кэш давно
//                         вытеснен объёмом прошедших записей —
//                         проверяется физическая NAND, а не кэш.
//
// ЧТО ЭТО ДОКАЗЫВАЕТ: операция не исказила то, что было на диске перед
// её запуском. ЧТО НЕ ДОКАЗЫВАЕТ: что исходные данные были корректны
// (необнаруженная ECC-ошибка чтения будет добросовестно скопирована и
// пройдёт верификацию, т.к. оба прохода читают уже испорченный сектор).
// Единственная защита от этого класса риска — независимый бэкап ДО
// запуска, не программная мера.
//
// ТРЕБОВАНИЯ: elevated администратор, том не системный/загрузочный,
// все процессы, использующие том, закрыты, ОБЯЗАТЕЛЕН бэкап данных.
//
// СБОРКА (VS2022/2026): консольное приложение (Win32), Unicode,
// /std:c++20. Линковка: bcrypt.lib (подключается через #pragma ниже).
// В манифест — requestedExecutionLevel="requireAdministrator".
//
// ЗАПУСК: ssd_refresh.exe \\.\E: [--no-verify]
// ------------------------------------------------------------------------

#include <windows.h>
#include <winioctl.h>
#include <bcrypt.h>
#include <algorithm>
#include <array>
#include <chrono>
#include <iomanip>
#include <iostream>
#include <sstream>
#include <string>
#include <vector>

#pragma comment(lib, "bcrypt.lib")

namespace {

    using Sha256Digest = std::array<unsigned char, 32>;

    std::wstring FormatSize(unsigned long long bytes) {
        const wchar_t* units[] = { L"B", L"KB", L"MB", L"GB", L"TB" };
        double v = static_cast<double>(bytes);
        int u = 0;
        while (v >= 1024.0 && u < 4) { v /= 1024.0; ++u; }
        std::wstringstream ss;
        ss << std::fixed << std::setprecision(2) << v << L" " << units[u];
        return ss.str();
    }

    struct BadRange {
        unsigned long long offset;
        unsigned long long length;
        DWORD lastError;
    };

    struct ChunkRecord {
        unsigned long long offset;
        unsigned long long length;
        Sha256Digest hash;
    };

} // namespace

// ==========================================================================
// SecurityManager — проверка прав доступа (без изменений в логике)
// ==========================================================================
class SecurityManager {
public:
    static bool IsElevatedAdministrator() {
        BOOL isAdminMember = FALSE;
        PSID adminGroupSid = nullptr;
        SID_IDENTIFIER_AUTHORITY ntAuthority = SECURITY_NT_AUTHORITY;

        if (!AllocateAndInitializeSid(&ntAuthority, 2,
            SECURITY_BUILTIN_DOMAIN_RID, DOMAIN_ALIAS_RID_ADMINS,
            0, 0, 0, 0, 0, 0, &adminGroupSid)) {
            std::wcerr << L"[-] AllocateAndInitializeSid: код ошибки " << GetLastError() << L"\n";
            return false;
        }
        BOOL checkOk = CheckTokenMembership(nullptr, adminGroupSid, &isAdminMember);
        FreeSid(adminGroupSid);
        if (!checkOk) {
            std::wcerr << L"[-] CheckTokenMembership: код ошибки " << GetLastError() << L"\n";
            return false;
        }
        if (!isAdminMember) {
            std::wcerr << L"[-] Текущий пользователь не входит в группу Administrators.\n";
            return false;
        }

        HANDLE hToken = nullptr;
        if (!OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, &hToken)) {
            std::wcerr << L"[-] OpenProcessToken: код ошибки " << GetLastError() << L"\n";
            return false;
        }

        TOKEN_ELEVATION_TYPE elevType{};
        DWORD retLen = 0;
        bool elevated = false;
        if (GetTokenInformation(hToken, TokenElevationType, &elevType, sizeof(elevType), &retLen)) {
            elevated = (elevType == TokenElevationTypeFull || elevType == TokenElevationTypeDefault);
            if (elevType == TokenElevationTypeLimited) {
                std::wcerr << L"[-] Админ-аккаунт БЕЗ elevation (UAC). Запустите через "
                    L"\"Запуск от имени администратора\".\n";
            }
        }
        else {
            std::wcerr << L"[-] GetTokenInformation(TokenElevationType): код ошибки " << GetLastError() << L"\n";
        }

        CloseHandle(hToken);
        return elevated;
    }

    static void TryEnableManageVolumePrivilege() {
        HANDLE hToken = nullptr;
        if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken)) {
            return;
        }
        LUID luid{};
        if (LookupPrivilegeValueW(nullptr, SE_MANAGE_VOLUME_NAME, &luid)) {
            TOKEN_PRIVILEGES tp{};
            tp.PrivilegeCount = 1;
            tp.Privileges[0].Luid = luid;
            tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
            SetLastError(ERROR_SUCCESS);
            AdjustTokenPrivileges(hToken, FALSE, &tp, sizeof(tp), nullptr, nullptr);
            if (GetLastError() == ERROR_NOT_ALL_ASSIGNED) {
                std::wcerr << L"[i] SeManageVolumePrivilege недоступна токену (не критично).\n";
            }
        }
        CloseHandle(hToken);
    }
};

// ==========================================================================
// VolumeRefresher — жизненный цикл операции + верификация
// ==========================================================================
class VolumeRefresher {
public:
    ~VolumeRefresher() { Cleanup(); }

    bool Prepare(const std::wstring& volumePath) {
        if (!OpenVolume(volumePath)) return false;
        if (!QueryGeometry()) return false;      // на ещё смонтированном хендле
        if (!OpenHashProvider()) return false;
        if (!LockAndDismount()) return false;
        if (!AllocateBuffer()) return false;
        return true;
    }

    // doVerify=false пропускает второй проход (быстрее, но без гарантий
    // из п.1 ответа — использовать только на этапе диагностики, не на
    // проде без крайней необходимости).
    int Run(bool doVerify) {
        MainLoop();
        if (doVerify) VerifyPass();
        Cleanup();
        return PrintSummary();
    }

    void EmergencyUnlock() { Cleanup(); }

private:
    HANDLE hVolume_ = INVALID_HANDLE_VALUE;
    bool locked_ = false;
    unsigned long long alignment_ = 0;
    unsigned long long totalBytes_ = 0;
    size_t chunkAligned_ = 0;
    void* buffer_ = nullptr;
    BCRYPT_ALG_HANDLE hHashAlg_ = nullptr;

    std::vector<BadRange> badRanges_;       // не удалось прочитать/записать при рефреше
    std::vector<ChunkRecord> verified_;     // успешно перезаписанные диапазоны + их хэш
    std::vector<BadRange> mismatches_;      // проход 2: хэш разошёлся с проходом 1

    // ---- подготовка ----

    bool OpenVolume(const std::wstring& volumePath) {
        hVolume_ = CreateFileW(
            volumePath.c_str(),
            GENERIC_READ | GENERIC_WRITE,
            FILE_SHARE_READ | FILE_SHARE_WRITE,
            nullptr,
            OPEN_EXISTING,
            FILE_FLAG_NO_BUFFERING | FILE_FLAG_WRITE_THROUGH,
            nullptr);

        if (hVolume_ == INVALID_HANDLE_VALUE) {
            DWORD err = GetLastError();
            std::wcerr << L"[FATAL] CreateFile: код ошибки " << err << L"\n";
            if (err == ERROR_ACCESS_DENIED)
                std::wcerr << L"        Запустите программу от имени администратора.\n";
            return false;
        }
        return true;
    }

    bool QueryGeometry() {
        DWORD br = 0;

        DISK_GEOMETRY geometry{};
        if (!DeviceIoControl(hVolume_, IOCTL_DISK_GET_DRIVE_GEOMETRY, nullptr, 0,
            &geometry, sizeof(geometry), &br, nullptr)) {
            std::wcerr << L"[FATAL] IOCTL_DISK_GET_DRIVE_GEOMETRY: код ошибки " << GetLastError() << L"\n";
            return false;
        }
        unsigned long long sectorSize = geometry.BytesPerSector ? geometry.BytesPerSector : 512;
        alignment_ = (sectorSize < 4096) ? 4096 : sectorSize;

        GET_LENGTH_INFORMATION lengthInfo{};
        if (!DeviceIoControl(hVolume_, IOCTL_DISK_GET_LENGTH_INFO, nullptr, 0,
            &lengthInfo, sizeof(lengthInfo), &br, nullptr)) {
            std::wcerr << L"[FATAL] IOCTL_DISK_GET_LENGTH_INFO: код ошибки " << GetLastError() << L"\n";
            return false;
        }
        totalBytes_ = static_cast<unsigned long long>(lengthInfo.Length.QuadPart);

        std::wcout << L"[i] Размер тома: " << FormatSize(totalBytes_) << L" (" << totalBytes_ << L" B)\n"
            << L"[i] Сектор: " << sectorSize << L" B, I/O-выравнивание: " << alignment_ << L" B\n";
        return true;
    }

    bool OpenHashProvider() {
        NTSTATUS status = BCryptOpenAlgorithmProvider(&hHashAlg_, BCRYPT_SHA256_ALGORITHM, nullptr, 0);
        if (status < 0) {
            std::wcerr << L"[FATAL] BCryptOpenAlgorithmProvider(SHA256): NTSTATUS=0x"
                << std::hex << status << std::dec << L"\n";
            return false;
        }
        return true;
    }

    bool LockAndDismount() {
        DWORD br = 0;
        if (!DeviceIoControl(hVolume_, FSCTL_LOCK_VOLUME, nullptr, 0, nullptr, 0, &br, nullptr)) {
            std::wcerr << L"[FATAL] FSCTL_LOCK_VOLUME: код ошибки " << GetLastError()
                << L"\n        Том занят другим процессом (антивирус, индексатор, "
                L"explorer.exe, открытые файлы).\n";
            return false;
        }
        locked_ = true;
        std::wcout << L"[OK] Том заблокирован в эксклюзивном режиме.\n";
        DeviceIoControl(hVolume_, FSCTL_DISMOUNT_VOLUME, nullptr, 0, nullptr, 0, &br, nullptr);
        return true;
    }

    bool AllocateBuffer() {
        const size_t DEFAULT_CHUNK = 8ull * 1024 * 1024; // 8 MB
        chunkAligned_ = static_cast<size_t>((DEFAULT_CHUNK / alignment_) * alignment_);
        if (chunkAligned_ == 0) chunkAligned_ = static_cast<size_t>(alignment_);

        buffer_ = VirtualAlloc(nullptr, chunkAligned_, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        if (!buffer_) {
            std::wcerr << L"[FATAL] VirtualAlloc: не удалось выделить буфер.\n";
            return false;
        }
        return true;
    }

    // ---- хэширование ----

    bool ComputeSha256(const void* data, size_t len, Sha256Digest& out) {
        BCRYPT_HASH_HANDLE hHash = nullptr;
        NTSTATUS status = BCryptCreateHash(hHashAlg_, &hHash, nullptr, 0, nullptr, 0, 0);
        if (status < 0) return false;

        status = BCryptHashData(hHash, static_cast<PUCHAR>(const_cast<void*>(data)),
            static_cast<ULONG>(len), 0);
        if (status >= 0) {
            status = BCryptFinishHash(hHash, out.data(), static_cast<ULONG>(out.size()), 0);
        }
        BCryptDestroyHash(hHash);
        return status >= 0;
    }

    // ---- проход 1: рефреш ----

    // Пытается прочитать/записать [offset, offset+attemptSize) целиком;
    // при ошибке дробит гранулу пополам вплоть до alignment_. Успешные
    // диапазоны хэшируются и попадают в verified_ для второго прохода.
    unsigned long long ProcessOffset(unsigned long long offset) {
        unsigned long long remaining = totalBytes_ - offset;
        size_t attemptSize = static_cast<size_t>(std::min<unsigned long long>(chunkAligned_, remaining));
        attemptSize = static_cast<size_t>(((attemptSize + alignment_ - 1) / alignment_) * alignment_);
        if (attemptSize > remaining) attemptSize = static_cast<size_t>((remaining / alignment_) * alignment_);
        if (attemptSize == 0) attemptSize = static_cast<size_t>(std::min<unsigned long long>(alignment_, remaining));

        DWORD lastErr = 0;
        LARGE_INTEGER li{};

        while (attemptSize >= alignment_) {
            li.QuadPart = static_cast<LONGLONG>(offset);
            if (!SetFilePointerEx(hVolume_, li, nullptr, FILE_BEGIN)) {
                lastErr = GetLastError();
                break;
            }

            DWORD bytesRead = 0;
            BOOL rOk = ReadFile(hVolume_, buffer_, static_cast<DWORD>(attemptSize), &bytesRead, nullptr);

            if (rOk && bytesRead == attemptSize) {
                Sha256Digest hash{};
                bool hashOk = ComputeSha256(buffer_, attemptSize, hash); // хэш ДО записи

                li.QuadPart = static_cast<LONGLONG>(offset);
                SetFilePointerEx(hVolume_, li, nullptr, FILE_BEGIN);

                DWORD bytesWritten = 0;
                BOOL wOk = WriteFile(hVolume_, buffer_, bytesRead, &bytesWritten, nullptr);
                if (wOk && bytesWritten == bytesRead) {
                    if (hashOk) {
                        verified_.push_back({ offset, attemptSize, hash });
                    }
                    else {
                        std::wcerr << L"\n[!] Хэш не посчитан для offset=" << offset
                            << L" — диапазон исключён из верификации.\n";
                    }
                    return attemptSize; // успех
                }
                lastErr = GetLastError();
            }
            else {
                lastErr = GetLastError();
            }

            if (attemptSize == alignment_) break;
            size_t half = static_cast<size_t>(((attemptSize / 2 + alignment_ - 1) / alignment_) * alignment_);
            attemptSize = (half < alignment_) ? static_cast<size_t>(alignment_) : half;
        }

        badRanges_.push_back({ offset, alignment_, lastErr });
        return alignment_;
    }

    void MainLoop() {
        unsigned long long offset = 0;
        auto start = std::chrono::steady_clock::now();
        auto lastReport = start;

        while (offset < totalBytes_) {
            offset += ProcessOffset(offset);
            ReportProgress(L"Рефреш", offset, totalBytes_, start, lastReport);
        }
        std::wcout << L"\n[OK] Проход 1 (рефреш) завершён. Верифицируемых диапазонов: "
            << verified_.size() << L", bad-ranges: " << badRanges_.size() << L"\n";
    }

    // ---- проход 2: верификация ----

    // Перечитывает КАЖДЫЙ успешно перезаписанный диапазон и сравнивает
    // хэш с зафиксированным в проходе 1. Выполняется ПОСЛЕ полного
    // завершения MainLoop — намеренно, чтобы гарантированно выйти за
    // пределы любого onboard write-кэша (см. комментарий в шапке файла).
    void VerifyPass() {
        if (verified_.empty()) return;

        std::wcout << L"[i] Запуск прохода 2 (верификация " << verified_.size() << L" диапазонов)...\n";
        auto start = std::chrono::steady_clock::now();
        auto lastReport = start;
        unsigned long long processed = 0;
        unsigned long long totalVerifyBytes = 0;
        for (auto& rec : verified_) totalVerifyBytes += rec.length;

        LARGE_INTEGER li{};
        for (const auto& rec : verified_) {
            li.QuadPart = static_cast<LONGLONG>(rec.offset);
            bool ok = SetFilePointerEx(hVolume_, li, nullptr, FILE_BEGIN) != 0;

            DWORD bytesRead = 0;
            if (ok) {
                ok = ReadFile(hVolume_, buffer_, static_cast<DWORD>(rec.length), &bytesRead, nullptr)
                    && bytesRead == rec.length;
            }

            if (ok) {
                Sha256Digest hash{};
                ok = ComputeSha256(buffer_, static_cast<size_t>(rec.length), hash) && (hash == rec.hash);
            }

            if (!ok) {
                mismatches_.push_back({ rec.offset, rec.length, GetLastError() });
            }

            processed += rec.length;
            ReportProgress(L"Верификация", processed, totalVerifyBytes, start, lastReport);
        }
        std::wcout << L"\n[OK] Проход 2 (верификация) завершён. Расхождений: " << mismatches_.size() << L"\n";
    }

    void ReportProgress(const wchar_t* label, unsigned long long processed, unsigned long long total,
        std::chrono::steady_clock::time_point start,
        std::chrono::steady_clock::time_point& lastReport) {
        auto now = std::chrono::steady_clock::now();
        if (std::chrono::duration_cast<std::chrono::milliseconds>(now - lastReport).count() < 1000) return;

        double elapsed = std::chrono::duration<double>(now - start).count();
        double speed = (elapsed > 0) ? (static_cast<double>(processed) / elapsed) : 0.0;
        double pct = (total > 0) ? (100.0 * static_cast<double>(processed) / static_cast<double>(total)) : 0.0;

        std::wcout << L"\r[.] " << label << L": " << std::fixed << std::setprecision(1) << pct << L"%  "
            << FormatSize(processed) << L" / " << FormatSize(total)
            << L"  ~" << FormatSize(static_cast<unsigned long long>(speed)) << L"/s     " << std::flush;
        lastReport = now;
    }

    // ---- завершение ----

    void Cleanup() {
        if (buffer_) {
            VirtualFree(buffer_, 0, MEM_RELEASE);
            buffer_ = nullptr;
        }
        if (hHashAlg_) {
            BCryptCloseAlgorithmProvider(hHashAlg_, 0);
            hHashAlg_ = nullptr;
        }
        if (hVolume_ != INVALID_HANDLE_VALUE) {
            if (locked_) {
                DWORD br = 0;
                DeviceIoControl(hVolume_, FSCTL_UNLOCK_VOLUME, nullptr, 0, nullptr, 0, &br, nullptr);
                locked_ = false;
            }
            CloseHandle(hVolume_);
            hVolume_ = INVALID_HANDLE_VALUE;
        }
    }

    // Приоритет кодов: verify-mismatch (3) хуже bad-range (2) — первое
    // значит "наша же операция что-то исказила", второе — "сектор был
    // физически нечитаем ДО нашего вмешательства".
    int PrintSummary() const {
        if (!mismatches_.empty()) {
            std::wcerr << L"\n[!!!] РАСХОЖДЕНИЯ ПОСЛЕ ВЕРИФИКАЦИИ (" << mismatches_.size() << L"):\n";
            for (auto& r : mismatches_) {
                std::wcerr << L"    offset=" << r.offset << L"  length=" << r.length
                    << L"  WinError=" << r.lastError << L"\n";
            }
            std::wcerr << L"    Содержимое этих диапазонов ПОСЛЕ рефреша не совпадает с тем, что было\n"
                L"    прочитано и записано в проходе 1. Немедленно остановите использование\n"
                L"    тома и восстановите данные из бэкапа — не полагайтесь на данные, лежащие\n"
                L"    в этих диапазонах сейчас.\n";
        }
        if (!badRanges_.empty()) {
            std::wcerr << L"\n[!] Нечитаемые/незаписываемые диапазоны при рефреше (" << badRanges_.size() << L"):\n";
            for (auto& r : badRanges_) {
                std::wcerr << L"    offset=" << r.offset << L"  length=" << r.length
                    << L"  WinError=" << r.lastError << L"\n";
            }
            std::wcerr << L"    Эти LBA превысили возможности Soft-LDPC (UNC) ещё ДО начала операции —\n"
                L"    данные, вероятно, физически утрачены независимо от нашего вмешательства.\n";
        }
        if (mismatches_.empty() && badRanges_.empty()) {
            std::wcout << L"[OK] Все диапазоны перезаписаны и верифицированы без расхождений.\n";
            return 0;
        }
        return mismatches_.empty() ? 2 : 3;
    }
};

// ==========================================================================
// Ctrl+C: аварийная разблокировка тома
// ==========================================================================
namespace {

    VolumeRefresher* g_active = nullptr;

    BOOL WINAPI ConsoleHandler(DWORD signal) {
        if (signal == CTRL_C_EVENT || signal == CTRL_CLOSE_EVENT || signal == CTRL_BREAK_EVENT) {
            std::wcerr << L"\n[!] Прерывание. Разблокировка тома...\n";
            if (g_active) g_active->EmergencyUnlock();
        }
        return FALSE;
    }

    bool ConfirmExclusiveAccess(const std::wstring& volumePath, bool verify) {
        std::wcout << L"=== SSD Cold-Data Refresh (in-place read/write) ===\n";
        std::wcout << L"Целевой том: " << volumePath << L"\n";
        std::wcout << L"Верификация (2-й проход, SHA-256): " << (verify ? L"ВКЛЮЧЕНА" : L"ОТКЛЮЧЕНА (--no-verify)") << L"\n";
        std::wcout << L"ВНИМАНИЕ: требуется эксклюзивный доступ. Убедитесь, что у вас есть\n"
            L"АКТУАЛЬНЫЙ БЭКАП данных этого тома. Закройте все программы, использующие\n"
            L"этот диск. Продолжить? (y/n): ";
        wchar_t answer = 0;
        std::wcin >> answer;
        return answer == L'y' || answer == L'Y';
    }

} // namespace

// ==========================================================================
// wmain — тонкий CLI-слой
// ==========================================================================
int wmain(int argc, wchar_t* argv[]) {
    std::wstring volumePath = L"\\\\.\\E:";
    bool verify = true;

    for (int i = 1; i < argc; ++i) {
        std::wstring arg = argv[i];
        if (arg == L"--no-verify") verify = false;
        else if (!arg.empty() && arg[0] != L'-') volumePath = arg;
    }

    if (!ConfirmExclusiveAccess(volumePath, verify)) {
        std::wcout << L"Отменено.\n";
        return 0;
    }

    if (!SecurityManager::IsElevatedAdministrator()) {
        std::wcerr << L"[FATAL] Требуется elevated-токен администратора для эксклюзивного raw-доступа к тому.\n";
        return 1;
    }
    SecurityManager::TryEnableManageVolumePrivilege();

    SetConsoleCtrlHandler(ConsoleHandler, TRUE);

    VolumeRefresher refresher;
    g_active = &refresher;

    if (!refresher.Prepare(volumePath)) {
        return 1;
    }
    return refresher.Run(verify);
}