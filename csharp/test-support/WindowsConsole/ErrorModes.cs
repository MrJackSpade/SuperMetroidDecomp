/// <summary>Win32 SetErrorMode bits; documented independently of renderer behavior.</summary>
internal static class ErrorModes
{
    /// <summary>Win32 SEM_FAILCRITICALERRORS; sends critical failures to the process instead of a system dialog.</summary>
    internal const uint FailCriticalErrors = 0x0001;
    /// <summary>Win32 SEM_NOGPFAULTERRORBOX; suppresses the general-protection error box.</summary>
    internal const uint NoGpFaultErrorBox = 0x0002;
    /// <summary>Win32 SEM_NOOPENFILEERRORBOX; suppresses the missing-file error box.</summary>
    internal const uint NoOpenFileErrorBox = 0x8000;
}
