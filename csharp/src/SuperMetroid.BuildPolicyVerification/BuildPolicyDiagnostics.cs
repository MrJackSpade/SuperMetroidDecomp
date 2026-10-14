namespace SuperMetroid.BuildPolicyVerification;

/// <summary>The diagnostics #142 makes build-breaking in every maintained C# project.</summary>
internal static class BuildPolicyDiagnostics
{
    /// <summary>The gated IDs, each with a violating and a compliant fixture file.</summary>
    internal static readonly string[] Gated =
    [
        "IDE0211", "CA1512", "CA1822", "CA1859", "CA1826",
        "IDE0032", "IDE0060", "IDE0078", "SYSLIB1054", "IDE0330",
    ];
}
