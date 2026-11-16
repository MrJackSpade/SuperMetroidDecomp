namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// The horizontal speeds of a Zoa's launch: at rest before the shooting animation's first cue, then each speed its
/// cues select in turn; how near Samus brings a hidden Zoa up and how fast it rises (#2156).
/// </summary>
public static class ZoaSpeedDefinitions
{
    /// <summary>The launch's first speed, half a pixel per frame.</summary>
    public static SignedSixteenSixteen SlowLaunchSpeed { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(8);

    /// <summary>The launch's second speed, five eighths of a pixel per frame.</summary>
    public static SignedSixteenSixteen MiddleLaunchSpeed { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(10);

    /// <summary>The launch's last speed, two pixels per frame.</summary>
    public static SignedSixteenSixteen FastLaunchSpeed { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(32);

    /// <summary>How close horizontally Samus comes, in pixels and exclusive, for a hidden Zoa to rise.</summary>
    internal const int ActivationDistance = 128;

    /// <summary>How fast a Zoa rises toward Samus's height, half a pixel an update.</summary>
    internal static SignedSixteenSixteen RisingSpeed { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(8);
}
