namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>How the opening's narration text glows in: the pool of glowing rectangles and their colour variants.</summary>
internal static class CinematicTextGlowDefinitions
{
    /// <summary>Rectangles that can glow at once; a request while all are busy is dropped.</summary>
    internal const int SlotCount = 8;
    /// <summary>Updates each colour variant of a glowing rectangle lasts.</summary>
    internal const int VariantDuration = 5;
    /// <summary>The fourth and final colour variant, after which the rectangle stops glowing.</summary>
    internal const int FinalVariant = 3;
}
