#pragma once

// --- Спецификация формата разреженного контейнера ---
#pragma pack(push, 1)
constexpr uint64_t IMAGE_MAGIC = 0x564F4C554D494D47ULL; // "VOLUMIMG"
constexpr uint32_t IMAGE_VERSION = 2;

enum class BlockType : uint32_t {
    ClusterExtent = 1,
    RawByteOffset = 2,
    EndOfStream = 0xFFFFFFFF
};

struct ImageHeader {
    uint64_t Magic;
    uint32_t Version;
    uint32_t BytesPerSector;
    uint32_t SectorsPerCluster;
    uint64_t TotalClusters;
    uint64_t VolumeLengthBytes;
};

struct BlockRecordHeader {
    BlockType Type;
    uint64_t  TargetOffset; // Абсолютное смещение на томе в байтах
    uint64_t  DataSize;     // Размер полезной нагрузки в байтах
};
#pragma pack(pop)

// RAII-обертка для выровненной памяти (Direct I/O)
class AlignedBuffer {
public:
    explicit AlignedBuffer(size_t size) : size_(size) {
        ptr_ = static_cast<uint8_t*>(VirtualAlloc(nullptr, size_, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE));
    }
    ~AlignedBuffer() {
        if (ptr_) VirtualFree(ptr_, 0, MEM_RELEASE);
    }
    AlignedBuffer(const AlignedBuffer&) = delete;
    AlignedBuffer& operator=(const AlignedBuffer&) = delete;

    uint8_t* get() const { return ptr_; }
    size_t size() const { return size_; }

private:
    uint8_t* ptr_ = nullptr;
    size_t size_ = 0;
};

// RAII-обертка для Win32 Handle
class SafeHandle {
public:
    SafeHandle(HANDLE h = INVALID_HANDLE_VALUE) : handle_(h) {}
    ~SafeHandle() { close(); }

    void close() {
        if (handle_ != INVALID_HANDLE_VALUE && handle_ != nullptr) {
            CloseHandle(handle_);
            handle_ = INVALID_HANDLE_VALUE;
        }
    }

    HANDLE get() const { return handle_; }
    bool isValid() const { return handle_ != INVALID_HANDLE_VALUE; }
    HANDLE* addressof() { return &handle_; }

    SafeHandle(const SafeHandle&) = delete;
    SafeHandle& operator=(const SafeHandle&) = delete;
    SafeHandle(SafeHandle&& other) noexcept : handle_(other.handle_) { other.handle_ = INVALID_HANDLE_VALUE; }
    SafeHandle& operator=(SafeHandle&& other) noexcept {
        if (this != &other) {
            close();
            handle_ = other.handle_;
            other.handle_ = INVALID_HANDLE_VALUE;
        }
        return *this;
    }

private:
    HANDLE handle_;
};

