using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Shitroid's ordinary artwork declarations; behavior and timing stay compiled.</summary>
internal static class ShitroidVisualDefinitions
{
    /// <summary>Shitroid instruction and ordinary OAM bank $A9.</summary>
    internal const byte Bank = 0xa9;
    /// <summary>Three Tourian baby Metroid drain and remorse compositions at $A9:F9A8..FAD8.</summary>
    internal const int FrameCount = 3;

    /// <summary>The spritemaps the typed Shitroid segments show, in segment order.</summary>
    internal static ushort[] Spritemaps() => [.. Enum.GetValues<ShitroidSegment>()
        .SelectMany(program => ShitroidAnimations.Segments.Beats(program))
        .OfType<AnimationBeat<BabyMetroidFrame, ShitroidCue>.Frame>()
        .Select(beat => BabyMetroidVisualToolingDefinitions.Spritemap(beat.Shown))];
}
