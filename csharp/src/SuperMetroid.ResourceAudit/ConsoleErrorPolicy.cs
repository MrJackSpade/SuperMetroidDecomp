namespace SuperMetroid.ResourceAudit;

/// <summary>Win32 SetErrorMode bits matching the repository console process policy.</summary>
internal static class ConsoleErrorPolicy
{
    /// <summary>SEM_FAILCRITICALERRORS: report critical errors through the console boundary.</summary>
    public const uint FailCriticalErrors = 0x0001;
    /// <summary>SEM_NOGPFAULTERRORBOX: no Windows fault-reporting dialog.</summary>
    public const uint NoFaultDialog = 0x0002;
    /// <summary>SEM_NOOPENFILEERRORBOX: file errors never prompt through legacy Windows UI.</summary>
    public const uint NoOpenFileDialog = 0x8000;
}
