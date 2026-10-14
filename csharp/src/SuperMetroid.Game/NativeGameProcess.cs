using System.Runtime.InteropServices;

/// <summary>Named Win32 process-error policy for the playable executable.</summary>
static partial class NativeGameProcess
{
    /// <summary>Prevents Windows from displaying critical-error dialogs for process I/O faults.</summary>
    internal const uint SemFailCriticalErrors = 0x0001;
    /// <summary>Prevents the Windows general-protection-fault dialog from taking focus.</summary>
    internal const uint SemNoGpFaultErrorBox = 0x0002;
    /// <summary>Prevents Windows from displaying a file-open error dialog.</summary>
    internal const uint SemNoOpenFileErrorBox = 0x8000;

    /// <summary>Applies the Win32 process error mode before fallible game startup work.</summary>
    [LibraryImport("kernel32.dll")]
    internal static partial uint SetErrorMode(uint errorMode);
}
