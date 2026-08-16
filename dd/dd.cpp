// dd.cpp - Windows port of the classic UNIX `dd` utility.
//
// Build (MSVC / Visual Studio 2026, x64 Native Tools Command Prompt):
//     cl /std:c++17 /O2 /EHsc /W4 /utf-8 dd.cpp /Fe:dd.exe
//
// Or via CMake (see CMakeLists.txt).
//
// Design notes are in README.md. Summary of Windows-specific mechanics
// implemented here:
//
//   1. SeManageVolumePrivilege is enabled in the process token at startup
//      (RAII-scoped) before any lock/dismount attempt, since volume lock,
//      dismount and SetFileValidData are gated on it (Microsoft Learn,
//      "Managing Privileges in a File System").
//   2. Pure C++ (RAII wrappers for HANDLE, privilege state and volume
//      locks; no manual malloc/free, no naked new/delete).
//   3. `\\.\X:` notation is accepted, and bare `X:` is normalized to it;
//      a stray trailing backslash on a volume device path is stripped
//      (CreateFile("\\.\X:\") opens the root *directory*, not the volume).
//   4. FSCTL_LOCK_VOLUME / FSCTL_UNLOCK_VOLUME are wrapped in a `VolumeLock`
//      RAII guard bound to the exact HANDLE that will subsequently perform
//      the I/O (locking through one handle and writing through another is
//      not honoured by NTFS: "a locked volume can be accessed only through
//      [the] handle... that locks it").
//   5. FSCTL_DISMOUNT_VOLUME is issued after a successful lock when
//      exclusive access is required. For `\\.\PhysicalDriveN` targets,
//      per the documented algorithm (open the disk, enumerate every child
//      volume via IOCTL_STORAGE_GET_DEVICE_NUMBER matching, lock+dismount
//      each, only then write to the disk handle), all resident volumes are
//      located, locked and dismounted before the disk write begins.
//   6. `iflag=bitmap`: when the source is a mounted NTFS volume, only the
//      allocated clusters (per FSCTL_GET_VOLUME_BITMAP) are read and
//      written; unallocated ranges are skipped and, for a regular-file
//      destination, represented as a Windows sparse range
//      (FSCTL_SET_SPARSE) instead of physically zero-filled bytes.

#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <windows.h>
#include <winioctl.h>

#include <cstdint>
#include <cstdio>
#include <cwchar>
#include <string>
#include <vector>
#include <algorithm>
#include <chrono>
#include <thread>
#include <optional>
#include <stdexcept>
#include <utility>

// =============================================================================
// Small formatting / error helpers
// =============================================================================

static void FatalErr(const std::wstring& msg) {
    fwprintf(stderr, L"dd: %ls\n", msg.c_str());
    exit(1);
}

static std::wstring Win32ErrorText(DWORD code) {
    LPWSTR buf = nullptr;
    DWORD n = FormatMessageW(
        FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM |
        FORMAT_MESSAGE_IGNORE_INSERTS,
        nullptr, code, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        reinterpret_cast<LPWSTR>(&buf), 0, nullptr);
    std::wstring s = (n && buf) ? std::wstring(buf, n) : L"unknown error";
    if (buf) LocalFree(buf);
    while (!s.empty() && (s.back() == L'\n' || s.back() == L'\r')) s.pop_back();
    return s;
}

static void FatalWin32(const std::wstring& ctx) {
    DWORD e = GetLastError();
    FatalErr(ctx + L": " + Win32ErrorText(e) + L" (code " + std::to_wstring(e) + L")");
}

static void WarnWin32(const std::wstring& ctx) {
    DWORD e = GetLastError();
    fwprintf(stderr, L"dd: warning: %ls: %ls (code %lu)\n", ctx.c_str(), Win32ErrorText(e).c_str(), e);
}

static std::wstring Widen(const std::string& s) {
    if (s.empty()) return {};
    int n = MultiByteToWideChar(CP_UTF8, 0, s.c_str(), (int)s.size(), nullptr, 0);
    std::wstring w(n, 0);
    MultiByteToWideChar(CP_UTF8, 0, s.c_str(), (int)s.size(), w.data(), n);
    return w;
}

// =============================================================================
// RAII primitives
// =============================================================================

// Owning wrapper around a Win32 HANDLE. Move-only.
class UniqueHandle {
public:
    UniqueHandle() = default;
    explicit UniqueHandle(HANDLE h) : h_(h) {}
    ~UniqueHandle() { Reset(); }

    UniqueHandle(const UniqueHandle&) = delete;
    UniqueHandle& operator=(const UniqueHandle&) = delete;

    UniqueHandle(UniqueHandle&& other) noexcept : h_(other.h_) { other.h_ = INVALID_HANDLE_VALUE; }
    UniqueHandle& operator=(UniqueHandle&& other) noexcept {
        if (this != &other) {
            Reset();
            h_ = other.h_;
            other.h_ = INVALID_HANDLE_VALUE;
        }
        return *this;
    }

    [[nodiscard]] HANDLE Get() const { return h_; }
    [[nodiscard]] bool Valid() const { return h_ != INVALID_HANDLE_VALUE && h_ != nullptr; }

    void Reset(HANDLE h = INVALID_HANDLE_VALUE) {
        if (Valid()) CloseHandle(h_);
        h_ = h;
    }

private:
    HANDLE h_ = INVALID_HANDLE_VALUE;
};

// Enables a named privilege (e.g. SeManageVolumePrivilege) in the current
// process token for the lifetime of this object, restoring the prior
// enabled/disabled state on destruction. Privileges must be *present* in
// the token (i.e. the process is elevated/Administrator) to be enabled;
// this class reports failure via Ok() rather than throwing, since dd should
// still proceed (and simply fail later, with a clear error, at the actual
// FSCTL that needed the privilege) if it is unavailable.
class PrivilegeEnabler {
public:
    explicit PrivilegeEnabler(const wchar_t* privilegeName) {
        HANDLE rawToken = nullptr;
        if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &rawToken)) {
            lastError_ = GetLastError();
            return;
        }
        token_.Reset(rawToken);

        LUID luid{};
        if (!LookupPrivilegeValueW(nullptr, privilegeName, &luid)) {
            lastError_ = GetLastError();
            return;
        }

        TOKEN_PRIVILEGES want{};
        want.PrivilegeCount = 1;
        want.Privileges[0].Luid = luid;
        want.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

        TOKEN_PRIVILEGES previous{};
        DWORD returnLen = 0;
        BOOL adjusted = AdjustTokenPrivileges(token_.Get(), FALSE, &want, sizeof(previous), &previous, &returnLen);
        DWORD adjustErr = GetLastError();
        if (!adjusted || adjustErr == ERROR_NOT_ALL_ASSIGNED) {
            // Privilege not held by this token at all (not an admin token,
            // or the privilege was stripped) -- cannot be enabled.
            lastError_ = (adjustErr == 0) ? ERROR_NOT_ALL_ASSIGNED : adjustErr;
            token_.Reset();
            return;
        }

        luid_ = luid;
        wasEnabledBefore_ = (previous.PrivilegeCount == 1) &&
            ((previous.Privileges[0].Attributes & SE_PRIVILEGE_ENABLED) != 0);
        enabled_ = true;
    }

    ~PrivilegeEnabler() {
        if (enabled_ && !wasEnabledBefore_ && token_.Valid()) {
            TOKEN_PRIVILEGES revert{};
            revert.PrivilegeCount = 1;
            revert.Privileges[0].Luid = luid_;
            revert.Privileges[0].Attributes = 0; // disable
            AdjustTokenPrivileges(token_.Get(), FALSE, &revert, 0, nullptr, nullptr);
        }
    }

    PrivilegeEnabler(const PrivilegeEnabler&) = delete;
    PrivilegeEnabler& operator=(const PrivilegeEnabler&) = delete;

    [[nodiscard]] bool Ok() const { return enabled_; }
    [[nodiscard]] DWORD LastError() const { return lastError_; }

private:
    UniqueHandle token_;
    LUID luid_{};
    bool enabled_ = false;
    bool wasEnabledBefore_ = false;
    DWORD lastError_ = 0;
};

// RAII guard around FSCTL_LOCK_VOLUME / FSCTL_UNLOCK_VOLUME, bound to a
// *borrowed* (non-owning) HANDLE that the caller will go on to use for I/O.
// Per the FSCTL_LOCK_VOLUME documentation, the lock is only meaningful
// through the exact handle that acquired it, so this class never owns or
// duplicates the handle -- it just brackets its lifetime.
//
// FSCTL_LOCK_VOLUME can fail transiently if another process briefly holds a
// handle to a file on the volume (e.g. the shell enumerating it right after
// insertion), so a short bounded retry loop is used, matching common
// practice in backup/imaging tools.
class VolumeLock {
public:
    VolumeLock() = default;

    VolumeLock(HANDLE volumeHandle, bool alsoDismount, int retries = 5,
        std::chrono::milliseconds retryDelay = std::chrono::milliseconds(200))
        : h_(volumeHandle) {
        DWORD junk = 0;
        for (int attempt = 0; attempt <= retries; attempt++) {
            if (DeviceIoControl(h_, FSCTL_LOCK_VOLUME, nullptr, 0, nullptr, 0, &junk, nullptr)) {
                locked_ = true;
                break;
            }
            if (attempt < retries) std::this_thread::sleep_for(retryDelay);
        }
        if (!locked_) {
            lastError_ = GetLastError();
            return;
        }
        if (alsoDismount) {
            if (!DeviceIoControl(h_, FSCTL_DISMOUNT_VOLUME, nullptr, 0, nullptr, 0, &junk, nullptr)) {
                lastError_ = GetLastError();
                dismountFailed_ = true;
                // Volume stays locked even if dismount failed; still useful
                // (blocks new opens), so we do not unlock here.
            }
            else {
                dismounted_ = true;
            }
        }
    }

    ~VolumeLock() {
        if (locked_) {
            DWORD junk = 0;
            DeviceIoControl(h_, FSCTL_UNLOCK_VOLUME, nullptr, 0, nullptr, 0, &junk, nullptr);
        }
    }

    VolumeLock(const VolumeLock&) = delete;
    VolumeLock& operator=(const VolumeLock&) = delete;
    VolumeLock(VolumeLock&& other) noexcept { *this = std::move(other); }
    VolumeLock& operator=(VolumeLock&& other) noexcept {
        if (this != &other) {
            if (locked_) {
                DWORD junk = 0;
                DeviceIoControl(h_, FSCTL_UNLOCK_VOLUME, nullptr, 0, nullptr, 0, &junk, nullptr);
            }
            h_ = other.h_;
            locked_ = other.locked_;
            dismounted_ = other.dismounted_;
            dismountFailed_ = other.dismountFailed_;
            lastError_ = other.lastError_;
            other.locked_ = false;
        }
        return *this;
    }

    [[nodiscard]] bool Locked() const { return locked_; }
    [[nodiscard]] bool Dismounted() const { return dismounted_; }
    [[nodiscard]] bool DismountFailed() const { return dismountFailed_; }
    [[nodiscard]] DWORD LastError() const { return lastError_; }

private:
    HANDLE h_ = nullptr;
    bool locked_ = false;
    bool dismounted_ = false;
    bool dismountFailed_ = false;
    DWORD lastError_ = 0;
};

// RAII-aligned heap buffer (required for FILE_FLAG_NO_BUFFERING, and simply
// good hygiene for large I/O buffers otherwise).
class AlignedBuffer {
public:
    AlignedBuffer(size_t size, size_t alignment) : size_(size) {
        ptr_ = _aligned_malloc(size, alignment);
        if (!ptr_) throw std::bad_alloc();
    }
    ~AlignedBuffer() { if (ptr_) _aligned_free(ptr_); }

    AlignedBuffer(const AlignedBuffer&) = delete;
    AlignedBuffer& operator=(const AlignedBuffer&) = delete;
    AlignedBuffer(AlignedBuffer&& other) noexcept : ptr_(other.ptr_), size_(other.size_) {
        other.ptr_ = nullptr;
        other.size_ = 0;
    }

    [[nodiscard]] void* Data() const { return ptr_; }
    [[nodiscard]] size_t Size() const { return size_; }

private:
    void* ptr_ = nullptr;
    size_t size_ = 0;
};

// =============================================================================
// Size operand parsing (dd-style: suffixes + 'x' chaining)
// =============================================================================

static unsigned long long ParseSize(const std::wstring& raw, const wchar_t* what) {
    if (raw.empty()) FatalErr(std::wstring(L"empty numeric operand for ") + what);
    unsigned long long total = 1;
    size_t pos = 0;
    bool any = false;
    while (pos < raw.size()) {
        size_t start = pos;
        while (pos < raw.size() && iswdigit(raw[pos])) pos++;
        if (pos == start) FatalErr(std::wstring(L"invalid number in ") + what + L"=" + raw);
        unsigned long long val = std::wcstoull(raw.substr(start, pos - start).c_str(), nullptr, 10);
        any = true;

        unsigned long long mult = 1;
        size_t sufStart = pos;
        while (pos < raw.size() && iswalpha(raw[pos])) pos++;
        std::wstring suf = raw.substr(sufStart, pos - sufStart);
        if (!suf.empty()) {
            if (suf == L"c") mult = 1ULL;
            else if (suf == L"w") mult = 2ULL;
            else if (suf == L"b") mult = 512ULL;
            else if (suf == L"K" || suf == L"KiB") mult = 1024ULL;
            else if (suf == L"kB") mult = 1000ULL;
            else if (suf == L"k") mult = 1024ULL;
            else if (suf == L"M" || suf == L"MiB") mult = 1024ULL * 1024;
            else if (suf == L"MB") mult = 1000ULL * 1000;
            else if (suf == L"G" || suf == L"GiB") mult = 1024ULL * 1024 * 1024;
            else if (suf == L"GB") mult = 1000ULL * 1000 * 1000;
            else if (suf == L"T" || suf == L"TiB") mult = 1024ULL * 1024 * 1024 * 1024;
            else if (suf == L"TB") mult = 1000ULL * 1000 * 1000 * 1000;
            else FatalErr(std::wstring(L"unknown unit suffix '") + suf + L"' in " + what + L"=" + raw);
        }

        total *= val * mult;

        if (pos < raw.size() && raw[pos] == L'x') {
            pos++; // consume 'x' and parse the next factor in the chain
            continue;
        }
        break;
    }
    if (!any) FatalErr(std::wstring(L"invalid number in ") + what + L"=" + raw);
    return total;
}

// =============================================================================
// Options
// =============================================================================

struct Options {
    std::wstring inPath;
    std::wstring outPath;
    unsigned long long ibs = 512, obs = 512;
    long long skip = 0;
    long long seek = 0;
    long long count = -1;

    bool convNotrunc = false;
    bool convNoerror = false;
    bool convSync = false;
    bool convFsync = false;
    bool convFdatasync = false;
    bool convExclOut = false;

    bool iflagDirect = false;
    bool oflagDirect = false;
    bool iflagNoCache = false;
    bool oflagNoCache = false;
    bool iflagBitmap = false; // Windows extension: FSCTL_GET_VOLUME_BITMAP-driven sparse source read

    enum class Status { Default, None, Progress } status = Status::Default;
};

static void PrintUsageAndExit() {
    fwprintf(stdout,
        L"Usage: dd.exe [OPERAND]...\n"
        L"  if=FILE        read from FILE (or \\\\.\\PhysicalDriveN, \\\\.\\X:, or bare X:)\n"
        L"  of=FILE        write to FILE (or a device path, as above)\n"
        L"  bs=BYTES       set both ibs and obs\n"
        L"  ibs=BYTES      input block size (default 512)\n"
        L"  obs=BYTES      output block size (default 512)\n"
        L"  skip=N         skip N ibs-sized blocks at start of input\n"
        L"  seek=N         skip N obs-sized blocks at start of output\n"
        L"  count=N        copy only N ibs-sized input blocks\n"
        L"  conv=CONVS     notrunc,noerror,sync,fsync,fdatasync,excl\n"
        L"  iflag=FLAGS    direct,nocache,bitmap (bitmap: NTFS used-blocks-only source read)\n"
        L"  oflag=FLAGS    direct,nocache,excl\n"
        L"  status=LEVEL   none | progress\n"
        L"BYTES accepts suffixes c,w,b,K,k,kB,M,MB,G,GB,T,TB and 'x' chaining (e.g. 512x8).\n"
        L"Device access (\\\\.\\PhysicalDriveN, \\\\.\\X:, bare X:) requires an elevated\n"
        L"(Administrator) console; dd.exe attempts to enable SeManageVolumePrivilege.\n");
    exit(0);
}

static std::vector<std::wstring> SplitCommaList(const std::wstring& s) {
    std::vector<std::wstring> out;
    size_t start = 0;
    while (start <= s.size()) {
        size_t comma = s.find(L',', start);
        if (comma == std::wstring::npos) { out.push_back(s.substr(start)); break; }
        out.push_back(s.substr(start, comma - start));
        start = comma + 1;
    }
    return out;
}

static Options ParseArgs(int argc, wchar_t** argv) {
    Options o;
    for (int i = 1; i < argc; i++) {
        std::wstring a = argv[i];
        if (a == L"--help" || a == L"/?" || a == L"-h") PrintUsageAndExit();
        size_t eq = a.find(L'=');
        if (eq == std::wstring::npos) FatalErr(L"unrecognized operand '" + a + L"' (expected key=value)");
        std::wstring key = a.substr(0, eq);
        std::wstring val = a.substr(eq + 1);

        if (key == L"if") o.inPath = val;
        else if (key == L"of") o.outPath = val;
        else if (key == L"bs") { o.ibs = o.obs = ParseSize(val, L"bs"); }
        else if (key == L"ibs") { o.ibs = ParseSize(val, L"ibs"); }
        else if (key == L"obs") { o.obs = ParseSize(val, L"obs"); }
        else if (key == L"skip") o.skip = (long long)ParseSize(val, L"skip");
        else if (key == L"seek") o.seek = (long long)ParseSize(val, L"seek");
        else if (key == L"count") o.count = (long long)ParseSize(val, L"count");
        else if (key == L"status") {
            if (val == L"none") o.status = Options::Status::None;
            else if (val == L"progress") o.status = Options::Status::Progress;
            else FatalErr(L"unknown status=" + val);
        }
        else if (key == L"conv") {
            for (auto& c : SplitCommaList(val)) {
                if (c == L"notrunc") o.convNotrunc = true;
                else if (c == L"noerror") o.convNoerror = true;
                else if (c == L"sync") o.convSync = true;
                else if (c == L"fsync") o.convFsync = true;
                else if (c == L"fdatasync") o.convFdatasync = true;
                else if (c == L"excl") o.convExclOut = true;
                else if (c.empty()) continue;
                else FatalErr(L"unknown conv=" + c);
            }
        }
        else if (key == L"iflag") {
            for (auto& c : SplitCommaList(val)) {
                if (c == L"direct") o.iflagDirect = true;
                else if (c == L"nocache") o.iflagNoCache = true;
                else if (c == L"bitmap") o.iflagBitmap = true;
                else if (c.empty()) continue;
                else FatalErr(L"unknown iflag=" + c);
            }
        }
        else if (key == L"oflag") {
            for (auto& c : SplitCommaList(val)) {
                if (c == L"direct") o.oflagDirect = true;
                else if (c == L"nocache") o.oflagNoCache = true;
                else if (c == L"excl") o.convExclOut = true;
                else if (c.empty()) continue;
                else FatalErr(L"unknown oflag=" + c);
            }
        }
        else {
            FatalErr(L"unknown operand key '" + key + L"'");
        }
    }
    if (o.inPath.empty()) FatalErr(L"if= is required");
    if (o.outPath.empty()) FatalErr(L"of= is required");
    if (o.ibs == 0 || o.obs == 0) FatalErr(L"block size must be > 0");
    if (o.iflagBitmap && (o.skip != 0 || o.seek != 0))
        FatalErr(L"iflag=bitmap cannot be combined with skip=/seek= in this implementation");
    return o;
}

// =============================================================================
// Device path recognition & normalization (requirement #3)
// =============================================================================

static bool IsDriveLetterForm(const std::wstring& p) {
    return p.size() == 2 && iswalpha(p[0]) && p[1] == L':';
}

// Accepts: "E:"  ->  "\\.\E:"
//          "\\.\E:"      -> unchanged
//          "\\.\E:\"     -> "\\.\E:"   (trailing slash opens the root
//                                       *directory*, not the volume object)
//          "\\.\PhysicalDrive1", "\\?\Volume{...}" -> unchanged
static std::wstring NormalizeDevicePath(const std::wstring& raw) {
    if (IsDriveLetterForm(raw)) return L"\\\\.\\" + raw;

    const std::wstring devPrefix = L"\\\\.\\";
    if (raw.rfind(devPrefix, 0) == 0) {
        std::wstring rest = raw.substr(devPrefix.size());
        if (rest.size() == 3 && iswalpha(rest[0]) && rest[1] == L':' && (rest[2] == L'\\' || rest[2] == L'/')) {
            return devPrefix + rest.substr(0, 2);
        }
    }
    return raw;
}

static bool IsDevicePath(const std::wstring& p) {
    return p.rfind(L"\\\\.\\", 0) == 0 || p.rfind(L"\\\\?\\", 0) == 0;
}

static bool IsVolumeDevicePath(const std::wstring& p) {
    // "\\.\E:" exactly (device-namespace prefix + one drive letter + colon).
    return p.rfind(L"\\\\.\\", 0) == 0 && p.size() == 6 && iswalpha(p[4]) && p[5] == L':';
}

static std::optional<DWORD> ExtractPhysicalDriveNumber(const std::wstring& p) {
    const std::wstring prefix = L"\\\\.\\PhysicalDrive";
    if (p.rfind(prefix, 0) != 0) return std::nullopt;
    std::wstring digits = p.substr(prefix.size());
    if (digits.empty() || !std::all_of(digits.begin(), digits.end(), iswdigit)) return std::nullopt;
    return (DWORD)std::wcstoul(digits.c_str(), nullptr, 10);
}

// =============================================================================
// Volume enumeration & disk-number matching (requirement #5, whole-disk case)
// =============================================================================

static std::optional<DWORD> GetStorageDeviceNumber(HANDLE h) {
    STORAGE_DEVICE_NUMBER sdn{};
    DWORD ret = 0;
    if (!DeviceIoControl(h, IOCTL_STORAGE_GET_DEVICE_NUMBER, nullptr, 0, &sdn, sizeof(sdn), &ret, nullptr))
        return std::nullopt;
    return sdn.DeviceNumber;
}

static std::vector<std::wstring> EnumerateVolumeGuidPaths() {
    std::vector<std::wstring> result;
    wchar_t nameBuf[MAX_PATH] = {};
    HANDLE h = FindFirstVolumeW(nameBuf, ARRAYSIZE(nameBuf));
    if (h == INVALID_HANDLE_VALUE) return result;
    do {
        std::wstring guidPath(nameBuf);
        while (!guidPath.empty() && guidPath.back() == L'\\') guidPath.pop_back(); // strip trailing '\'
        result.push_back(guidPath);
    } while (FindNextVolumeW(h, nameBuf, ARRAYSIZE(nameBuf)));
    FindVolumeClose(h);
    return result;
}

struct VolumeBinding {
    UniqueHandle handle; // must be declared before `lock` (destruction order)
    VolumeLock lock;
    std::wstring path;
};

// Implements the documented algorithm for exclusive whole-disk access:
//   1) enumerate every volume in the system,
//   2) keep those whose underlying StorageDeviceNumber matches diskNumber,
//   3) open each with GENERIC_READ|GENERIC_WRITE + FILE_SHARE_READ|WRITE,
//   4) FSCTL_LOCK_VOLUME then FSCTL_DISMOUNT_VOLUME on each (same handle),
//   5) only then is it safe to write to \\.\PhysicalDriveN itself.
// Volumes that cannot be matched, opened, or locked are reported as
// warnings; the caller decides whether to proceed (raw writes will simply
// fail later with a clear Win32 error if exclusivity was not achieved).
static std::vector<VolumeBinding> LockAllVolumesOnDisk(DWORD diskNumber) {
    std::vector<VolumeBinding> bindings;
    for (const auto& guidPath : EnumerateVolumeGuidPaths()) {
        UniqueHandle probe(CreateFileW(guidPath.c_str(), 0, FILE_SHARE_READ | FILE_SHARE_WRITE,
            nullptr, OPEN_EXISTING, 0, nullptr));
        if (!probe.Valid()) continue;
        auto num = GetStorageDeviceNumber(probe.Get());
        probe.Reset();
        if (!num || *num != diskNumber) continue;

        UniqueHandle vh(CreateFileW(guidPath.c_str(), GENERIC_READ | GENERIC_WRITE,
            FILE_SHARE_READ | FILE_SHARE_WRITE, nullptr, OPEN_EXISTING, 0, nullptr));
        if (!vh.Valid()) {
            WarnWin32(L"could not open volume " + guidPath + L" for locking");
            continue;
        }
        VolumeLock lk(vh.Get(), /*alsoDismount=*/true);
        if (!lk.Locked()) {
            fwprintf(stderr, L"dd: warning: could not lock volume %ls: %ls\n",
                guidPath.c_str(), Win32ErrorText(lk.LastError()).c_str());
        }
        else if (lk.DismountFailed()) {
            fwprintf(stderr, L"dd: warning: locked but could not dismount volume %ls: %ls\n",
                guidPath.c_str(), Win32ErrorText(lk.LastError()).c_str());
        }
        bindings.push_back(VolumeBinding{ std::move(vh), std::move(lk), guidPath });
    }
    return bindings;
}

// =============================================================================
// Target abstraction
// =============================================================================

struct OpenedTarget {
    UniqueHandle handle;                     // the handle actually used for ReadFile/WriteFile
    VolumeLock ownLock;                      // lock held via `handle` itself (volume-path target)
    std::vector<VolumeBinding> childVolumes; // locks held on volumes residing on a whole-disk target
    bool isDevice = false;
    bool isVolumePath = false;
    DWORD sectorSize = 512;
};

static DWORD QuerySectorSize(HANDLE h) {
    DISK_GEOMETRY_EX geo{};
    DWORD ret = 0;
    if (DeviceIoControl(h, IOCTL_DISK_GET_DRIVE_GEOMETRY_EX, nullptr, 0, &geo, sizeof(geo), &ret, nullptr)) {
        if (geo.Geometry.BytesPerSector > 0) return geo.Geometry.BytesPerSector;
    }
    STORAGE_PROPERTY_QUERY q{};
    q.PropertyId = StorageAccessAlignmentProperty;
    q.QueryType = PropertyStandardQuery;
    STORAGE_ACCESS_ALIGNMENT_DESCRIPTOR align{};
    if (DeviceIoControl(h, IOCTL_STORAGE_QUERY_PROPERTY, &q, sizeof(q), &align, sizeof(align), &ret, nullptr)) {
        if (align.BytesPerLogicalSector > 0) return align.BytesPerLogicalSector;
    }
    return 512;
}

static OpenedTarget OpenTarget(const std::wstring& rawPath, bool forWrite, const Options& o) {
    OpenedTarget t;
    std::wstring path = NormalizeDevicePath(rawPath);
    t.isDevice = IsDevicePath(path);
    t.isVolumePath = IsVolumeDevicePath(path);
    auto physicalDriveNumber = t.isDevice ? ExtractPhysicalDriveNumber(path) : std::nullopt;

    DWORD access = forWrite ? (GENERIC_READ | GENERIC_WRITE) : GENERIC_READ;
    DWORD share = FILE_SHARE_READ | FILE_SHARE_WRITE;
    DWORD create = OPEN_EXISTING;
    if (!t.isDevice && forWrite) create = o.convNotrunc ? OPEN_ALWAYS : CREATE_ALWAYS;
    if (forWrite && o.convExclOut) share = 0;

    DWORD flags = FILE_ATTRIBUTE_NORMAL;
    bool wantDirect = forWrite ? o.oflagDirect : o.iflagDirect;
    bool wantNoCache = forWrite ? o.oflagNoCache : o.iflagNoCache;
    if (wantDirect) flags |= FILE_FLAG_NO_BUFFERING | FILE_FLAG_WRITE_THROUGH;
    else if (wantNoCache) flags |= FILE_FLAG_WRITE_THROUGH;

    // Requirement #5: for a whole-disk write target, every resident volume
    // must be located, locked and dismounted *before* we write through the
    // disk handle, per the documented algorithm. This does not depend on
    // t.handle, so it is done first.
    if (forWrite && physicalDriveNumber) {
        t.childVolumes = LockAllVolumesOnDisk(*physicalDriveNumber);
    }

    t.handle.Reset(CreateFileW(path.c_str(), access, share, nullptr, create, flags, nullptr));
    if (!t.handle.Valid()) {
        DWORD e = GetLastError();
        std::wstring hint;
        if (e == ERROR_ACCESS_DENIED && t.isDevice)
            hint = L" (raw device access requires an elevated/Administrator console)";
        FatalErr(L"failed to open '" + path + L"': " + Win32ErrorText(e) + hint);
    }

    // Requirement #4/#5: for a volume-path write target (\\.\X:), lock and
    // dismount *through this exact handle*, since NTFS only honours the
    // lock for the handle that acquired it.
    if (forWrite && t.isVolumePath) {
        t.ownLock = VolumeLock(t.handle.Get(), /*alsoDismount=*/true);
        if (!t.ownLock.Locked())
            FatalErr(L"could not lock volume '" + path + L"' for exclusive write: " +
                Win32ErrorText(t.ownLock.LastError()) +
                L" (system/paging volumes cannot be locked; close all handles to files on it)");
        if (t.ownLock.DismountFailed())
            fwprintf(stderr, L"dd: warning: volume locked but dismount failed: %ls\n",
                Win32ErrorText(t.ownLock.LastError()).c_str());
    }

    t.sectorSize = t.isDevice ? QuerySectorSize(t.handle.Get()) : 512;
    return t;
}

static bool SeekAbsolute(HANDLE h, long long offset) {
    LARGE_INTEGER li; li.QuadPart = offset;
    return SetFilePointerEx(h, li, nullptr, FILE_BEGIN) != 0;
}

// =============================================================================
// Requirement #6: FSCTL_GET_VOLUME_BITMAP -- used-clusters-only extents
// =============================================================================

struct ByteExtent { unsigned long long offset; unsigned long long length; };

struct NtfsGeometry {
    DWORD bytesPerCluster;
    unsigned long long totalClusters;
    unsigned long long volumeBytes;
};

static NtfsGeometry QueryNtfsGeometry(HANDLE h) {
    NTFS_VOLUME_DATA_BUFFER nv{};
    DWORD ret = 0;
    if (!DeviceIoControl(h, FSCTL_GET_NTFS_VOLUME_DATA, nullptr, 0, &nv, sizeof(nv), &ret, nullptr))
        FatalWin32(L"iflag=bitmap requires an NTFS source volume (FSCTL_GET_NTFS_VOLUME_DATA failed)");
    NtfsGeometry g{};
    g.bytesPerCluster = nv.BytesPerCluster;
    g.totalClusters = (unsigned long long)nv.TotalClusters.QuadPart;
    g.volumeBytes = g.totalClusters * g.bytesPerCluster;
    return g;
}

// Walks the whole volume bitmap (FSCTL_GET_VOLUME_BITMAP is called
// repeatedly, advancing StartingLcn, since one call may not cover the
// entire volume) and returns merged runs of contiguous allocated clusters,
// converted to byte extents.
static std::vector<ByteExtent> GetAllocatedExtents(HANDLE h, const NtfsGeometry& geo) {
    std::vector<ByteExtent> extents;
    constexpr size_t kBitmapChunkBits = 8ULL * 1024 * 1024; // 1 MiB of bitmap bytes per call = 8Mi clusters/call
    std::vector<unsigned char> raw(sizeof(VOLUME_BITMAP_BUFFER) - 1 + kBitmapChunkBits / 8);

    unsigned long long clusterCursor = 0;
    bool haveOpenRun = false;
    unsigned long long runStart = 0;

    while (clusterCursor < geo.totalClusters) {
        STARTING_LCN_INPUT_BUFFER in{};
        in.StartingLcn.QuadPart = (LONGLONG)clusterCursor;
        DWORD ret = 0;
        BOOL ok = DeviceIoControl(h, FSCTL_GET_VOLUME_BITMAP, &in, sizeof(in), raw.data(), (DWORD)raw.size(), &ret, nullptr);
        DWORD err = ok ? 0 : GetLastError();
        if (!ok && err != ERROR_MORE_DATA) FatalWin32(L"FSCTL_GET_VOLUME_BITMAP failed");

        auto* bmp = reinterpret_cast<VOLUME_BITMAP_BUFFER*>(raw.data());
        unsigned long long startLcn = (unsigned long long)bmp->StartingLcn.QuadPart;
        unsigned long long bitsReturned = (unsigned long long)bmp->BitmapSize.QuadPart;
        size_t bufferBitCapacity = (raw.size() - (sizeof(VOLUME_BITMAP_BUFFER) - 1)) * 8;
        unsigned long long bitsInThisBuffer = std::min<unsigned long long>(bitsReturned, bufferBitCapacity);

        for (unsigned long long i = 0; i < bitsInThisBuffer; i++) {
            unsigned long long lcn = startLcn + i;
            if (lcn >= geo.totalClusters) break;
            bool allocated = (bmp->Buffer[i / 8] >> (i % 8)) & 1;
            if (allocated && !haveOpenRun) { haveOpenRun = true; runStart = lcn; }
            else if (!allocated && haveOpenRun) {
                extents.push_back({ runStart * geo.bytesPerCluster, (lcn - runStart) * (unsigned long long)geo.bytesPerCluster });
                haveOpenRun = false;
            }
        }
        clusterCursor = startLcn + bitsInThisBuffer;
        if (bitsInThisBuffer == 0) break; // safety: avoid infinite loop on unexpected zero-progress reply
    }
    if (haveOpenRun) {
        extents.push_back({ runStart * geo.bytesPerCluster, (geo.totalClusters - runStart) * (unsigned long long)geo.bytesPerCluster });
    }
    return extents;
}

static void MarkSparse(HANDLE h) {
    DWORD junk = 0;
    DeviceIoControl(h, FSCTL_SET_SPARSE, nullptr, 0, nullptr, 0, &junk, nullptr);
}

// =============================================================================
// Stats / progress
// =============================================================================

struct Stats {
    unsigned long long recordsInFull = 0, recordsInPartial = 0;
    unsigned long long recordsOutFull = 0, recordsOutPartial = 0;
    unsigned long long bytesCopied = 0;
    unsigned long long logicalRangeBytes = 0; // for bitmap mode: total volume size represented
    std::chrono::steady_clock::time_point start;
};

static std::wstring HumanBytes(unsigned long long b) {
    const wchar_t* units[] = { L"B", L"KiB", L"MiB", L"GiB", L"TiB" };
    double v = (double)b;
    int u = 0;
    while (v >= 1024.0 && u < 4) { v /= 1024.0; u++; }
    wchar_t buf[64];
    swprintf(buf, 64, L"%.1f %ls", v, units[u]);
    return buf;
}

static void PrintFinalStats(const Stats& s) {
    auto elapsed = std::chrono::duration<double>(std::chrono::steady_clock::now() - s.start).count();
    if (elapsed <= 0) elapsed = 1e-6;
    double bps = s.bytesCopied / elapsed;
    fwprintf(stderr, L"%llu+%llu records in\n", s.recordsInFull, s.recordsInPartial);
    fwprintf(stderr, L"%llu+%llu records out\n", s.recordsOutFull, s.recordsOutPartial);
    fwprintf(stderr, L"%llu bytes (%ls) copied, %.3f s, %ls/s\n",
        s.bytesCopied, HumanBytes(s.bytesCopied).c_str(), elapsed, HumanBytes((unsigned long long)bps).c_str());
    if (s.logicalRangeBytes > 0) {
        double pct = 100.0 * (double)s.bytesCopied / (double)s.logicalRangeBytes;
        fwprintf(stderr, L"used blocks only: %ls of %ls logical volume size transferred (%.1f%%)\n",
            HumanBytes(s.bytesCopied).c_str(), HumanBytes(s.logicalRangeBytes).c_str(), pct);
    }
}

static void PrintProgressLine(const Stats& s) {
    auto elapsed = std::chrono::duration<double>(std::chrono::steady_clock::now() - s.start).count();
    if (elapsed <= 0) elapsed = 1e-6;
    double bps = s.bytesCopied / elapsed;
    fwprintf(stderr, L"\r%llu bytes (%ls) copied, %.1f s, %ls/s   ",
        s.bytesCopied, HumanBytes(s.bytesCopied).c_str(), elapsed, HumanBytes((unsigned long long)bps).c_str());
    fflush(stderr);
}

// =============================================================================
// Core copy engine
// =============================================================================

static bool WriteFull(HANDLE h, const void* buf, DWORD want) {
    const char* p = (const char*)buf;
    DWORD remaining = want;
    while (remaining > 0) {
        DWORD wrote = 0;
        if (!WriteFile(h, p, remaining, &wrote, nullptr)) return false;
        if (wrote == 0) return false;
        p += wrote;
        remaining -= wrote;
    }
    return true;
}

// Copies at most `limitBytes` bytes (nullopt == unlimited, governed instead
// by `limitBlocks` ibs-blocks / EOF) from `inH` to `outH`, re-blocking
// between ibs and obs. Positions in `inH`/`outH` are wherever the caller
// last left them (via SeekAbsolute) -- this function performs no seeking
// itself, so it can be called repeatedly for disjoint extents (bitmap mode)
// or once for a contiguous whole-stream copy.
static void CopyRange(HANDLE inH, HANDLE outH, const Options& o, size_t alignment,
    std::optional<unsigned long long> limitBytes, std::optional<long long> limitBlocks,
    Stats& stats, bool sectorPadFinalWrite, DWORD outSectorSize,
    std::chrono::steady_clock::time_point& lastProgress) {
    AlignedBuffer inBuf(o.ibs, alignment);
    std::vector<unsigned char> pending;
    pending.reserve(o.obs * 2);

    unsigned long long bytesReadThisRange = 0;
    long long blocksRead = 0;

    auto flush = [&](bool finalFlush) {
        size_t offset = 0;
        while (pending.size() - offset >= o.obs) {
            AlignedBuffer outBuf(o.obs, alignment);
            memcpy(outBuf.Data(), pending.data() + offset, o.obs);
            if (!WriteFull(outH, outBuf.Data(), (DWORD)o.obs)) FatalWin32(L"write error");
            stats.recordsOutFull++;
            stats.bytesCopied += o.obs;
            offset += o.obs;
        }
        if (offset > 0) pending.erase(pending.begin(), pending.begin() + offset);
        if (finalFlush && !pending.empty()) {
            if (sectorPadFinalWrite) {
                size_t padded = ((pending.size() + outSectorSize - 1) / outSectorSize) * outSectorSize;
                pending.resize(padded, 0);
            }
            AlignedBuffer outBuf(pending.size(), alignment);
            memcpy(outBuf.Data(), pending.data(), pending.size());
            if (!WriteFull(outH, outBuf.Data(), (DWORD)pending.size())) FatalWin32(L"write error");
            stats.recordsOutPartial++;
            stats.bytesCopied += pending.size();
            pending.clear();
        }
        };

    for (;;) {
        if (limitBlocks && blocksRead >= *limitBlocks) break;
        if (limitBytes && bytesReadThisRange >= *limitBytes) break;

        DWORD wantThisRead = (DWORD)o.ibs;
        if (limitBytes) {
            unsigned long long remaining = *limitBytes - bytesReadThisRange;
            wantThisRead = (DWORD)std::min<unsigned long long>(wantThisRead, remaining);
        }
        if (wantThisRead == 0) break;

        DWORD got = 0;
        BOOL ok = ReadFile(inH, inBuf.Data(), wantThisRead, &got, nullptr);
        if (!ok) {
            DWORD e = GetLastError();
            if (o.convNoerror) {
                fwprintf(stderr, L"dd: read error at block %lld: %ls\n", blocksRead, Win32ErrorText(e).c_str());
                if (o.convSync) { memset(inBuf.Data(), 0, o.ibs); got = (DWORD)o.ibs; }
                else got = 0;
                blocksRead++;
                bytesReadThisRange += got;
                if (got == 0) continue;
            }
            else {
                FatalWin32(L"read error at block " + std::to_wstring(blocksRead));
            }
        }
        else {
            if (got == 0) break; // EOF
            blocksRead++;
            bytesReadThisRange += got;
        }

        if (got == (DWORD)o.ibs) stats.recordsInFull++;
        else {
            stats.recordsInPartial++;
            if (o.convSync && !limitBytes) { // padding inside a byte-bounded extent would overshoot it
                memset((char*)inBuf.Data() + got, 0, o.ibs - got);
                got = (DWORD)o.ibs;
            }
        }

        pending.insert(pending.end(), (unsigned char*)inBuf.Data(), (unsigned char*)inBuf.Data() + got);
        flush(false);

        if (o.status == Options::Status::Progress) {
            auto now = std::chrono::steady_clock::now();
            if (std::chrono::duration<double>(now - lastProgress).count() >= 0.5) {
                PrintProgressLine(stats);
                lastProgress = now;
            }
        }
    }
    flush(true);
}

static int RunCopy(const Options& o) {
    OpenedTarget in = OpenTarget(o.inPath, false, o);
    OpenedTarget out = OpenTarget(o.outPath, true, o);

    size_t alignment = 8;
    if (o.iflagDirect) alignment = std::max<size_t>(alignment, in.sectorSize);
    if (o.oflagDirect) alignment = std::max<size_t>(alignment, out.sectorSize);

    if (o.iflagDirect && (o.ibs % in.sectorSize) != 0)
        FatalErr(L"ibs must be a multiple of the input sector size (" + std::to_wstring(in.sectorSize) + L") when iflag=direct");
    if (o.oflagDirect && (o.obs % out.sectorSize) != 0)
        FatalErr(L"obs must be a multiple of the output sector size (" + std::to_wstring(out.sectorSize) + L") when oflag=direct");

    Stats stats;
    stats.start = std::chrono::steady_clock::now();
    auto lastProgress = stats.start;
    bool sectorPad = out.isDevice && o.oflagDirect;

    if (o.iflagBitmap) {
        // ---- Requirement #6: used-clusters-only copy -----------------------
        if (!in.isVolumePath) FatalErr(L"iflag=bitmap requires if= to be a mounted volume (\\\\.\\X: or X:)");
        NtfsGeometry geo = QueryNtfsGeometry(in.handle.Get());
        fwprintf(stderr, L"dd: NTFS volume: %llu clusters x %lu bytes = %ls\n",
            geo.totalClusters, geo.bytesPerCluster, HumanBytes(geo.volumeBytes).c_str());
        auto extents = GetAllocatedExtents(in.handle.Get(), geo);
        unsigned long long usedBytes = 0;
        for (auto& e : extents) usedBytes += e.length;
        fwprintf(stderr, L"dd: %zu allocated extent(s), %ls used of %ls (%.1f%%)\n",
            extents.size(), HumanBytes(usedBytes).c_str(), HumanBytes(geo.volumeBytes).c_str(),
            100.0 * (double)usedBytes / (double)std::max<unsigned long long>(geo.volumeBytes, 1));
        stats.logicalRangeBytes = geo.volumeBytes;

        if (!out.isDevice) {
            // Pre-size + sparsify the destination file so unwritten gaps
            // between extents read back as zero without consuming space.
            LARGE_INTEGER size; size.QuadPart = (LONGLONG)geo.volumeBytes;
            if (!SetFilePointerEx(out.handle.Get(), size, nullptr, FILE_BEGIN) || !SetEndOfFile(out.handle.Get()))
                WarnWin32(L"could not pre-size output file to volume length");
            MarkSparse(out.handle.Get());
            SeekAbsolute(out.handle.Get(), 0);
        }

        for (auto& e : extents) {
            if (!SeekAbsolute(in.handle.Get(), (long long)e.offset)) FatalWin32(L"seek on input volume failed");
            if (!SeekAbsolute(out.handle.Get(), (long long)e.offset)) FatalWin32(L"seek on output failed");
            CopyRange(in.handle.Get(), out.handle.Get(), o, alignment, e.length, std::nullopt,
                stats, sectorPad, out.sectorSize, lastProgress);
        }
    }
    else {
        // ---- Whole-stream copy (standard dd semantics) ---------------------
        if (o.skip > 0) {
            long long off = o.skip * (long long)o.ibs;
            if (!SeekAbsolute(in.handle.Get(), off)) {
                AlignedBuffer discard(o.ibs, alignment);
                long long remaining = off;
                while (remaining > 0) {
                    DWORD chunk = (DWORD)std::min<long long>(remaining, (long long)o.ibs);
                    DWORD got = 0;
                    if (!ReadFile(in.handle.Get(), discard.Data(), chunk, &got, nullptr) || got == 0) break;
                    remaining -= got;
                }
            }
        }
        if (o.seek > 0) {
            long long off = o.seek * (long long)o.obs;
            if (!SeekAbsolute(out.handle.Get(), off)) FatalErr(L"seek= requested but output is not seekable");
        }

        std::optional<long long> limitBlocks = (o.count >= 0) ? std::optional<long long>(o.count) : std::nullopt;
        CopyRange(in.handle.Get(), out.handle.Get(), o, alignment, std::nullopt, limitBlocks,
            stats, sectorPad, out.sectorSize, lastProgress);
    }

    if (o.convFsync || o.convFdatasync) {
        if (!FlushFileBuffers(out.handle.Get())) WarnWin32(L"FlushFileBuffers failed");
    }

    if (!out.isDevice && !o.convNotrunc && !o.iflagBitmap) {
        LARGE_INTEGER cur{};
        if (SetFilePointerEx(out.handle.Get(), cur, &cur, FILE_CURRENT)) SetEndOfFile(out.handle.Get());
    }

    if (o.status == Options::Status::Progress) fwprintf(stderr, L"\n");
    // `in`/`out` (and their VolumeLock/VolumeBinding members) unlock and
    // close automatically here via RAII as they go out of scope.
    if (o.status != Options::Status::None) PrintFinalStats(stats);
    return 0;
}

// =============================================================================
// Entry point
// =============================================================================

int wmain(int argc, wchar_t** argv) {
    if (argc <= 1) PrintUsageAndExit();
    Options o = ParseArgs(argc, argv);

    // Requirement #1: enable SeManageVolumePrivilege for the lifetime of the
    // process. Locking/dismounting/writing raw volumes needs it; if it
    // cannot be enabled (non-elevated console, or the privilege was
    // stripped from the token) we proceed anyway and let the specific
    // FSCTL/CreateFile call fail with an explicit, actionable error instead
    // of guessing up front whether the whole operation is device-related.
    PrivilegeEnabler manageVolume(SE_MANAGE_VOLUME_NAME);
    if (!manageVolume.Ok() && (IsDevicePath(NormalizeDevicePath(o.inPath)) || IsDevicePath(NormalizeDevicePath(o.outPath)))) {
        fwprintf(stderr,
            L"dd: warning: could not enable SeManageVolumePrivilege (%ls); "
            L"volume lock/dismount will likely fail unless this console is running as Administrator\n",
            Win32ErrorText(manageVolume.LastError()).c_str());
    }

    try {
        return RunCopy(o);
    }
    catch (const std::exception& ex) {
        FatalErr(Widen(std::string("unhandled exception: ") + ex.what()));
        return 1;
    }
}