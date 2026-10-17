namespace SuperMetroid.TestSupport.WindowsConsole;

/// <summary>Win32 SetErrorMode bits (winbase.h <c>SEM_*</c>), composed by OR and tested by mask; documented independently of renderer behavior.</summary>
[Flags]
internal enum ErrorModes : uint
{
    /// <summary>Win32 SEM_FAILCRITICALERRORS; sends critical failures to the process instead of a system dialog.</summary>
    FailCriticalErrors = 0x0001,
    /// <summary>Win32 SEM_NOGPFAULTERRORBOX; suppresses the general-protection error box.</summary>
    NoGpFaultErrorBox = 0x0002,
    /// <summary>Win32 SEM_NOOPENFILEERRORBOX; suppresses the missing-file error box.</summary>
    NoOpenFileErrorBox = 0x8000,
}
