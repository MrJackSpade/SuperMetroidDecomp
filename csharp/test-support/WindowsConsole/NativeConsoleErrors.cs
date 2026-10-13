using System.Runtime.InteropServices;

/// <summary>Console diagnostics must report failures to stderr, never show native focus-stealing dialogs.</summary>
internal static partial class NativeConsoleErrors
{
    /// <summary>Sets the Win32 process error mode before any fallible console work begins.</summary>
    [LibraryImport("kernel32.dll")]
    private static partial uint SetErrorMode(uint mode);

    /// <summary>Suppresses critical-error, general-protection, and missing-file Windows dialogs.</summary>
    internal static void DisableDialogs() => SetErrorMode(ErrorModes.FailCriticalErrors | ErrorModes.NoGpFaultErrorBox | ErrorModes.NoOpenFileErrorBox);

}

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
