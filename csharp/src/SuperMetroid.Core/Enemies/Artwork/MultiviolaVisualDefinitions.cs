using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Eight distinct bank-$A2 OAM compositions in Multiviola's fourteen-frame
/// flight loop. Durations and the loop branch live in its typed program.
/// </summary>
internal static class MultiviolaVisualDefinitions
{
    /// <summary>Native Multiviola spritemap bank $A2.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>Eight distinct compositions selected by the fourteen-frame flight loop.</summary>
    internal const int FrameCount = 8;

    /// <summary>Builds the export list from the flight loop's first pass out, preserving loop order.</summary>
    internal static EnemySpritemapDefinition[] Frames() =>
        [.. MultiviolaAnimations.Spritemaps(MultiviolaProgram.Flying).Take(FrameCount)
            .Select((frame, index) => new EnemySpritemapDefinition(Bank, frame, $"multiviola_spin_{index}"))];
}
