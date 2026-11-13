namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The growing, vertical and horizontal shutters' poses and Kamer's platform loop as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
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
    /// <summary>The horizontal animation.</summary>
    Horizontal,
    /// <summary>The Kamer platform animation.</summary>
    KamerPlatform,
}
