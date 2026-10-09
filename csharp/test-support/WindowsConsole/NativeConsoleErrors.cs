using System.Runtime.InteropServices;

/// <summary>Console diagnostics must report failures to stderr, never show native focus-stealing dialogs.</summary>
internal static partial class NativeConsoleErrors
{
    /// <summary>Sets the calling process's Windows error-reporting mode and returns its previous mode.</summary>
    [LibraryImport("kernel32.dll")]
    private static partial uint SetErrorMode(uint mode);

    /// <summary>Disables critical-error, access-violation, and missing-file dialogs for console hosts.</summary>
    internal static void DisableDialogs() => SetErrorMode(ErrorModes.FailCriticalErrors | ErrorModes.NoGpFaultErrorBox | ErrorModes.NoOpenFileErrorBox);

}

/// <summary>Win32 SetErrorMode bits; documented independently of renderer behavior.</summary>
internal static class ErrorModes
{
    /// <summary>Prevents Windows from displaying a critical-error dialog when a device cannot be found.</summary>
    internal const uint FailCriticalErrors = 0x0001;
    /// <summary>Suppresses the system error box for an unhandled general-protection fault.</summary>
    internal const uint NoGpFaultErrorBox = 0x0002;
    /// <summary>Suppresses the dialog shown when Windows cannot find a requested file.</summary>
    internal const uint NoOpenFileErrorBox = 0x8000;
}
