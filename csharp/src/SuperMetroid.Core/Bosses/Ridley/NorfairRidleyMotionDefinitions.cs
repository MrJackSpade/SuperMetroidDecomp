namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Lower Norfair Ridley's pogo bounces and how he lifts a grabbed Samus (#2171).</summary>
internal static class NorfairRidleyMotionDefinitions
{
    /// <summary>The sideways speed a pogo bounce starts from when Ridley has none: three quarters of a pixel an update.</summary>
    internal static readonly SignedEightEight PogoRestartSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(192);

    /// <summary>The fastest Ridley falls on a pogo bounce: six pixels an update.</summary>
    internal static readonly SignedEightEight PogoMaximumFallSpeed = SignedEightEight.FromPixels(6);

    /// <summary>The least upward speed Ridley lifts a grabbed Samus with: two pixels an update.</summary>
    internal static readonly SignedEightEight GrabLiftMinimumSpeed = SignedEightEight.FromPixels(2);
}
