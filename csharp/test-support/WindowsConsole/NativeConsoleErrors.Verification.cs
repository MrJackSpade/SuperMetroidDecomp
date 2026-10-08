using System.Runtime.InteropServices;

/// <summary>Verifier-side check of the process error policy; tools only install it.</summary>
internal static partial class NativeConsoleErrors
{
    [LibraryImport("kernel32.dll")]
    private static partial uint GetErrorMode();

    /// <summary>Verifier-only assertion of the actual process policy, before an intentional boundary failure.</summary>
    internal static void VerifyDialogsDisabled()
    {
        const uint required = ErrorModes.FailCriticalErrors | ErrorModes.NoGpFaultErrorBox | ErrorModes.NoOpenFileErrorBox;
        if ((GetErrorMode() & required) != required)
            throw new InvalidOperationException("Console process did not install the required Windows no-dialog policy.");
    }
}
