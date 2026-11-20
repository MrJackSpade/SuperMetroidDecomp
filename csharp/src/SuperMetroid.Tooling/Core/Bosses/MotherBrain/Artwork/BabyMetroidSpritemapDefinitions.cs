using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Baby Metroid's ordinary artwork declarations; behavior and timing stay compiled.</summary>
internal static class BabyMetroidSpritemapDefinitions
{
    /// <summary>Baby Metroid instruction and ordinary OAM bank $A9.</summary>
    internal const byte Bank = 0xa9;
    /// <summary>Three Tourian baby Metroid drain and remorse compositions at $A9:F9A8..FAD8.</summary>
    internal const int FrameCount = 3;

    /// <summary>The spritemaps the typed Baby Metroid segments show, in segment order.</summary>
    internal static ushort[] Spritemaps() => [.. Enum.GetValues<BabyMetroidSegment>()
        .SelectMany(program => BabyMetroidAnimations.Segments.Beats(program))
        .OfType<AnimationBeat<BabyMetroidFrame, BabyMetroidCue>.Frame>()
        .Select(beat => BabyMetroidVisualToolingDefinitions.Spritemap(beat.Shown))];
}
