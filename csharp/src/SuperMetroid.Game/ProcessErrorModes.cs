namespace SuperMetroid.Game;

/// <summary>The Win32 <c>SetErrorMode</c> flags the game sets (winbase.h <c>SEM_*</c>).</summary>
[Flags]
internal enum ProcessErrorModes : uint
{
    /// <summary>No flag.</summary>
    None = 0,
    /// <summary><c>SEM_FAILCRITICALERRORS</c>: no critical-error dialogs for process I/O faults.</summary>
    FailCriticalErrors = 0x0001,
    /// <summary><c>SEM_NOGPFAULTERRORBOX</c>: no general-protection-fault dialog taking focus.</summary>
    NoGpFaultErrorBox = 0x0002,
    /// <summary><c>SEM_NOOPENFILEERRORBOX</c>: no file-open error dialog.</summary>
    NoOpenFileErrorBox = 0x8000,
}
