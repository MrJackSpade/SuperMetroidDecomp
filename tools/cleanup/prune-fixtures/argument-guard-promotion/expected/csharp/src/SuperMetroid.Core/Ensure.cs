namespace SuperMetroid.Core;

/// <summary>The fixture's stand-in for the repository's argument checks, whose presence turns SME6201 on.</summary>
internal static class Ensure
{
    /// <summary>The checks' name, which production reports.</summary>
    internal static string Name => nameof(Ensure);
}
