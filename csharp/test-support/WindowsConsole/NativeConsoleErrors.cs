using System.Runtime.InteropServices;

/// <summary>Console diagnostics must report failures to stderr, never show native focus-stealing dialogs.</summary>
internal static partial class NativeConsoleErrors
{
    [LibraryImport("kernel32.dll")]
    private static partial uint SetErrorMode(uint mode);

    [LibraryImport("kernel32.dll")]
    private static partial uint GetErrorMode();

    internal static void DisableDialogs() => SetErrorMode(ErrorModes.FailCriticalErrors | ErrorModes.NoGpFaultErrorBox | ErrorModes.NoOpenFileErrorBox);

    /// <summary>Verifier-only assertion of the actual process policy, before an intentional boundary failure.</summary>
    internal static void VerifyDialogsDisabled()
    {
        const uint required = ErrorModes.FailCriticalErrors | ErrorModes.NoGpFaultErrorBox | ErrorModes.NoOpenFileErrorBox;
        if ((GetErrorMode() & required) != required)
            throw new InvalidOperationException("Console process did not install the required Windows no-dialog policy.");
    }
}

/// <summary>Win32 SetErrorMode bits; documented independently of renderer behavior.</summary>
internal static class ErrorModes
{
    internal const uint FailCriticalErrors = 0x0001;
    internal const uint NoGpFaultErrorBox = 0x0002;
    internal const uint NoOpenFileErrorBox = 0x8000;
}
