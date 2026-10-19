using System.Runtime.InteropServices;

namespace SuperMetroid.TestSupport.WindowsConsole.Verification;

/// <summary>Verifier-side check of the process error policy; tools only install it.</summary>
internal static partial class NativeConsoleErrorPolicyCheck
{
    /// <summary>Reads the process-wide Win32 dialog suppression modes for the verifier assertion.</summary>
    [LibraryImport("kernel32.dll")]
    private static partial ErrorModes GetErrorMode();

    /// <summary>Verifier-only assertion of the actual process policy, before an intentional boundary failure.</summary>
    internal static void VerifyDialogsDisabled()
    {
        const ErrorModes required = ErrorModes.FailCriticalErrors | ErrorModes.NoGpFaultErrorBox | ErrorModes.NoOpenFileErrorBox;
        if (!GetErrorMode().HasFlag(required))
            throw new InvalidOperationException("Console process did not install the required Windows no-dialog policy.");
    }
}
