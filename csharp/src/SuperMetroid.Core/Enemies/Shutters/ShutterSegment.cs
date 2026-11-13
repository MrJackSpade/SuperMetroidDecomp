namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The growing and vertical shutters' poses and Kamer's platform loop as typed animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="ShutterAnimations"/>.
/// </summary>
internal enum ShutterSegment
{
    /// <summary>The 10-pixel growing shutter animation.</summary>
    GrowingTenPixels,
    /// <summary>The 20-pixel growing shutter animation.</summary>
    GrowingTwentyPixels,
    /// <summary>The 30-pixel growing shutter animation.</summary>
    GrowingThirtyPixels,
    /// <summary>The 40-pixel growing shutter animation.</summary>
    FortyPixels,
    /// <summary>The Kamer platform animation.</summary>
    KamerPlatform,
}
