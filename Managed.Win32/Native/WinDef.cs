using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Managed.Win32.Native
{
    public partial struct FILETIME
    {
        [NativeTypeName("DWORD")]
        public uint dwLowDateTime;

        [NativeTypeName("DWORD")]
        public uint dwHighDateTime;
    }

    public unsafe partial struct SECURITY_ATTRIBUTES
    {
        [NativeTypeName("DWORD")]
        public uint nLength;

        [NativeTypeName("LPVOID")]
        public void* lpSecurityDescriptor;

        [NativeTypeName("BOOL")]
        public int bInheritHandle;
    }

    public unsafe partial struct OVERLAPPED
    {
        [NativeTypeName("ULONG_PTR")]
        public nuint Internal;

        [NativeTypeName("ULONG_PTR")]
        public nuint InternalHigh;

        [NativeTypeName("__AnonymousRecord_minwinbase_L55_C5")]
        public _Anonymous_e__Union Anonymous;

        [NativeTypeName("HANDLE")]
        public void* hEvent;

        [UnscopedRef]
        public ref uint Offset
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref Anonymous.Anonymous.Offset;
            }
        }

        [UnscopedRef]
        public ref uint OffsetHigh
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref Anonymous.Anonymous.OffsetHigh;
            }
        }

        [UnscopedRef]
        public ref void* Pointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref Anonymous.Pointer;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_minwinbase_L56_C9")]
            public _Anonymous_e__Struct Anonymous;

            [FieldOffset(0)]
            [NativeTypeName("PVOID")]
            public void* Pointer;

            public partial struct _Anonymous_e__Struct
            {
                [NativeTypeName("DWORD")]
                public uint Offset;

                [NativeTypeName("DWORD")]
                public uint OffsetHigh;
            }
        }
    }

    public unsafe partial struct OVERLAPPED_ENTRY
    {
        [NativeTypeName("ULONG_PTR")]
        public nuint lpCompletionKey;

        [NativeTypeName("LPOVERLAPPED")]
        public OVERLAPPED* lpOverlapped;

        [NativeTypeName("ULONG_PTR")]
        public nuint Internal;

        [NativeTypeName("DWORD")]
        public uint dwNumberOfBytesTransferred;
    }

    public partial struct SYSTEMTIME
    {
        [NativeTypeName("WORD")]
        public ushort wYear;

        [NativeTypeName("WORD")]
        public ushort wMonth;

        [NativeTypeName("WORD")]
        public ushort wDayOfWeek;

        [NativeTypeName("WORD")]
        public ushort wDay;

        [NativeTypeName("WORD")]
        public ushort wHour;

        [NativeTypeName("WORD")]
        public ushort wMinute;

        [NativeTypeName("WORD")]
        public ushort wSecond;

        [NativeTypeName("WORD")]
        public ushort wMilliseconds;
    }

    public partial struct WIN32_FIND_DATAA
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

        [NativeTypeName("DWORD")]
        public uint dwReserved0;

        [NativeTypeName("DWORD")]
        public uint dwReserved1;

        [NativeTypeName("CHAR[260]")]
        public _cFileName_e__FixedBuffer cFileName;

        [NativeTypeName("CHAR[14]")]
        public _cAlternateFileName_e__FixedBuffer cAlternateFileName;

        [InlineArray(260)]
        public partial struct _cFileName_e__FixedBuffer
        {
            public sbyte e0;
        }

        [InlineArray(14)]
        public partial struct _cAlternateFileName_e__FixedBuffer
        {
            public sbyte e0;
        }
    }

    public partial struct WIN32_FIND_DATAW
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

        [NativeTypeName("DWORD")]
        public uint dwReserved0;

        [NativeTypeName("DWORD")]
        public uint dwReserved1;

        [NativeTypeName("WCHAR[260]")]
        public _cFileName_e__FixedBuffer cFileName;

        [NativeTypeName("WCHAR[14]")]
        public _cAlternateFileName_e__FixedBuffer cAlternateFileName;

        [InlineArray(260)]
        public partial struct _cFileName_e__FixedBuffer
        {
            public ushort e0;
        }

        [InlineArray(14)]
        public partial struct _cAlternateFileName_e__FixedBuffer
        {
            public ushort e0;
        }
    }

    public enum _FINDEX_INFO_LEVELS
    {
        FindExInfoStandard,
        FindExInfoBasic,
        FindExInfoMaxInfoLevel,
    }

    public enum _FINDEX_SEARCH_OPS
    {
        FindExSearchNameMatch,
        FindExSearchLimitToDirectories,
        FindExSearchLimitToDevices,
        FindExSearchMaxSearchOp,
    }

    public enum _READ_DIRECTORY_NOTIFY_INFORMATION_CLASS
    {
        ReadDirectoryNotifyInformation = 1,
        ReadDirectoryNotifyExtendedInformation,
        ReadDirectoryNotifyFullInformation,
        ReadDirectoryNotifyMaximumInformation,
    }

    public enum _GET_FILEEX_INFO_LEVELS
    {
        GetFileExInfoStandard,
        GetFileExMaxInfoLevel,
    }

    public enum _FILE_INFO_BY_HANDLE_CLASS
    {
        FileBasicInfo,
        FileStandardInfo,
        FileNameInfo,
        FileRenameInfo,
        FileDispositionInfo,
        FileAllocationInfo,
        FileEndOfFileInfo,
        FileStreamInfo,
        FileCompressionInfo,
        FileAttributeTagInfo,
        FileIdBothDirectoryInfo,
        FileIdBothDirectoryRestartInfo,
        FileIoPriorityHintInfo,
        FileRemoteProtocolInfo,
        FileFullDirectoryInfo,
        FileFullDirectoryRestartInfo,
        FileStorageInfo,
        FileAlignmentInfo,
        FileIdInfo,
        FileIdExtdDirectoryInfo,
        FileIdExtdDirectoryRestartInfo,
        FileDispositionInfoEx,
        FileRenameInfoEx,
        FileCaseSensitiveInfo,
        FileNormalizedNameInfo,
        MaximumFileInfoByHandleClass,
    }

    public enum _FILE_INFO_BY_NAME_CLASS
    {
        FileStatByNameInfo,
        FileStatLxByNameInfo,
        FileCaseSensitiveByNameInfo,
        FileStatBasicByNameInfo,
        MaximumFileInfoByNameClass,
    }

    public unsafe partial struct PROCESS_HEAP_ENTRY
    {
        [NativeTypeName("PVOID")]
        public void* lpData;

        [NativeTypeName("DWORD")]
        public uint cbData;

        public byte cbOverhead;

        public byte iRegionIndex;

        [NativeTypeName("WORD")]
        public ushort wFlags;

        [NativeTypeName("__AnonymousRecord_minwinbase_L252_C5")]
        public _Anonymous_e__Union Anonymous;

        [UnscopedRef]
        public ref _Anonymous_e__Union._Block_e__Struct Block
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref Anonymous.Block;
            }
        }

        [UnscopedRef]
        public ref _Anonymous_e__Union._Region_e__Struct Region
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref Anonymous.Region;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_minwinbase_L253_C9")]
            public _Block_e__Struct Block;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_minwinbase_L257_C9")]
            public _Region_e__Struct Region;

            public unsafe partial struct _Block_e__Struct
            {
                [NativeTypeName("HANDLE")]
                public void* hMem;

                [NativeTypeName("DWORD[3]")]
                public _dwReserved_e__FixedBuffer dwReserved;

                [InlineArray(3)]
                public partial struct _dwReserved_e__FixedBuffer
                {
                    public uint e0;
                }
            }

            public unsafe partial struct _Region_e__Struct
            {
                [NativeTypeName("DWORD")]
                public uint dwCommittedSize;

                [NativeTypeName("DWORD")]
                public uint dwUnCommittedSize;

                [NativeTypeName("LPVOID")]
                public void* lpFirstBlock;

                [NativeTypeName("LPVOID")]
                public void* lpLastBlock;
            }
        }
    }

    public unsafe partial struct _CREATE_THREAD_DEBUG_INFO
    {
        [NativeTypeName("HANDLE")]
        public void* hThread;

        [NativeTypeName("LPVOID")]
        public void* lpThreadLocalBase;

        [NativeTypeName("LPTHREAD_START_ROUTINE")]
        public delegate* unmanaged[Stdcall]<void*, uint> lpStartAddress;
    }

    public unsafe partial struct _CREATE_PROCESS_DEBUG_INFO
    {
        [NativeTypeName("HANDLE")]
        public void* hFile;

        [NativeTypeName("HANDLE")]
        public void* hProcess;

        [NativeTypeName("HANDLE")]
        public void* hThread;

        [NativeTypeName("LPVOID")]
        public void* lpBaseOfImage;

        [NativeTypeName("DWORD")]
        public uint dwDebugInfoFileOffset;

        [NativeTypeName("DWORD")]
        public uint nDebugInfoSize;

        [NativeTypeName("LPVOID")]
        public void* lpThreadLocalBase;

        [NativeTypeName("LPTHREAD_START_ROUTINE")]
        public delegate* unmanaged[Stdcall]<void*, uint> lpStartAddress;

        [NativeTypeName("LPVOID")]
        public void* lpImageName;

        [NativeTypeName("WORD")]
        public ushort fUnicode;
    }

    public partial struct _EXIT_THREAD_DEBUG_INFO
    {
        [NativeTypeName("DWORD")]
        public uint dwExitCode;
    }

    public partial struct _EXIT_PROCESS_DEBUG_INFO
    {
        [NativeTypeName("DWORD")]
        public uint dwExitCode;
    }

    public unsafe partial struct _LOAD_DLL_DEBUG_INFO
    {
        [NativeTypeName("HANDLE")]
        public void* hFile;

        [NativeTypeName("LPVOID")]
        public void* lpBaseOfDll;

        [NativeTypeName("DWORD")]
        public uint dwDebugInfoFileOffset;

        [NativeTypeName("DWORD")]
        public uint nDebugInfoSize;

        [NativeTypeName("LPVOID")]
        public void* lpImageName;

        [NativeTypeName("WORD")]
        public ushort fUnicode;
    }

    public unsafe partial struct _UNLOAD_DLL_DEBUG_INFO
    {
        [NativeTypeName("LPVOID")]
        public void* lpBaseOfDll;
    }

    public unsafe partial struct _OUTPUT_DEBUG_STRING_INFO
    {
        [NativeTypeName("LPSTR")]
        public sbyte* lpDebugStringData;

        [NativeTypeName("WORD")]
        public ushort fUnicode;

        [NativeTypeName("WORD")]
        public ushort nDebugStringLength;
    }

    public partial struct _RIP_INFO
    {
        [NativeTypeName("DWORD")]
        public uint dwError;

        [NativeTypeName("DWORD")]
        public uint dwType;
    }

    public static partial class Methods
    {
        [NativeTypeName("#define STRICT 1")]
        public const int STRICT = 1;

        [NativeTypeName("#define MAX_PATH 260")]
        public const int MAX_PATH = 260;

        [NativeTypeName("#define FALSE 0")]
        public const int FALSE = 0;

        [NativeTypeName("#define TRUE 1")]
        public const int TRUE = 1;

        [NativeTypeName("#define FIND_FIRST_EX_CASE_SENSITIVE 0x00000001")]
        public const int FIND_FIRST_EX_CASE_SENSITIVE = 0x00000001;

        [NativeTypeName("#define FIND_FIRST_EX_LARGE_FETCH 0x00000002")]
        public const int FIND_FIRST_EX_LARGE_FETCH = 0x00000002;

        [NativeTypeName("#define FIND_FIRST_EX_ON_DISK_ENTRIES_ONLY 0x00000004")]
        public const int FIND_FIRST_EX_ON_DISK_ENTRIES_ONLY = 0x00000004;

        [NativeTypeName("#define LOCKFILE_FAIL_IMMEDIATELY 0x00000001")]
        public const int LOCKFILE_FAIL_IMMEDIATELY = 0x00000001;

        [NativeTypeName("#define LOCKFILE_EXCLUSIVE_LOCK 0x00000002")]
        public const int LOCKFILE_EXCLUSIVE_LOCK = 0x00000002;

        [NativeTypeName("#define PROCESS_HEAP_REGION 0x0001")]
        public const int PROCESS_HEAP_REGION = 0x0001;

        [NativeTypeName("#define PROCESS_HEAP_UNCOMMITTED_RANGE 0x0002")]
        public const int PROCESS_HEAP_UNCOMMITTED_RANGE = 0x0002;

        [NativeTypeName("#define PROCESS_HEAP_ENTRY_BUSY 0x0004")]
        public const int PROCESS_HEAP_ENTRY_BUSY = 0x0004;

        [NativeTypeName("#define PROCESS_HEAP_SEG_ALLOC 0x0008")]
        public const int PROCESS_HEAP_SEG_ALLOC = 0x0008;

        [NativeTypeName("#define PROCESS_HEAP_ENTRY_MOVEABLE 0x0010")]
        public const int PROCESS_HEAP_ENTRY_MOVEABLE = 0x0010;

        [NativeTypeName("#define PROCESS_HEAP_ENTRY_DDESHARE 0x0020")]
        public const int PROCESS_HEAP_ENTRY_DDESHARE = 0x0020;

        [NativeTypeName("#define EXCEPTION_DEBUG_EVENT 1")]
        public const int EXCEPTION_DEBUG_EVENT = 1;

        [NativeTypeName("#define CREATE_THREAD_DEBUG_EVENT 2")]
        public const int CREATE_THREAD_DEBUG_EVENT = 2;

        [NativeTypeName("#define CREATE_PROCESS_DEBUG_EVENT 3")]
        public const int CREATE_PROCESS_DEBUG_EVENT = 3;

        [NativeTypeName("#define EXIT_THREAD_DEBUG_EVENT 4")]
        public const int EXIT_THREAD_DEBUG_EVENT = 4;

        [NativeTypeName("#define EXIT_PROCESS_DEBUG_EVENT 5")]
        public const int EXIT_PROCESS_DEBUG_EVENT = 5;

        [NativeTypeName("#define LOAD_DLL_DEBUG_EVENT 6")]
        public const int LOAD_DLL_DEBUG_EVENT = 6;

        [NativeTypeName("#define UNLOAD_DLL_DEBUG_EVENT 7")]
        public const int UNLOAD_DLL_DEBUG_EVENT = 7;

        [NativeTypeName("#define OUTPUT_DEBUG_STRING_EVENT 8")]
        public const int OUTPUT_DEBUG_STRING_EVENT = 8;

        [NativeTypeName("#define RIP_EVENT 9")]
        public const int RIP_EVENT = 9;

        [NativeTypeName("#define STILL_ACTIVE STATUS_PENDING")]
        public const uint STILL_ACTIVE = ((uint)(0x00000103));

        [NativeTypeName("#define EXCEPTION_ACCESS_VIOLATION STATUS_ACCESS_VIOLATION")]
        public const uint EXCEPTION_ACCESS_VIOLATION = ((uint)(0xC0000005));

        [NativeTypeName("#define EXCEPTION_DATATYPE_MISALIGNMENT STATUS_DATATYPE_MISALIGNMENT")]
        public const uint EXCEPTION_DATATYPE_MISALIGNMENT = ((uint)(0x80000002));

        [NativeTypeName("#define EXCEPTION_BREAKPOINT STATUS_BREAKPOINT")]
        public const uint EXCEPTION_BREAKPOINT = ((uint)(0x80000003));

        [NativeTypeName("#define EXCEPTION_SINGLE_STEP STATUS_SINGLE_STEP")]
        public const uint EXCEPTION_SINGLE_STEP = ((uint)(0x80000004));

        [NativeTypeName("#define EXCEPTION_ARRAY_BOUNDS_EXCEEDED STATUS_ARRAY_BOUNDS_EXCEEDED")]
        public const uint EXCEPTION_ARRAY_BOUNDS_EXCEEDED = ((uint)(0xC000008C));

        [NativeTypeName("#define EXCEPTION_FLT_DENORMAL_OPERAND STATUS_FLOAT_DENORMAL_OPERAND")]
        public const uint EXCEPTION_FLT_DENORMAL_OPERAND = ((uint)(0xC000008D));

        [NativeTypeName("#define EXCEPTION_FLT_DIVIDE_BY_ZERO STATUS_FLOAT_DIVIDE_BY_ZERO")]
        public const uint EXCEPTION_FLT_DIVIDE_BY_ZERO = ((uint)(0xC000008E));

        [NativeTypeName("#define EXCEPTION_FLT_INEXACT_RESULT STATUS_FLOAT_INEXACT_RESULT")]
        public const uint EXCEPTION_FLT_INEXACT_RESULT = ((uint)(0xC000008F));

        [NativeTypeName("#define EXCEPTION_FLT_INVALID_OPERATION STATUS_FLOAT_INVALID_OPERATION")]
        public const uint EXCEPTION_FLT_INVALID_OPERATION = ((uint)(0xC0000090));

        [NativeTypeName("#define EXCEPTION_FLT_OVERFLOW STATUS_FLOAT_OVERFLOW")]
        public const uint EXCEPTION_FLT_OVERFLOW = ((uint)(0xC0000091));

        [NativeTypeName("#define EXCEPTION_FLT_STACK_CHECK STATUS_FLOAT_STACK_CHECK")]
        public const uint EXCEPTION_FLT_STACK_CHECK = ((uint)(0xC0000092));

        [NativeTypeName("#define EXCEPTION_FLT_UNDERFLOW STATUS_FLOAT_UNDERFLOW")]
        public const uint EXCEPTION_FLT_UNDERFLOW = ((uint)(0xC0000093));

        [NativeTypeName("#define EXCEPTION_INT_DIVIDE_BY_ZERO STATUS_INTEGER_DIVIDE_BY_ZERO")]
        public const uint EXCEPTION_INT_DIVIDE_BY_ZERO = ((uint)(0xC0000094));

        [NativeTypeName("#define EXCEPTION_INT_OVERFLOW STATUS_INTEGER_OVERFLOW")]
        public const uint EXCEPTION_INT_OVERFLOW = ((uint)(0xC0000095));

        [NativeTypeName("#define EXCEPTION_PRIV_INSTRUCTION STATUS_PRIVILEGED_INSTRUCTION")]
        public const uint EXCEPTION_PRIV_INSTRUCTION = ((uint)(0xC0000096));

        [NativeTypeName("#define EXCEPTION_IN_PAGE_ERROR STATUS_IN_PAGE_ERROR")]
        public const uint EXCEPTION_IN_PAGE_ERROR = ((uint)(0xC0000006));

        [NativeTypeName("#define EXCEPTION_ILLEGAL_INSTRUCTION STATUS_ILLEGAL_INSTRUCTION")]
        public const uint EXCEPTION_ILLEGAL_INSTRUCTION = ((uint)(0xC000001D));

        [NativeTypeName("#define EXCEPTION_NONCONTINUABLE_EXCEPTION STATUS_NONCONTINUABLE_EXCEPTION")]
        public const uint EXCEPTION_NONCONTINUABLE_EXCEPTION = ((uint)(0xC0000025));

        [NativeTypeName("#define EXCEPTION_STACK_OVERFLOW STATUS_STACK_OVERFLOW")]
        public const uint EXCEPTION_STACK_OVERFLOW = ((uint)(0xC00000FD));

        [NativeTypeName("#define EXCEPTION_INVALID_DISPOSITION STATUS_INVALID_DISPOSITION")]
        public const uint EXCEPTION_INVALID_DISPOSITION = ((uint)(0xC0000026));

        [NativeTypeName("#define EXCEPTION_GUARD_PAGE STATUS_GUARD_PAGE_VIOLATION")]
        public const uint EXCEPTION_GUARD_PAGE = ((uint)(0x80000001));

        [NativeTypeName("#define EXCEPTION_INVALID_HANDLE STATUS_INVALID_HANDLE")]
        public const uint EXCEPTION_INVALID_HANDLE = ((uint)(0xC0000008));

        [NativeTypeName("#define CONTROL_C_EXIT STATUS_CONTROL_C_EXIT")]
        public const uint CONTROL_C_EXIT = ((uint)(0xC000013A));

        [NativeTypeName("#define LMEM_FIXED 0x0000")]
        public const int LMEM_FIXED = 0x0000;

        [NativeTypeName("#define LMEM_MOVEABLE 0x0002")]
        public const int LMEM_MOVEABLE = 0x0002;

        [NativeTypeName("#define LMEM_NOCOMPACT 0x0010")]
        public const int LMEM_NOCOMPACT = 0x0010;

        [NativeTypeName("#define LMEM_NODISCARD 0x0020")]
        public const int LMEM_NODISCARD = 0x0020;

        [NativeTypeName("#define LMEM_ZEROINIT 0x0040")]
        public const int LMEM_ZEROINIT = 0x0040;

        [NativeTypeName("#define LMEM_MODIFY 0x0080")]
        public const int LMEM_MODIFY = 0x0080;

        [NativeTypeName("#define LMEM_DISCARDABLE 0x0F00")]
        public const int LMEM_DISCARDABLE = 0x0F00;

        [NativeTypeName("#define LMEM_VALID_FLAGS 0x0F72")]
        public const int LMEM_VALID_FLAGS = 0x0F72;

        [NativeTypeName("#define LMEM_INVALID_HANDLE 0x8000")]
        public const int LMEM_INVALID_HANDLE = 0x8000;

        [NativeTypeName("#define LHND (LMEM_MOVEABLE | LMEM_ZEROINIT)")]
        public const int LHND = (0x0002 | 0x0040);

        [NativeTypeName("#define LPTR (LMEM_FIXED | LMEM_ZEROINIT)")]
        public const int LPTR = (0x0000 | 0x0040);

        [NativeTypeName("#define NONZEROLHND (LMEM_MOVEABLE)")]
        public const int NONZEROLHND = (0x0002);

        [NativeTypeName("#define NONZEROLPTR (LMEM_FIXED)")]
        public const int NONZEROLPTR = (0x0000);

        [NativeTypeName("#define LMEM_DISCARDED 0x4000")]
        public const int LMEM_DISCARDED = 0x4000;

        [NativeTypeName("#define LMEM_LOCKCOUNT 0x00FF")]
        public const int LMEM_LOCKCOUNT = 0x00FF;

        [NativeTypeName("#define NUMA_NO_PREFERRED_NODE ((DWORD) -1)")]
        public const uint NUMA_NO_PREFERRED_NODE = unchecked((uint)(-1));
    }
}
