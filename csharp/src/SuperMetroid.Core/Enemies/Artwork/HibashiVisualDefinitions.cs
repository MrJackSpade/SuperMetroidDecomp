using SuperMetroid.Core.Enemies.Hibashi;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Hibashi's ordinary artwork declarations: the poses its typed programs show.</summary>
internal static class HibashiVisualDefinitions
{
    /// <summary>Hibashi instruction and ordinary OAM bank $A6.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>The fire pillar's twenty-three selected OAM frames at $A6:9082..9469.</summary>
    internal const int FrameCount = 23;

    /// <summary>Resolves the twenty-three bank-$A6 OAM compositions used for Hibashi's fire-pillar presentation.</summary>
    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.FramesFromSpritemaps(Bank,
            HibashiAnimations.Spritemaps(Enum.GetValues<HibashiProgram>()), FrameCount, "hibashi");
}

