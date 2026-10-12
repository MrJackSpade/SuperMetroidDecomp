using System.Runtime.InteropServices;

/// <summary>Console diagnostics must report failures to stderr, never show native focus-stealing dialogs.</summary>
internal static partial class NativeConsoleErrors
{
    /// <summary>set error mode.</summary>
    [LibraryImport("kernel32.dll")]
    private static partial uint SetErrorMode(uint mode);

    /// <summary>disable dialogs.</summary>
    internal static void DisableDialogs() => SetErrorMode(ErrorModes.FailCriticalErrors | ErrorModes.NoGpFaultErrorBox | ErrorModes.NoOpenFileErrorBox);

}

/// <summary>Win32 SetErrorMode bits; documented independently of renderer behavior.</summary>
internal static class ErrorModes
{
    /// <summary>Stores fail critical errors.</summary>
    internal const uint FailCriticalErrors = 0x0001;
    /// <summary>Stores no gp fault error box.</summary>
    internal const uint NoGpFaultErrorBox = 0x0002;
    /// <summary>Stores no open file error box.</summary>
    internal const uint NoOpenFileErrorBox = 0x8000;
}
