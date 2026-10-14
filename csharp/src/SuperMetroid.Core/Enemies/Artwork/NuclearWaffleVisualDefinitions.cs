using SuperMetroid.Core.Enemies.Nuclear;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Puromi/Nuclear Waffle's ordinary head compositions, separate from its projectile links.</summary>
internal static class NuclearWaffleVisualDefinitions
{
    /// <summary>Native Puromi instruction and head OAM bank $A6.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>Eight distinct head frames at $A6:9954..9985 selected by the twelve-frame loop.</summary>
    internal const int FrameCount = 8;

    /// <summary>The head compositions Puromi's typed body animation (#641) shows, in order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps() => NuclearWaffleAnimations.Spritemaps(NuclearWaffleProgram.BodyLoop);

    /// <summary>Resolves the eight bank-$A6 head compositions used by Puromi's twelve-step animation loop.</summary>
    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.FramesFromSpritemaps(Bank, Spritemaps(), FrameCount, "nuclear_waffle");
}
