using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Managed.Win32.Native.Kernel32
{
    public partial struct DISK_SPACE_INFORMATION
    {
        [NativeTypeName("ULONGLONG")]
        public ulong ActualTotalAllocationUnits;

        [NativeTypeName("ULONGLONG")]
        public ulong ActualAvailableAllocationUnits;

        [NativeTypeName("ULONGLONG")]
        public ulong ActualPoolUnavailableAllocationUnits;

        [NativeTypeName("ULONGLONG")]
        public ulong CallerTotalAllocationUnits;

        [NativeTypeName("ULONGLONG")]
        public ulong CallerAvailableAllocationUnits;

        [NativeTypeName("ULONGLONG")]
        public ulong CallerPoolUnavailableAllocationUnits;

        [NativeTypeName("ULONGLONG")]
        public ulong UsedAllocationUnits;

        [NativeTypeName("ULONGLONG")]
        public ulong TotalReservedAllocationUnits;

        [NativeTypeName("ULONGLONG")]
        public ulong VolumeStorageReserveAllocationUnits;

        [NativeTypeName("ULONGLONG")]
        public ulong AvailableCommittedAllocationUnits;

        [NativeTypeName("ULONGLONG")]
        public ulong PoolAvailableAllocationUnits;

        [NativeTypeName("DWORD")]
        public uint SectorsPerAllocationUnit;

        [NativeTypeName("DWORD")]
        public uint BytesPerSector;
    }

    public partial struct _WIN32_FILE_ATTRIBUTE_DATA
    {
        [NativeTypeName("DWORD")]
        public uint dwFileAttributes;

        public FILETIME ftCreationTime;

        public FILETIME ftLastAccessTime;

        public FILETIME ftLastWriteTime;

        [NativeTypeName("DWORD")]
        public uint nFileSizeHigh;

        [NativeTypeName("DWORD")]
        public uint nFileSizeLow;
    }

    public partial struct _BY_HANDLE_FILE_INFORMATION
    {
        [NativeTypeName("DWORD")]
        public uint dwFileAttributes;

        public FILETIME ftCreationTime;

        public FILETIME ftLastAccessTime;

        public FILETIME ftLastWriteTime;

        [NativeTypeName("DWORD")]
        public uint dwVolumeSerialNumber;

        [NativeTypeName("DWORD")]
        public uint nFileSizeHigh;

        [NativeTypeName("DWORD")]
        public uint nFileSizeLow;

        [NativeTypeName("DWORD")]
        public uint nNumberOfLinks;

        [NativeTypeName("DWORD")]
        public uint nFileIndexHigh;

        [NativeTypeName("DWORD")]
        public uint nFileIndexLow;
    }

    public unsafe partial struct _CREATEFILE2_EXTENDED_PARAMETERS
    {
        [NativeTypeName("DWORD")]
        public uint dwSize;

        [NativeTypeName("DWORD")]
        public uint dwFileAttributes;

        [NativeTypeName("DWORD")]
        public uint dwFileFlags;

        [NativeTypeName("DWORD")]
        public uint dwSecurityQosFlags;

        [NativeTypeName("LPSECURITY_ATTRIBUTES")]
        public SECURITY_ATTRIBUTES* lpSecurityAttributes;

        [NativeTypeName("HANDLE")]
        public void* hTemplateFile;
    }

    public enum _STREAM_INFO_LEVELS
    {
        FindStreamInfoStandard,
        FindStreamInfoMaxInfoLevel,
    }

    public partial struct _WIN32_FIND_STREAM_DATA
    {
        [NativeTypeName("LARGE_INTEGER")]
        public long StreamSize;

        [NativeTypeName("WCHAR[296]")]
        public _cStreamName_e__FixedBuffer cStreamName;

        [InlineArray(296)]
        public partial struct _cStreamName_e__FixedBuffer
        {
            public ushort e0;
        }
    }

    public unsafe partial struct _CREATEFILE3_EXTENDED_PARAMETERS
    {
        [NativeTypeName("DWORD")]
        public uint dwSize;

        [NativeTypeName("DWORD")]
        public uint dwFileAttributes;

        [NativeTypeName("DWORD")]
        public uint dwFileFlags;

        [NativeTypeName("DWORD")]
        public uint dwSecurityQosFlags;

        [NativeTypeName("LPSECURITY_ATTRIBUTES")]
        public SECURITY_ATTRIBUTES* lpSecurityAttributes;

        [NativeTypeName("HANDLE")]
        public void* hTemplateFile;
    }

    public enum DIRECTORY_FLAGS
    {
        DIRECTORY_FLAGS_NONE = 0,
        DIRECTORY_FLAGS_DISALLOW_PATH_REDIRECTS = 0x000000001,
    }

    public unsafe partial struct _PROCESS_INFORMATION
    {
        [NativeTypeName("HANDLE")]
        public void* hProcess;

        [NativeTypeName("HANDLE")]
        public void* hThread;

        [NativeTypeName("DWORD")]
        public uint dwProcessId;

        [NativeTypeName("DWORD")]
        public uint dwThreadId;
    }

    public unsafe partial struct _STARTUPINFOA
    {
        [NativeTypeName("DWORD")]
        public uint cb;

        [NativeTypeName("LPSTR")]
        public sbyte* lpReserved;

        [NativeTypeName("LPSTR")]
        public sbyte* lpDesktop;

        [NativeTypeName("LPSTR")]
        public sbyte* lpTitle;

        [NativeTypeName("DWORD")]
        public uint dwX;

        [NativeTypeName("DWORD")]
        public uint dwY;

        [NativeTypeName("DWORD")]
        public uint dwXSize;

        [NativeTypeName("DWORD")]
        public uint dwYSize;

        [NativeTypeName("DWORD")]
        public uint dwXCountChars;

        [NativeTypeName("DWORD")]
        public uint dwYCountChars;

        [NativeTypeName("DWORD")]
        public uint dwFillAttribute;

        [NativeTypeName("DWORD")]
        public uint dwFlags;

        [NativeTypeName("WORD")]
        public ushort wShowWindow;

        [NativeTypeName("WORD")]
        public ushort cbReserved2;

        [NativeTypeName("LPBYTE")]
        public byte* lpReserved2;

        [NativeTypeName("HANDLE")]
        public void* hStdInput;

        [NativeTypeName("HANDLE")]
        public void* hStdOutput;

        [NativeTypeName("HANDLE")]
        public void* hStdError;
    }

    public unsafe partial struct _STARTUPINFOW
    {
        [NativeTypeName("DWORD")]
        public uint cb;

        [NativeTypeName("LPWSTR")]
        public ushort* lpReserved;

        [NativeTypeName("LPWSTR")]
        public ushort* lpDesktop;

        [NativeTypeName("LPWSTR")]
        public ushort* lpTitle;

        [NativeTypeName("DWORD")]
        public uint dwX;

        [NativeTypeName("DWORD")]
        public uint dwY;

        [NativeTypeName("DWORD")]
        public uint dwXSize;

        [NativeTypeName("DWORD")]
        public uint dwYSize;

        [NativeTypeName("DWORD")]
        public uint dwXCountChars;

        [NativeTypeName("DWORD")]
        public uint dwYCountChars;

        [NativeTypeName("DWORD")]
        public uint dwFillAttribute;

        [NativeTypeName("DWORD")]
        public uint dwFlags;

        [NativeTypeName("WORD")]
        public ushort wShowWindow;

        [NativeTypeName("WORD")]
        public ushort cbReserved2;

        [NativeTypeName("LPBYTE")]
        public byte* lpReserved2;

        [NativeTypeName("HANDLE")]
        public void* hStdInput;

        [NativeTypeName("HANDLE")]
        public void* hStdOutput;

        [NativeTypeName("HANDLE")]
        public void* hStdError;
    }

    public enum _QUEUE_USER_APC_FLAGS
    {
        QUEUE_USER_APC_FLAGS_NONE = 0x00000000,
        QUEUE_USER_APC_FLAGS_SPECIAL_USER_APC = 0x00000001,
        QUEUE_USER_APC_CALLBACK_DATA_CONTEXT = 0x00010000,
    }

    public unsafe partial struct _APC_CALLBACK_DATA
    {
        [NativeTypeName("ULONG_PTR")]
        public nuint Parameter;

        [NativeTypeName("PCONTEXT")]
        public _CONTEXT* ContextRecord;

        [NativeTypeName("ULONG_PTR")]
        public nuint Reserved0;

        [NativeTypeName("ULONG_PTR")]
        public nuint Reserved1;
    }

    public enum _THREAD_INFORMATION_CLASS
    {
        ThreadMemoryPriority,
        ThreadAbsoluteCpuPriority,
        ThreadDynamicCodePolicy,
        ThreadPowerThrottling,
        ThreadInformationClassMax,
    }

    public partial struct _MEMORY_PRIORITY_INFORMATION
    {
        [NativeTypeName("ULONG")]
        public uint MemoryPriority;
    }

    public partial struct _THREAD_POWER_THROTTLING_STATE
    {
        [NativeTypeName("ULONG")]
        public uint Version;

        [NativeTypeName("ULONG")]
        public uint ControlMask;

        [NativeTypeName("ULONG")]
        public uint StateMask;
    }

    public enum _PROCESS_INFORMATION_CLASS
    {
        ProcessMemoryPriority,
        ProcessMemoryExhaustionInfo,
        ProcessAppMemoryInfo,
        ProcessInPrivateInfo,
        ProcessPowerThrottling,
        ProcessReservedValue1,
        ProcessTelemetryCoverageInfo,
        ProcessProtectionLevelInfo,
        ProcessLeapSecondInfo,
        ProcessMachineTypeInfo,
        ProcessOverrideSubsequentPrefetchParameter,
        ProcessMaxOverridePrefetchParameter,
        ProcessInformationClassMax,
    }

    public partial struct _APP_MEMORY_INFORMATION
    {
        [NativeTypeName("ULONG64")]
        public ulong AvailableCommit;

        [NativeTypeName("ULONG64")]
        public ulong PrivateCommitUsage;

        [NativeTypeName("ULONG64")]
        public ulong PeakPrivateCommitUsage;

        [NativeTypeName("ULONG64")]
        public ulong TotalCommitUsage;
    }

    public enum _MACHINE_ATTRIBUTES
    {
        UserEnabled = 0x00000001,
        KernelEnabled = 0x00000002,
        Wow64Container = 0x00000004,
    }

    public partial struct _PROCESS_MACHINE_INFORMATION
    {
        public ushort ProcessMachine;

        public ushort Res0;

        [NativeTypeName("MACHINE_ATTRIBUTES")]
        public _MACHINE_ATTRIBUTES MachineAttributes;
    }

    public partial struct OVERRIDE_PREFETCH_PARAMETER
    {
        [NativeTypeName("UINT32")]
        public uint Value;
    }

    public enum _PROCESS_MEMORY_EXHAUSTION_TYPE
    {
        PMETypeFailFastOnCommitFailure,
        PMETypeMax,
    }

    public partial struct _PROCESS_MEMORY_EXHAUSTION_INFO
    {
        public ushort Version;

        public ushort Reserved;

        [NativeTypeName("PROCESS_MEMORY_EXHAUSTION_TYPE")]
        public _PROCESS_MEMORY_EXHAUSTION_TYPE Type;

        [NativeTypeName("ULONG_PTR")]
        public nuint Value;
    }

    public partial struct _PROCESS_POWER_THROTTLING_STATE
    {
        [NativeTypeName("ULONG")]
        public uint Version;

        [NativeTypeName("ULONG")]
        public uint ControlMask;

        [NativeTypeName("ULONG")]
        public uint StateMask;
    }

    public partial struct PROCESS_PROTECTION_LEVEL_INFORMATION
    {
        [NativeTypeName("DWORD")]
        public uint ProtectionLevel;
    }

    public partial struct _PROCESS_LEAP_SECOND_INFO
    {
        [NativeTypeName("ULONG")]
        public uint Flags;

        [NativeTypeName("ULONG")]
        public uint Reserved;
    }

    public static unsafe partial class Methods
    {
        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("LONG")]
        public static extern int CompareFileTime([NativeTypeName("const FILETIME *")] FILETIME* lpFileTime1, [NativeTypeName("const FILETIME *")] FILETIME* lpFileTime2);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int CreateDirectoryA([NativeTypeName("LPCSTR")] sbyte* lpPathName, [NativeTypeName("LPSECURITY_ATTRIBUTES")] SECURITY_ATTRIBUTES* lpSecurityAttributes);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int CreateDirectoryW([NativeTypeName("LPCWSTR")] ushort* lpPathName, [NativeTypeName("LPSECURITY_ATTRIBUTES")] SECURITY_ATTRIBUTES* lpSecurityAttributes);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* CreateFileA([NativeTypeName("LPCSTR")] sbyte* lpFileName, [NativeTypeName("DWORD")] uint dwDesiredAccess, [NativeTypeName("DWORD")] uint dwShareMode, [NativeTypeName("LPSECURITY_ATTRIBUTES")] SECURITY_ATTRIBUTES* lpSecurityAttributes, [NativeTypeName("DWORD")] uint dwCreationDisposition, [NativeTypeName("DWORD")] uint dwFlagsAndAttributes, [NativeTypeName("HANDLE")] void* hTemplateFile);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* CreateFileW([NativeTypeName("LPCWSTR")] ushort* lpFileName, [NativeTypeName("DWORD")] uint dwDesiredAccess, [NativeTypeName("DWORD")] uint dwShareMode, [NativeTypeName("LPSECURITY_ATTRIBUTES")] SECURITY_ATTRIBUTES* lpSecurityAttributes, [NativeTypeName("DWORD")] uint dwCreationDisposition, [NativeTypeName("DWORD")] uint dwFlagsAndAttributes, [NativeTypeName("HANDLE")] void* hTemplateFile);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int DefineDosDeviceW([NativeTypeName("DWORD")] uint dwFlags, [NativeTypeName("LPCWSTR")] ushort* lpDeviceName, [NativeTypeName("LPCWSTR")] ushort* lpTargetPath);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int DeleteFileA([NativeTypeName("LPCSTR")] sbyte* lpFileName);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int DeleteFileW([NativeTypeName("LPCWSTR")] ushort* lpFileName);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int DeleteVolumeMountPointW([NativeTypeName("LPCWSTR")] ushort* lpszVolumeMountPoint);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int FileTimeToLocalFileTime([NativeTypeName("const FILETIME *")] FILETIME* lpFileTime, [NativeTypeName("LPFILETIME")] FILETIME* lpLocalFileTime);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int FindClose([NativeTypeName("HANDLE")] void* hFindFile);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int FindCloseChangeNotification([NativeTypeName("HANDLE")] void* hChangeHandle);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* FindFirstChangeNotificationA([NativeTypeName("LPCSTR")] sbyte* lpPathName, [NativeTypeName("BOOL")] int bWatchSubtree, [NativeTypeName("DWORD")] uint dwNotifyFilter);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* FindFirstChangeNotificationW([NativeTypeName("LPCWSTR")] ushort* lpPathName, [NativeTypeName("BOOL")] int bWatchSubtree, [NativeTypeName("DWORD")] uint dwNotifyFilter);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* FindFirstFileA([NativeTypeName("LPCSTR")] sbyte* lpFileName, [NativeTypeName("LPWIN32_FIND_DATAA")] WIN32_FIND_DATAA* lpFindFileData);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* FindFirstFileW([NativeTypeName("LPCWSTR")] ushort* lpFileName, [NativeTypeName("LPWIN32_FIND_DATAW")] WIN32_FIND_DATAW* lpFindFileData);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* FindFirstFileExA([NativeTypeName("LPCSTR")] sbyte* lpFileName, [NativeTypeName("FINDEX_INFO_LEVELS")] _FINDEX_INFO_LEVELS fInfoLevelId, [NativeTypeName("LPVOID")] void* lpFindFileData, [NativeTypeName("FINDEX_SEARCH_OPS")] _FINDEX_SEARCH_OPS fSearchOp, [NativeTypeName("LPVOID")] void* lpSearchFilter, [NativeTypeName("DWORD")] uint dwAdditionalFlags);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* FindFirstFileExW([NativeTypeName("LPCWSTR")] ushort* lpFileName, [NativeTypeName("FINDEX_INFO_LEVELS")] _FINDEX_INFO_LEVELS fInfoLevelId, [NativeTypeName("LPVOID")] void* lpFindFileData, [NativeTypeName("FINDEX_SEARCH_OPS")] _FINDEX_SEARCH_OPS fSearchOp, [NativeTypeName("LPVOID")] void* lpSearchFilter, [NativeTypeName("DWORD")] uint dwAdditionalFlags);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* FindFirstVolumeW([NativeTypeName("LPWSTR")] ushort* lpszVolumeName, [NativeTypeName("DWORD")] uint cchBufferLength);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int FindNextChangeNotification([NativeTypeName("HANDLE")] void* hChangeHandle);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int FindNextFileA([NativeTypeName("HANDLE")] void* hFindFile, [NativeTypeName("LPWIN32_FIND_DATAA")] WIN32_FIND_DATAA* lpFindFileData);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int FindNextFileW([NativeTypeName("HANDLE")] void* hFindFile, [NativeTypeName("LPWIN32_FIND_DATAW")] WIN32_FIND_DATAW* lpFindFileData);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int FindNextVolumeW([NativeTypeName("HANDLE")] void* hFindVolume, [NativeTypeName("LPWSTR")] ushort* lpszVolumeName, [NativeTypeName("DWORD")] uint cchBufferLength);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int FindVolumeClose([NativeTypeName("HANDLE")] void* hFindVolume);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int FlushFileBuffers([NativeTypeName("HANDLE")] void* hFile);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetDiskFreeSpaceA([NativeTypeName("LPCSTR")] sbyte* lpRootPathName, [NativeTypeName("LPDWORD")] uint* lpSectorsPerCluster, [NativeTypeName("LPDWORD")] uint* lpBytesPerSector, [NativeTypeName("LPDWORD")] uint* lpNumberOfFreeClusters, [NativeTypeName("LPDWORD")] uint* lpTotalNumberOfClusters);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetDiskFreeSpaceW([NativeTypeName("LPCWSTR")] ushort* lpRootPathName, [NativeTypeName("LPDWORD")] uint* lpSectorsPerCluster, [NativeTypeName("LPDWORD")] uint* lpBytesPerSector, [NativeTypeName("LPDWORD")] uint* lpNumberOfFreeClusters, [NativeTypeName("LPDWORD")] uint* lpTotalNumberOfClusters);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetDiskFreeSpaceExA([NativeTypeName("LPCSTR")] sbyte* lpDirectoryName, [NativeTypeName("PULARGE_INTEGER")] ulong* lpFreeBytesAvailableToCaller, [NativeTypeName("PULARGE_INTEGER")] ulong* lpTotalNumberOfBytes, [NativeTypeName("PULARGE_INTEGER")] ulong* lpTotalNumberOfFreeBytes);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetDiskFreeSpaceExW([NativeTypeName("LPCWSTR")] ushort* lpDirectoryName, [NativeTypeName("PULARGE_INTEGER")] ulong* lpFreeBytesAvailableToCaller, [NativeTypeName("PULARGE_INTEGER")] ulong* lpTotalNumberOfBytes, [NativeTypeName("PULARGE_INTEGER")] ulong* lpTotalNumberOfFreeBytes);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HRESULT")]
        public static extern int GetDiskSpaceInformationA([NativeTypeName("LPCSTR")] sbyte* rootPath, DISK_SPACE_INFORMATION* diskSpaceInfo);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HRESULT")]
        public static extern int GetDiskSpaceInformationW([NativeTypeName("LPCWSTR")] ushort* rootPath, DISK_SPACE_INFORMATION* diskSpaceInfo);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern uint GetDriveTypeA([NativeTypeName("LPCSTR")] sbyte* lpRootPathName);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern uint GetDriveTypeW([NativeTypeName("LPCWSTR")] ushort* lpRootPathName);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetFileAttributesA([NativeTypeName("LPCSTR")] sbyte* lpFileName);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetFileAttributesW([NativeTypeName("LPCWSTR")] ushort* lpFileName);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetFileAttributesExA([NativeTypeName("LPCSTR")] sbyte* lpFileName, [NativeTypeName("GET_FILEEX_INFO_LEVELS")] _GET_FILEEX_INFO_LEVELS fInfoLevelId, [NativeTypeName("LPVOID")] void* lpFileInformation);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetFileAttributesExW([NativeTypeName("LPCWSTR")] ushort* lpFileName, [NativeTypeName("GET_FILEEX_INFO_LEVELS")] _GET_FILEEX_INFO_LEVELS fInfoLevelId, [NativeTypeName("LPVOID")] void* lpFileInformation);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetFileInformationByHandle([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("LPBY_HANDLE_FILE_INFORMATION")] _BY_HANDLE_FILE_INFORMATION* lpFileInformation);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetFileSize([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("LPDWORD")] uint* lpFileSizeHigh);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetFileSizeEx([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("PLARGE_INTEGER")] long* lpFileSize);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetFileType([NativeTypeName("HANDLE")] void* hFile);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetFinalPathNameByHandleA([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("LPSTR")] sbyte* lpszFilePath, [NativeTypeName("DWORD")] uint cchFilePath, [NativeTypeName("DWORD")] uint dwFlags);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetFinalPathNameByHandleW([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("LPWSTR")] ushort* lpszFilePath, [NativeTypeName("DWORD")] uint cchFilePath, [NativeTypeName("DWORD")] uint dwFlags);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetFileTime([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("LPFILETIME")] FILETIME* lpCreationTime, [NativeTypeName("LPFILETIME")] FILETIME* lpLastAccessTime, [NativeTypeName("LPFILETIME")] FILETIME* lpLastWriteTime);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetFullPathNameW([NativeTypeName("LPCWSTR")] ushort* lpFileName, [NativeTypeName("DWORD")] uint nBufferLength, [NativeTypeName("LPWSTR")] ushort* lpBuffer, [NativeTypeName("LPWSTR *")] ushort** lpFilePart);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetFullPathNameA([NativeTypeName("LPCSTR")] sbyte* lpFileName, [NativeTypeName("DWORD")] uint nBufferLength, [NativeTypeName("LPSTR")] sbyte* lpBuffer, [NativeTypeName("LPSTR *")] sbyte** lpFilePart);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetLogicalDrives();

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetLogicalDriveStringsW([NativeTypeName("DWORD")] uint nBufferLength, [NativeTypeName("LPWSTR")] ushort* lpBuffer);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetLongPathNameA([NativeTypeName("LPCSTR")] sbyte* lpszShortPath, [NativeTypeName("LPSTR")] sbyte* lpszLongPath, [NativeTypeName("DWORD")] uint cchBuffer);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetLongPathNameW([NativeTypeName("LPCWSTR")] ushort* lpszShortPath, [NativeTypeName("LPWSTR")] ushort* lpszLongPath, [NativeTypeName("DWORD")] uint cchBuffer);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int AreShortNamesEnabled([NativeTypeName("HANDLE")] void* Handle, [NativeTypeName("BOOL *")] int* Enabled);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetShortPathNameW([NativeTypeName("LPCWSTR")] ushort* lpszLongPath, [NativeTypeName("LPWSTR")] ushort* lpszShortPath, [NativeTypeName("DWORD")] uint cchBuffer);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern uint GetTempFileNameW([NativeTypeName("LPCWSTR")] ushort* lpPathName, [NativeTypeName("LPCWSTR")] ushort* lpPrefixString, uint uUnique, [NativeTypeName("LPWSTR")] ushort* lpTempFileName);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetVolumeInformationByHandleW([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("LPWSTR")] ushort* lpVolumeNameBuffer, [NativeTypeName("DWORD")] uint nVolumeNameSize, [NativeTypeName("LPDWORD")] uint* lpVolumeSerialNumber, [NativeTypeName("LPDWORD")] uint* lpMaximumComponentLength, [NativeTypeName("LPDWORD")] uint* lpFileSystemFlags, [NativeTypeName("LPWSTR")] ushort* lpFileSystemNameBuffer, [NativeTypeName("DWORD")] uint nFileSystemNameSize);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetVolumeInformationW([NativeTypeName("LPCWSTR")] ushort* lpRootPathName, [NativeTypeName("LPWSTR")] ushort* lpVolumeNameBuffer, [NativeTypeName("DWORD")] uint nVolumeNameSize, [NativeTypeName("LPDWORD")] uint* lpVolumeSerialNumber, [NativeTypeName("LPDWORD")] uint* lpMaximumComponentLength, [NativeTypeName("LPDWORD")] uint* lpFileSystemFlags, [NativeTypeName("LPWSTR")] ushort* lpFileSystemNameBuffer, [NativeTypeName("DWORD")] uint nFileSystemNameSize);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetVolumePathNameW([NativeTypeName("LPCWSTR")] ushort* lpszFileName, [NativeTypeName("LPWSTR")] ushort* lpszVolumePathName, [NativeTypeName("DWORD")] uint cchBufferLength);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int LocalFileTimeToFileTime([NativeTypeName("const FILETIME *")] FILETIME* lpLocalFileTime, [NativeTypeName("LPFILETIME")] FILETIME* lpFileTime);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int LockFile([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("DWORD")] uint dwFileOffsetLow, [NativeTypeName("DWORD")] uint dwFileOffsetHigh, [NativeTypeName("DWORD")] uint nNumberOfBytesToLockLow, [NativeTypeName("DWORD")] uint nNumberOfBytesToLockHigh);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int LockFileEx([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("DWORD")] uint dwFlags, [NativeTypeName("DWORD")] uint dwReserved, [NativeTypeName("DWORD")] uint nNumberOfBytesToLockLow, [NativeTypeName("DWORD")] uint nNumberOfBytesToLockHigh, [NativeTypeName("LPOVERLAPPED")] OVERLAPPED* lpOverlapped);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint QueryDosDeviceW([NativeTypeName("LPCWSTR")] ushort* lpDeviceName, [NativeTypeName("LPWSTR")] ushort* lpTargetPath, [NativeTypeName("DWORD")] uint ucchMax);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int ReadFile([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("LPVOID")] void* lpBuffer, [NativeTypeName("DWORD")] uint nNumberOfBytesToRead, [NativeTypeName("LPDWORD")] uint* lpNumberOfBytesRead, [NativeTypeName("LPOVERLAPPED")] OVERLAPPED* lpOverlapped);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int ReadFileEx([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("LPVOID")] void* lpBuffer, [NativeTypeName("DWORD")] uint nNumberOfBytesToRead, [NativeTypeName("LPOVERLAPPED")] OVERLAPPED* lpOverlapped, [NativeTypeName("LPOVERLAPPED_COMPLETION_ROUTINE")] delegate* unmanaged[Stdcall]<uint, uint, OVERLAPPED*, void> lpCompletionRoutine);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int ReadFileScatter([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("FILE_SEGMENT_ELEMENT[]")] _FILE_SEGMENT_ELEMENT* aSegmentArray, [NativeTypeName("DWORD")] uint nNumberOfBytesToRead, [NativeTypeName("LPDWORD")] uint* lpReserved, [NativeTypeName("LPOVERLAPPED")] OVERLAPPED* lpOverlapped);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int RemoveDirectoryA([NativeTypeName("LPCSTR")] sbyte* lpPathName);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int RemoveDirectoryW([NativeTypeName("LPCWSTR")] ushort* lpPathName);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetEndOfFile([NativeTypeName("HANDLE")] void* hFile);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetFileAttributesA([NativeTypeName("LPCSTR")] sbyte* lpFileName, [NativeTypeName("DWORD")] uint dwFileAttributes);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetFileAttributesW([NativeTypeName("LPCWSTR")] ushort* lpFileName, [NativeTypeName("DWORD")] uint dwFileAttributes);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetFileInformationByHandle([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("FILE_INFO_BY_HANDLE_CLASS")] _FILE_INFO_BY_HANDLE_CLASS FileInformationClass, [NativeTypeName("LPVOID")] void* lpFileInformation, [NativeTypeName("DWORD")] uint dwBufferSize);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint SetFilePointer([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("LONG")] int lDistanceToMove, [NativeTypeName("PLONG")] int* lpDistanceToMoveHigh, [NativeTypeName("DWORD")] uint dwMoveMethod);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetFilePointerEx([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("LARGE_INTEGER")] long liDistanceToMove, [NativeTypeName("PLARGE_INTEGER")] long* lpNewFilePointer, [NativeTypeName("DWORD")] uint dwMoveMethod);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetFileTime([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("const FILETIME *")] FILETIME* lpCreationTime, [NativeTypeName("const FILETIME *")] FILETIME* lpLastAccessTime, [NativeTypeName("const FILETIME *")] FILETIME* lpLastWriteTime);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetFileValidData([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("LONGLONG")] long ValidDataLength);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int UnlockFile([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("DWORD")] uint dwFileOffsetLow, [NativeTypeName("DWORD")] uint dwFileOffsetHigh, [NativeTypeName("DWORD")] uint nNumberOfBytesToUnlockLow, [NativeTypeName("DWORD")] uint nNumberOfBytesToUnlockHigh);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int UnlockFileEx([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("DWORD")] uint dwReserved, [NativeTypeName("DWORD")] uint nNumberOfBytesToUnlockLow, [NativeTypeName("DWORD")] uint nNumberOfBytesToUnlockHigh, [NativeTypeName("LPOVERLAPPED")] OVERLAPPED* lpOverlapped);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int WriteFile([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("LPCVOID")] void* lpBuffer, [NativeTypeName("DWORD")] uint nNumberOfBytesToWrite, [NativeTypeName("LPDWORD")] uint* lpNumberOfBytesWritten, [NativeTypeName("LPOVERLAPPED")] OVERLAPPED* lpOverlapped);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int WriteFileEx([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("LPCVOID")] void* lpBuffer, [NativeTypeName("DWORD")] uint nNumberOfBytesToWrite, [NativeTypeName("LPOVERLAPPED")] OVERLAPPED* lpOverlapped, [NativeTypeName("LPOVERLAPPED_COMPLETION_ROUTINE")] delegate* unmanaged[Stdcall]<uint, uint, OVERLAPPED*, void> lpCompletionRoutine);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int WriteFileGather([NativeTypeName("HANDLE")] void* hFile, [NativeTypeName("FILE_SEGMENT_ELEMENT[]")] _FILE_SEGMENT_ELEMENT* aSegmentArray, [NativeTypeName("DWORD")] uint nNumberOfBytesToWrite, [NativeTypeName("LPDWORD")] uint* lpReserved, [NativeTypeName("LPOVERLAPPED")] OVERLAPPED* lpOverlapped);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetTempPathW([NativeTypeName("DWORD")] uint nBufferLength, [NativeTypeName("LPWSTR")] ushort* lpBuffer);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetVolumeNameForVolumeMountPointW([NativeTypeName("LPCWSTR")] ushort* lpszVolumeMountPoint, [NativeTypeName("LPWSTR")] ushort* lpszVolumeName, [NativeTypeName("DWORD")] uint cchBufferLength);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetVolumePathNamesForVolumeNameW([NativeTypeName("LPCWSTR")] ushort* lpszVolumeName, [NativeTypeName("LPWCH")] ushort* lpszVolumePathNames, [NativeTypeName("DWORD")] uint cchBufferLength, [NativeTypeName("PDWORD")] uint* lpcchReturnLength);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* CreateFile2([NativeTypeName("LPCWSTR")] ushort* lpFileName, [NativeTypeName("DWORD")] uint dwDesiredAccess, [NativeTypeName("DWORD")] uint dwShareMode, [NativeTypeName("DWORD")] uint dwCreationDisposition, [NativeTypeName("LPCREATEFILE2_EXTENDED_PARAMETERS")] _CREATEFILE2_EXTENDED_PARAMETERS* pCreateExParams);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetFileIoOverlappedRange([NativeTypeName("HANDLE")] void* FileHandle, [NativeTypeName("PUCHAR")] byte* OverlappedRangeStart, [NativeTypeName("ULONG")] uint Length);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetCompressedFileSizeA([NativeTypeName("LPCSTR")] sbyte* lpFileName, [NativeTypeName("LPDWORD")] uint* lpFileSizeHigh);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetCompressedFileSizeW([NativeTypeName("LPCWSTR")] ushort* lpFileName, [NativeTypeName("LPDWORD")] uint* lpFileSizeHigh);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* FindFirstStreamW([NativeTypeName("LPCWSTR")] ushort* lpFileName, [NativeTypeName("STREAM_INFO_LEVELS")] _STREAM_INFO_LEVELS InfoLevel, [NativeTypeName("LPVOID")] void* lpFindStreamData, [NativeTypeName("DWORD")] uint dwFlags);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int FindNextStreamW([NativeTypeName("HANDLE")] void* hFindStream, [NativeTypeName("LPVOID")] void* lpFindStreamData);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int AreFileApisANSI();

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetTempPathA([NativeTypeName("DWORD")] uint nBufferLength, [NativeTypeName("LPSTR")] sbyte* lpBuffer);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* FindFirstFileNameW([NativeTypeName("LPCWSTR")] ushort* lpFileName, [NativeTypeName("DWORD")] uint dwFlags, [NativeTypeName("LPDWORD")] uint* StringLength, [NativeTypeName("PWSTR")] ushort* LinkName);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int FindNextFileNameW([NativeTypeName("HANDLE")] void* hFindStream, [NativeTypeName("LPDWORD")] uint* StringLength, [NativeTypeName("PWSTR")] ushort* LinkName);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetVolumeInformationA([NativeTypeName("LPCSTR")] sbyte* lpRootPathName, [NativeTypeName("LPSTR")] sbyte* lpVolumeNameBuffer, [NativeTypeName("DWORD")] uint nVolumeNameSize, [NativeTypeName("LPDWORD")] uint* lpVolumeSerialNumber, [NativeTypeName("LPDWORD")] uint* lpMaximumComponentLength, [NativeTypeName("LPDWORD")] uint* lpFileSystemFlags, [NativeTypeName("LPSTR")] sbyte* lpFileSystemNameBuffer, [NativeTypeName("DWORD")] uint nFileSystemNameSize);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern uint GetTempFileNameA([NativeTypeName("LPCSTR")] sbyte* lpPathName, [NativeTypeName("LPCSTR")] sbyte* lpPrefixString, uint uUnique, [NativeTypeName("LPSTR")] sbyte* lpTempFileName);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern void SetFileApisToOEM();

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern void SetFileApisToANSI();

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetTempPath2W([NativeTypeName("DWORD")] uint BufferLength, [NativeTypeName("LPWSTR")] ushort* Buffer);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetTempPath2A([NativeTypeName("DWORD")] uint BufferLength, [NativeTypeName("LPSTR")] sbyte* Buffer);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* CreateFile3([NativeTypeName("LPCWSTR")] ushort* lpFileName, [NativeTypeName("DWORD")] uint dwDesiredAccess, [NativeTypeName("DWORD")] uint dwShareMode, [NativeTypeName("DWORD")] uint dwCreationDisposition, [NativeTypeName("LPCREATEFILE3_EXTENDED_PARAMETERS")] _CREATEFILE3_EXTENDED_PARAMETERS* pCreateExParams);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* CreateDirectory2A([NativeTypeName("LPCSTR")] sbyte* lpPathName, [NativeTypeName("DWORD")] uint dwDesiredAccess, [NativeTypeName("DWORD")] uint dwShareMode, DIRECTORY_FLAGS DirectoryFlags, [NativeTypeName("LPSECURITY_ATTRIBUTES")] SECURITY_ATTRIBUTES* lpSecurityAttributes);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* CreateDirectory2W([NativeTypeName("LPCWSTR")] ushort* lpPathName, [NativeTypeName("DWORD")] uint dwDesiredAccess, [NativeTypeName("DWORD")] uint dwShareMode, DIRECTORY_FLAGS DirectoryFlags, [NativeTypeName("LPSECURITY_ATTRIBUTES")] SECURITY_ATTRIBUTES* lpSecurityAttributes);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int RemoveDirectory2A([NativeTypeName("LPCSTR")] sbyte* lpPathName, DIRECTORY_FLAGS DirectoryFlags);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int RemoveDirectory2W([NativeTypeName("LPCWSTR")] ushort* lpPathName, DIRECTORY_FLAGS DirectoryFlags);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int DeleteFile2A([NativeTypeName("LPCSTR")] sbyte* lpFileName, [NativeTypeName("DWORD")] uint Flags);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int DeleteFile2W([NativeTypeName("LPCWSTR")] ushort* lpFileName, [NativeTypeName("DWORD")] uint Flags);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint QueueUserAPC([NativeTypeName("PAPCFUNC")] delegate* unmanaged[Stdcall]<nuint, void> pfnAPC, [NativeTypeName("HANDLE")] void* hThread, [NativeTypeName("ULONG_PTR")] nuint dwData);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int QueueUserAPC2([NativeTypeName("PAPCFUNC")] delegate* unmanaged[Stdcall]<nuint, void> ApcRoutine, [NativeTypeName("HANDLE")] void* Thread, [NativeTypeName("ULONG_PTR")] nuint Data, [NativeTypeName("QUEUE_USER_APC_FLAGS")] _QUEUE_USER_APC_FLAGS Flags);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetProcessTimes([NativeTypeName("HANDLE")] void* hProcess, [NativeTypeName("LPFILETIME")] FILETIME* lpCreationTime, [NativeTypeName("LPFILETIME")] FILETIME* lpExitTime, [NativeTypeName("LPFILETIME")] FILETIME* lpKernelTime, [NativeTypeName("LPFILETIME")] FILETIME* lpUserTime);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* GetCurrentProcess();

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetCurrentProcessId();

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern void ExitProcess(uint uExitCode);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int TerminateProcess([NativeTypeName("HANDLE")] void* hProcess, uint uExitCode);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetExitCodeProcess([NativeTypeName("HANDLE")] void* hProcess, [NativeTypeName("LPDWORD")] uint* lpExitCode);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SwitchToThread();

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* CreateThread([NativeTypeName("LPSECURITY_ATTRIBUTES")] SECURITY_ATTRIBUTES* lpThreadAttributes, [NativeTypeName("SIZE_T")] nuint dwStackSize, [NativeTypeName("LPTHREAD_START_ROUTINE")] delegate* unmanaged[Stdcall]<void*, uint> lpStartAddress, [NativeTypeName("LPVOID")] void* lpParameter, [NativeTypeName("DWORD")] uint dwCreationFlags, [NativeTypeName("LPDWORD")] uint* lpThreadId);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* CreateRemoteThread([NativeTypeName("HANDLE")] void* hProcess, [NativeTypeName("LPSECURITY_ATTRIBUTES")] SECURITY_ATTRIBUTES* lpThreadAttributes, [NativeTypeName("SIZE_T")] nuint dwStackSize, [NativeTypeName("LPTHREAD_START_ROUTINE")] delegate* unmanaged[Stdcall]<void*, uint> lpStartAddress, [NativeTypeName("LPVOID")] void* lpParameter, [NativeTypeName("DWORD")] uint dwCreationFlags, [NativeTypeName("LPDWORD")] uint* lpThreadId);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* GetCurrentThread();

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetCurrentThreadId();

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* OpenThread([NativeTypeName("DWORD")] uint dwDesiredAccess, [NativeTypeName("BOOL")] int bInheritHandle, [NativeTypeName("DWORD")] uint dwThreadId);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetThreadPriority([NativeTypeName("HANDLE")] void* hThread, int nPriority);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetThreadPriorityBoost([NativeTypeName("HANDLE")] void* hThread, [NativeTypeName("BOOL")] int bDisablePriorityBoost);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetThreadPriorityBoost([NativeTypeName("HANDLE")] void* hThread, [NativeTypeName("PBOOL")] int* pDisablePriorityBoost);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int GetThreadPriority([NativeTypeName("HANDLE")] void* hThread);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern void ExitThread([NativeTypeName("DWORD")] uint dwExitCode);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int TerminateThread([NativeTypeName("HANDLE")] void* hThread, [NativeTypeName("DWORD")] uint dwExitCode);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetExitCodeThread([NativeTypeName("HANDLE")] void* hThread, [NativeTypeName("LPDWORD")] uint* lpExitCode);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint SuspendThread([NativeTypeName("HANDLE")] void* hThread);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint ResumeThread([NativeTypeName("HANDLE")] void* hThread);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint TlsAlloc();

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("LPVOID")]
        public static extern void* TlsGetValue([NativeTypeName("DWORD")] uint dwTlsIndex);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int TlsSetValue([NativeTypeName("DWORD")] uint dwTlsIndex, [NativeTypeName("LPVOID")] void* lpTlsValue);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int TlsFree([NativeTypeName("DWORD")] uint dwTlsIndex);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int CreateProcessA([NativeTypeName("LPCSTR")] sbyte* lpApplicationName, [NativeTypeName("LPSTR")] sbyte* lpCommandLine, [NativeTypeName("LPSECURITY_ATTRIBUTES")] SECURITY_ATTRIBUTES* lpProcessAttributes, [NativeTypeName("LPSECURITY_ATTRIBUTES")] SECURITY_ATTRIBUTES* lpThreadAttributes, [NativeTypeName("BOOL")] int bInheritHandles, [NativeTypeName("DWORD")] uint dwCreationFlags, [NativeTypeName("LPVOID")] void* lpEnvironment, [NativeTypeName("LPCSTR")] sbyte* lpCurrentDirectory, [NativeTypeName("LPSTARTUPINFOA")] _STARTUPINFOA* lpStartupInfo, [NativeTypeName("LPPROCESS_INFORMATION")] _PROCESS_INFORMATION* lpProcessInformation);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int CreateProcessW([NativeTypeName("LPCWSTR")] ushort* lpApplicationName, [NativeTypeName("LPWSTR")] ushort* lpCommandLine, [NativeTypeName("LPSECURITY_ATTRIBUTES")] SECURITY_ATTRIBUTES* lpProcessAttributes, [NativeTypeName("LPSECURITY_ATTRIBUTES")] SECURITY_ATTRIBUTES* lpThreadAttributes, [NativeTypeName("BOOL")] int bInheritHandles, [NativeTypeName("DWORD")] uint dwCreationFlags, [NativeTypeName("LPVOID")] void* lpEnvironment, [NativeTypeName("LPCWSTR")] ushort* lpCurrentDirectory, [NativeTypeName("LPSTARTUPINFOW")] _STARTUPINFOW* lpStartupInfo, [NativeTypeName("LPPROCESS_INFORMATION")] _PROCESS_INFORMATION* lpProcessInformation);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetProcessShutdownParameters([NativeTypeName("DWORD")] uint dwLevel, [NativeTypeName("DWORD")] uint dwFlags);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetProcessVersion([NativeTypeName("DWORD")] uint ProcessId);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern void GetStartupInfoW([NativeTypeName("LPSTARTUPINFOW")] _STARTUPINFOW* lpStartupInfo);

        [DllImport("advapi32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int CreateProcessAsUserW([NativeTypeName("HANDLE")] void* hToken, [NativeTypeName("LPCWSTR")] ushort* lpApplicationName, [NativeTypeName("LPWSTR")] ushort* lpCommandLine, [NativeTypeName("LPSECURITY_ATTRIBUTES")] SECURITY_ATTRIBUTES* lpProcessAttributes, [NativeTypeName("LPSECURITY_ATTRIBUTES")] SECURITY_ATTRIBUTES* lpThreadAttributes, [NativeTypeName("BOOL")] int bInheritHandles, [NativeTypeName("DWORD")] uint dwCreationFlags, [NativeTypeName("LPVOID")] void* lpEnvironment, [NativeTypeName("LPCWSTR")] ushort* lpCurrentDirectory, [NativeTypeName("LPSTARTUPINFOW")] _STARTUPINFOW* lpStartupInfo, [NativeTypeName("LPPROCESS_INFORMATION")] _PROCESS_INFORMATION* lpProcessInformation);

        [DllImport("advapi32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetThreadToken([NativeTypeName("PHANDLE")] void** Thread, [NativeTypeName("HANDLE")] void* Token);

        [DllImport("advapi32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int OpenProcessToken([NativeTypeName("HANDLE")] void* ProcessHandle, [NativeTypeName("DWORD")] uint DesiredAccess, [NativeTypeName("PHANDLE")] void** TokenHandle);

        [DllImport("advapi32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int OpenThreadToken([NativeTypeName("HANDLE")] void* ThreadHandle, [NativeTypeName("DWORD")] uint DesiredAccess, [NativeTypeName("BOOL")] int OpenAsSelf, [NativeTypeName("PHANDLE")] void** TokenHandle);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetPriorityClass([NativeTypeName("HANDLE")] void* hProcess, [NativeTypeName("DWORD")] uint dwPriorityClass);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetPriorityClass([NativeTypeName("HANDLE")] void* hProcess);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetThreadStackGuarantee([NativeTypeName("PULONG")] uint* StackSizeInBytes);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int ProcessIdToSessionId([NativeTypeName("DWORD")] uint dwProcessId, [NativeTypeName("DWORD *")] uint* pSessionId);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetProcessId([NativeTypeName("HANDLE")] void* Process);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetThreadId([NativeTypeName("HANDLE")] void* Thread);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern void FlushProcessWriteBuffers();

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetProcessIdOfThread([NativeTypeName("HANDLE")] void* Thread);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int InitializeProcThreadAttributeList([NativeTypeName("LPPROC_THREAD_ATTRIBUTE_LIST")] _PROC_THREAD_ATTRIBUTE_LIST* lpAttributeList, [NativeTypeName("DWORD")] uint dwAttributeCount, [NativeTypeName("DWORD")] uint dwFlags, [NativeTypeName("PSIZE_T")] nuint* lpSize);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern void DeleteProcThreadAttributeList([NativeTypeName("LPPROC_THREAD_ATTRIBUTE_LIST")] _PROC_THREAD_ATTRIBUTE_LIST* lpAttributeList);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int UpdateProcThreadAttribute([NativeTypeName("LPPROC_THREAD_ATTRIBUTE_LIST")] _PROC_THREAD_ATTRIBUTE_LIST* lpAttributeList, [NativeTypeName("DWORD")] uint dwFlags, [NativeTypeName("DWORD_PTR")] nuint Attribute, [NativeTypeName("PVOID")] void* lpValue, [NativeTypeName("SIZE_T")] nuint cbSize, [NativeTypeName("PVOID")] void* lpPreviousValue, [NativeTypeName("PSIZE_T")] nuint* lpReturnSize);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetProcessAffinityUpdateMode([NativeTypeName("HANDLE")] void* hProcess, [NativeTypeName("DWORD")] uint dwFlags);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int QueryProcessAffinityUpdateMode([NativeTypeName("HANDLE")] void* hProcess, [NativeTypeName("LPDWORD")] uint* lpdwFlags);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* CreateRemoteThreadEx([NativeTypeName("HANDLE")] void* hProcess, [NativeTypeName("LPSECURITY_ATTRIBUTES")] SECURITY_ATTRIBUTES* lpThreadAttributes, [NativeTypeName("SIZE_T")] nuint dwStackSize, [NativeTypeName("LPTHREAD_START_ROUTINE")] delegate* unmanaged[Stdcall]<void*, uint> lpStartAddress, [NativeTypeName("LPVOID")] void* lpParameter, [NativeTypeName("DWORD")] uint dwCreationFlags, [NativeTypeName("LPPROC_THREAD_ATTRIBUTE_LIST")] _PROC_THREAD_ATTRIBUTE_LIST* lpAttributeList, [NativeTypeName("LPDWORD")] uint* lpThreadId);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern void GetCurrentThreadStackLimits([NativeTypeName("PULONG_PTR")] ulong* LowLimit, [NativeTypeName("PULONG_PTR")] ulong* HighLimit);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetThreadContext([NativeTypeName("HANDLE")] void* hThread, [NativeTypeName("LPCONTEXT")] _CONTEXT* lpContext);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetThreadContext([NativeTypeName("HANDLE")] void* hThread, [NativeTypeName("const CONTEXT *")] _CONTEXT* lpContext);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int FlushInstructionCache([NativeTypeName("HANDLE")] void* hProcess, [NativeTypeName("LPCVOID")] void* lpBaseAddress, [NativeTypeName("SIZE_T")] nuint dwSize);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetThreadTimes([NativeTypeName("HANDLE")] void* hThread, [NativeTypeName("LPFILETIME")] FILETIME* lpCreationTime, [NativeTypeName("LPFILETIME")] FILETIME* lpExitTime, [NativeTypeName("LPFILETIME")] FILETIME* lpKernelTime, [NativeTypeName("LPFILETIME")] FILETIME* lpUserTime);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HANDLE")]
        public static extern void* OpenProcess([NativeTypeName("DWORD")] uint dwDesiredAccess, [NativeTypeName("BOOL")] int bInheritHandle, [NativeTypeName("DWORD")] uint dwProcessId);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int IsProcessorFeaturePresent([NativeTypeName("DWORD")] uint ProcessorFeature);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetProcessHandleCount([NativeTypeName("HANDLE")] void* hProcess, [NativeTypeName("PDWORD")] uint* pdwHandleCount);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint GetCurrentProcessorNumber();

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetThreadIdealProcessorEx([NativeTypeName("HANDLE")] void* hThread, [NativeTypeName("PPROCESSOR_NUMBER")] _PROCESSOR_NUMBER* lpIdealProcessor, [NativeTypeName("PPROCESSOR_NUMBER")] _PROCESSOR_NUMBER* lpPreviousIdealProcessor);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetThreadIdealProcessorEx([NativeTypeName("HANDLE")] void* hThread, [NativeTypeName("PPROCESSOR_NUMBER")] _PROCESSOR_NUMBER* lpIdealProcessor);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern void GetCurrentProcessorNumberEx([NativeTypeName("PPROCESSOR_NUMBER")] _PROCESSOR_NUMBER* ProcNumber);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetProcessPriorityBoost([NativeTypeName("HANDLE")] void* hProcess, [NativeTypeName("PBOOL")] int* pDisablePriorityBoost);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetProcessPriorityBoost([NativeTypeName("HANDLE")] void* hProcess, [NativeTypeName("BOOL")] int bDisablePriorityBoost);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetThreadIOPendingFlag([NativeTypeName("HANDLE")] void* hThread, [NativeTypeName("PBOOL")] int* lpIOIsPending);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetSystemTimes([NativeTypeName("PFILETIME")] FILETIME* lpIdleTime, [NativeTypeName("PFILETIME")] FILETIME* lpKernelTime, [NativeTypeName("PFILETIME")] FILETIME* lpUserTime);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetThreadInformation([NativeTypeName("HANDLE")] void* hThread, [NativeTypeName("THREAD_INFORMATION_CLASS")] _THREAD_INFORMATION_CLASS ThreadInformationClass, [NativeTypeName("LPVOID")] void* ThreadInformation, [NativeTypeName("DWORD")] uint ThreadInformationSize);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetThreadInformation([NativeTypeName("HANDLE")] void* hThread, [NativeTypeName("THREAD_INFORMATION_CLASS")] _THREAD_INFORMATION_CLASS ThreadInformationClass, [NativeTypeName("LPVOID")] void* ThreadInformation, [NativeTypeName("DWORD")] uint ThreadInformationSize);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int IsProcessCritical([NativeTypeName("HANDLE")] void* hProcess, [NativeTypeName("PBOOL")] int* Critical);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetProtectedPolicy([NativeTypeName("LPCGUID")] Guid* PolicyGuid, [NativeTypeName("ULONG_PTR")] nuint PolicyValue, [NativeTypeName("PULONG_PTR")] ulong* OldPolicyValue);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int QueryProtectedPolicy([NativeTypeName("LPCGUID")] Guid* PolicyGuid, [NativeTypeName("PULONG_PTR")] ulong* PolicyValue);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("DWORD")]
        public static extern uint SetThreadIdealProcessor([NativeTypeName("HANDLE")] void* hThread, [NativeTypeName("DWORD")] uint dwIdealProcessor);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetProcessInformation([NativeTypeName("HANDLE")] void* hProcess, [NativeTypeName("PROCESS_INFORMATION_CLASS")] _PROCESS_INFORMATION_CLASS ProcessInformationClass, [NativeTypeName("LPVOID")] void* ProcessInformation, [NativeTypeName("DWORD")] uint ProcessInformationSize);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetProcessInformation([NativeTypeName("HANDLE")] void* hProcess, [NativeTypeName("PROCESS_INFORMATION_CLASS")] _PROCESS_INFORMATION_CLASS ProcessInformationClass, [NativeTypeName("LPVOID")] void* ProcessInformation, [NativeTypeName("DWORD")] uint ProcessInformationSize);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetProcessDefaultCpuSets([NativeTypeName("HANDLE")] void* Process, [NativeTypeName("PULONG")] uint* CpuSetIds, [NativeTypeName("ULONG")] uint CpuSetIdCount, [NativeTypeName("PULONG")] uint* RequiredIdCount);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetProcessDefaultCpuSets([NativeTypeName("HANDLE")] void* Process, [NativeTypeName("const ULONG *")] uint* CpuSetIds, [NativeTypeName("ULONG")] uint CpuSetIdCount);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetThreadSelectedCpuSets([NativeTypeName("HANDLE")] void* Thread, [NativeTypeName("PULONG")] uint* CpuSetIds, [NativeTypeName("ULONG")] uint CpuSetIdCount, [NativeTypeName("PULONG")] uint* RequiredIdCount);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int SetThreadSelectedCpuSets([NativeTypeName("HANDLE")] void* Thread, [NativeTypeName("const ULONG *")] uint* CpuSetIds, [NativeTypeName("ULONG")] uint CpuSetIdCount);

        [DllImport("advapi32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int CreateProcessAsUserA([NativeTypeName("HANDLE")] void* hToken, [NativeTypeName("LPCSTR")] sbyte* lpApplicationName, [NativeTypeName("LPSTR")] sbyte* lpCommandLine, [NativeTypeName("LPSECURITY_ATTRIBUTES")] SECURITY_ATTRIBUTES* lpProcessAttributes, [NativeTypeName("LPSECURITY_ATTRIBUTES")] SECURITY_ATTRIBUTES* lpThreadAttributes, [NativeTypeName("BOOL")] int bInheritHandles, [NativeTypeName("DWORD")] uint dwCreationFlags, [NativeTypeName("LPVOID")] void* lpEnvironment, [NativeTypeName("LPCSTR")] sbyte* lpCurrentDirectory, [NativeTypeName("LPSTARTUPINFOA")] _STARTUPINFOA* lpStartupInfo, [NativeTypeName("LPPROCESS_INFORMATION")] _PROCESS_INFORMATION* lpProcessInformation);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("BOOL")]
        public static extern int GetProcessShutdownParameters([NativeTypeName("LPDWORD")] uint* lpdwLevel, [NativeTypeName("LPDWORD")] uint* lpdwFlags);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HRESULT")]
        public static extern int GetMachineTypeAttributes(ushort Machine, [NativeTypeName("MACHINE_ATTRIBUTES *")] _MACHINE_ATTRIBUTES* MachineTypeAttributes);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HRESULT")]
        public static extern int SetThreadDescription([NativeTypeName("HANDLE")] void* hThread, [NativeTypeName("PCWSTR")] ushort* lpThreadDescription);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("HRESULT")]
        public static extern int GetThreadDescription([NativeTypeName("HANDLE")] void* hThread, [NativeTypeName("PWSTR *")] ushort** ppszThreadDescription);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        [return: NativeTypeName("LPVOID")]
        public static extern void* TlsGetValue2([NativeTypeName("DWORD")] uint dwTlsIndex);

        [NativeTypeName("#define CREATE_NEW 1")]
        public const int CREATE_NEW = 1;

        [NativeTypeName("#define CREATE_ALWAYS 2")]
        public const int CREATE_ALWAYS = 2;

        [NativeTypeName("#define OPEN_EXISTING 3")]
        public const int OPEN_EXISTING = 3;

        [NativeTypeName("#define OPEN_ALWAYS 4")]
        public const int OPEN_ALWAYS = 4;

        [NativeTypeName("#define TRUNCATE_EXISTING 5")]
        public const int TRUNCATE_EXISTING = 5;

        [NativeTypeName("#define INVALID_FILE_SIZE ((DWORD)0xFFFFFFFF)")]
        public const uint INVALID_FILE_SIZE = ((uint)(0xFFFFFFFF));

        [NativeTypeName("#define INVALID_SET_FILE_POINTER ((DWORD)-1)")]
        public const uint INVALID_SET_FILE_POINTER = unchecked((uint)(-1));

        [NativeTypeName("#define INVALID_FILE_ATTRIBUTES ((DWORD)-1)")]
        public const uint INVALID_FILE_ATTRIBUTES = unchecked((uint)(-1));

        [NativeTypeName("#define CreateDirectory CreateDirectoryA")]
        public static delegate*<sbyte*, SECURITY_ATTRIBUTES*, int> CreateDirectory => &CreateDirectoryA;

        [NativeTypeName("#define CreateFile CreateFileA")]
        public static delegate*<sbyte*, uint, uint, SECURITY_ATTRIBUTES*, uint, uint, void*, void*> CreateFile => &CreateFileA;

        [NativeTypeName("#define DeleteFile DeleteFileA")]
        public static delegate*<sbyte*, int> DeleteFile => &DeleteFileA;

        [NativeTypeName("#define FindFirstChangeNotification FindFirstChangeNotificationA")]
        public static delegate*<sbyte*, int, uint, void*> FindFirstChangeNotification => &FindFirstChangeNotificationA;

        [NativeTypeName("#define FindFirstFile FindFirstFileA")]
        public static delegate*<sbyte*, WIN32_FIND_DATAA*, void*> FindFirstFile => &FindFirstFileA;

        [NativeTypeName("#define FindFirstFileEx FindFirstFileExA")]
        public static delegate*<sbyte*, _FINDEX_INFO_LEVELS, void*, _FINDEX_SEARCH_OPS, void*, uint, void*> FindFirstFileEx => &FindFirstFileExA;

        [NativeTypeName("#define FindNextFile FindNextFileA")]
        public static delegate*<void*, WIN32_FIND_DATAA*, int> FindNextFile => &FindNextFileA;

        [NativeTypeName("#define GetDiskFreeSpace GetDiskFreeSpaceA")]
        public static delegate*<sbyte*, uint*, uint*, uint*, uint*, int> GetDiskFreeSpace => &GetDiskFreeSpaceA;

        [NativeTypeName("#define GetDiskFreeSpaceEx GetDiskFreeSpaceExA")]
        public static delegate*<sbyte*, ulong*, ulong*, ulong*, int> GetDiskFreeSpaceEx => &GetDiskFreeSpaceExA;

        [NativeTypeName("#define GetDiskSpaceInformation GetDiskSpaceInformationA")]
        public static delegate*<sbyte*, DISK_SPACE_INFORMATION*, int> GetDiskSpaceInformation => &GetDiskSpaceInformationA;

        [NativeTypeName("#define GetDriveType GetDriveTypeA")]
        public static delegate*<sbyte*, uint> GetDriveType => &GetDriveTypeA;

        [NativeTypeName("#define GetFileAttributes GetFileAttributesA")]
        public static delegate*<sbyte*, uint> GetFileAttributes => &GetFileAttributesA;

        [NativeTypeName("#define GetFileAttributesEx GetFileAttributesExA")]
        public static delegate*<sbyte*, _GET_FILEEX_INFO_LEVELS, void*, int> GetFileAttributesEx => &GetFileAttributesExA;

        [NativeTypeName("#define GetFinalPathNameByHandle GetFinalPathNameByHandleA")]
        public static delegate*<void*, sbyte*, uint, uint, uint> GetFinalPathNameByHandle => &GetFinalPathNameByHandleA;

        [NativeTypeName("#define GetFullPathName GetFullPathNameA")]
        public static delegate*<sbyte*, uint, sbyte*, sbyte**, uint> GetFullPathName => &GetFullPathNameA;

        [NativeTypeName("#define GetLongPathName GetLongPathNameA")]
        public static delegate*<sbyte*, sbyte*, uint, uint> GetLongPathName => &GetLongPathNameA;

        [NativeTypeName("#define RemoveDirectory RemoveDirectoryA")]
        public static delegate*<sbyte*, int> RemoveDirectory => &RemoveDirectoryA;

        [NativeTypeName("#define SetFileAttributes SetFileAttributesA")]
        public static delegate*<sbyte*, uint, int> SetFileAttributes => &SetFileAttributesA;

        [NativeTypeName("#define GetCompressedFileSize GetCompressedFileSizeA")]
        public static delegate*<sbyte*, uint*, uint> GetCompressedFileSize => &GetCompressedFileSizeA;

        [NativeTypeName("#define GetTempPath GetTempPathA")]
        public static delegate*<uint, sbyte*, uint> GetTempPath => &GetTempPathA;

        [NativeTypeName("#define GetVolumeInformation GetVolumeInformationA")]
        public static delegate*<sbyte*, sbyte*, uint, uint*, uint*, uint*, sbyte*, uint, int> GetVolumeInformation => &GetVolumeInformationA;

        [NativeTypeName("#define GetTempFileName GetTempFileNameA")]
        public static delegate*<sbyte*, sbyte*, uint, sbyte*, uint> GetTempFileName => &GetTempFileNameA;

        [NativeTypeName("#define GetTempPath2 GetTempPath2A")]
        public static delegate*<uint, sbyte*, uint> GetTempPath2 => &GetTempPath2A;

        [NativeTypeName("#define CreateDirectory2 CreateDirectory2A")]
        public static delegate*<sbyte*, uint, uint, DIRECTORY_FLAGS, SECURITY_ATTRIBUTES*, void*> CreateDirectory2 => &CreateDirectory2A;

        [NativeTypeName("#define RemoveDirectory2 RemoveDirectory2A")]
        public static delegate*<sbyte*, DIRECTORY_FLAGS, int> RemoveDirectory2 => &RemoveDirectory2A;

        [NativeTypeName("#define DeleteFile2 DeleteFile2A")]
        public static delegate*<sbyte*, uint, int> DeleteFile2 => &DeleteFile2A;

        [NativeTypeName("#define TLS_OUT_OF_INDEXES ((DWORD)0xFFFFFFFF)")]
        public const uint TLS_OUT_OF_INDEXES = ((uint)(0xFFFFFFFF));

        [NativeTypeName("#define CreateProcess CreateProcessA")]
        public static delegate*<sbyte*, sbyte*, SECURITY_ATTRIBUTES*, SECURITY_ATTRIBUTES*, int, uint, void*, sbyte*, _STARTUPINFOA*, _PROCESS_INFORMATION*, int> CreateProcess => &CreateProcessA;

        [NativeTypeName("#define PROC_THREAD_ATTRIBUTE_REPLACE_VALUE 0x00000001")]
        public const int PROC_THREAD_ATTRIBUTE_REPLACE_VALUE = 0x00000001;

        [NativeTypeName("#define PROCESS_AFFINITY_ENABLE_AUTO_UPDATE 0x00000001UL")]
        public const uint PROCESS_AFFINITY_ENABLE_AUTO_UPDATE = 0x00000001U;

        [NativeTypeName("#define THREAD_POWER_THROTTLING_CURRENT_VERSION 1")]
        public const int THREAD_POWER_THROTTLING_CURRENT_VERSION = 1;

        [NativeTypeName("#define THREAD_POWER_THROTTLING_EXECUTION_SPEED 0x1")]
        public const int THREAD_POWER_THROTTLING_EXECUTION_SPEED = 0x1;

        [NativeTypeName("#define THREAD_POWER_THROTTLING_VALID_FLAGS (THREAD_POWER_THROTTLING_EXECUTION_SPEED)")]
        public const int THREAD_POWER_THROTTLING_VALID_FLAGS = (0x1);

        [NativeTypeName("#define PME_CURRENT_VERSION 1")]
        public const int PME_CURRENT_VERSION = 1;

        [NativeTypeName("#define PME_FAILFAST_ON_COMMIT_FAIL_DISABLE 0x0")]
        public const int PME_FAILFAST_ON_COMMIT_FAIL_DISABLE = 0x0;

        [NativeTypeName("#define PME_FAILFAST_ON_COMMIT_FAIL_ENABLE 0x1")]
        public const int PME_FAILFAST_ON_COMMIT_FAIL_ENABLE = 0x1;

        [NativeTypeName("#define PROCESS_POWER_THROTTLING_CURRENT_VERSION 1")]
        public const int PROCESS_POWER_THROTTLING_CURRENT_VERSION = 1;

        [NativeTypeName("#define PROCESS_POWER_THROTTLING_EXECUTION_SPEED 0x1")]
        public const int PROCESS_POWER_THROTTLING_EXECUTION_SPEED = 0x1;

        [NativeTypeName("#define PROCESS_POWER_THROTTLING_IGNORE_TIMER_RESOLUTION 0x4")]
        public const int PROCESS_POWER_THROTTLING_IGNORE_TIMER_RESOLUTION = 0x4;

        [NativeTypeName("#define PROCESS_POWER_THROTTLING_VALID_FLAGS ((PROCESS_POWER_THROTTLING_EXECUTION_SPEED | \\\r\n                                               PROCESS_POWER_THROTTLING_IGNORE_TIMER_RESOLUTION))")]
        public const int PROCESS_POWER_THROTTLING_VALID_FLAGS = ((0x1 | 0x4));

        [NativeTypeName("#define PROCESS_LEAP_SECOND_INFO_FLAG_ENABLE_SIXTY_SECOND 0x1")]
        public const int PROCESS_LEAP_SECOND_INFO_FLAG_ENABLE_SIXTY_SECOND = 0x1;

        [NativeTypeName("#define PROCESS_LEAP_SECOND_INFO_VALID_FLAGS (PROCESS_LEAP_SECOND_INFO_FLAG_ENABLE_SIXTY_SECOND)")]
        public const int PROCESS_LEAP_SECOND_INFO_VALID_FLAGS = (0x1);

        [NativeTypeName("#define CreateProcessAsUser CreateProcessAsUserA")]
        public static delegate*<void*, sbyte*, sbyte*, SECURITY_ATTRIBUTES*, SECURITY_ATTRIBUTES*, int, uint, void*, sbyte*, _STARTUPINFOA*, _PROCESS_INFORMATION*, int> CreateProcessAsUser => &CreateProcessAsUserA;
    }
}
