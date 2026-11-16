namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>How an Alcoon waits underground, jumps out, walks and jumps back in (#2149).</summary>
internal static class AlcoonMotionDefinitions
{
    /// <summary>
    /// How close Samus comes vertically to the Alcoon's landing point, in pixels and exclusive, before it can jump out.
    /// </summary>
    internal const int EmergeYDistance = 32;

    /// <summary>How close Samus comes horizontally to the Alcoon, in pixels and exclusive, before it jumps out.</summary>
    internal const int EmergeXDistance = 80;

    /// <summary>How far from its spawn point an emerged Alcoon walks, in pixels, before it jumps back underground.</summary>
    internal const int HideXDistance = 112;

    /// <summary>The upward speed an Alcoon jumps out of the ground at: twelve pixels an update.</summary>
    internal static SignedSixteenSixteen EmergeJumpVelocity { get; } = SignedSixteenSixteen.FromWholePixels(-12);

    /// <summary>The upward speed an Alcoon jumps back underground at: four pixels an update.</summary>
    internal static SignedSixteenSixteen HideJumpVelocity { get; } = SignedSixteenSixteen.FromWholePixels(-4);

    /// <summary>How much an airborne Alcoon's vertical speed gains each update: half a pixel an update.</summary>
    internal static SignedSixteenSixteen Gravity { get; } = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(128);

    /// <summary>How far a walking Alcoon moves down each update to keep to the floor, or to fall where the floor ends.</summary>
    internal static SignedSixteenSixteen FloorPress { get; } = SignedSixteenSixteen.FromWholePixels(2);
}
