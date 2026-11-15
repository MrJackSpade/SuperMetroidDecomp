namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>How the opening's narration letters glow in: each glow's colour variants and how long each lasts.</summary>
internal static class CinematicTextGlowDefinitions
{
    /// <summary>Updates each colour variant of a glowing rectangle lasts.</summary>
    internal const int VariantDuration = 5;
    /// <summary>The fourth and final colour variant, after which the rectangle stops glowing.</summary>
    internal const int FinalVariant = 3;
}
