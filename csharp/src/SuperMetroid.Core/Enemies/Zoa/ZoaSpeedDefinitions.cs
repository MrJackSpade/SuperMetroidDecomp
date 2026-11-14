namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// The horizontal speeds of a Zoa's launch: at rest before the shooting animation's first cue, then each speed its
/// cues select in turn.
/// </summary>
public static class ZoaSpeedDefinitions
{
    /// <summary>The launch's first speed, half a pixel per frame.</summary>
    public static SignedSixteenSixteen SlowLaunchSpeed { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(8);

    /// <summary>The launch's second speed, five eighths of a pixel per frame.</summary>
    public static SignedSixteenSixteen MiddleLaunchSpeed { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(10);

    /// <summary>The launch's last speed, two pixels per frame.</summary>
    public static SignedSixteenSixteen FastLaunchSpeed { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(32);
}
