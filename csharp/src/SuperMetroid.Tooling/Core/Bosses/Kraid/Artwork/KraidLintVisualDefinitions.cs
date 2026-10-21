
namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Kraid's ordinary belly-lint compositions; timing and actor motion remain compiled.</summary>
internal static class KraidLintVisualDefinitions
{
    /// <summary>Native Kraid instruction and artwork bank $A7.</summary>
    internal const byte Bank = KraidFootVisualToolingDefinitions.Bank;
    /// <summary>Initial and post-growth ordinary OAM frames at $A7:A5DF and $A7:8C6C.</summary>
    internal const int FrameCount = 2;

    /// <summary>The spritemaps Kraid's typed lint programs show, in first-shown order.</summary>
    internal static ushort[] Spritemaps() => [.. KraidAnimationsTooling.Spritemaps(KraidPartProgram.LintInitial, KraidPartProgram.LintPostGrowth)];
}
