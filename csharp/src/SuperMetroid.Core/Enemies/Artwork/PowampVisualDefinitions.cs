using SuperMetroid.Core.Enemies.Powamp;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Powamp's ordinary artwork declarations; behavior and timing live in its typed programs.</summary>
internal static class PowampVisualDefinitions
{
    /// <summary>Powamp instruction and ordinary OAM bank $A8.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Six Powamp body and balloon compositions at $A8:C675..C698.</summary>
    internal const int FrameCount = 6;

    /// <summary>Builds the six frame definitions from the poses Powamp's programs show, in authored order.</summary>
    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.FramesFromSpritemaps(Bank, PowampAnimations.Spritemaps(Enum.GetValues<PowampProgram>()), FrameCount, "powamp");
}
