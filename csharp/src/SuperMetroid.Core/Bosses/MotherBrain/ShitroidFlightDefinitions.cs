namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How sharply the Tourian Shitroid pulls toward each point it flies to: the divisor its gradual acceleration divides the
/// distance to the point by (a larger divisor is a gentler pull), the extra push a reversal takes while it is off
/// screen, the push a spin-jumping Samus gives it and how it closes on a latch.
/// </summary>
internal static class ShitroidFlightDefinitions
{
    /// <summary>The pull of a rush (to the middle of the room, onto its Sidehopper and onto Samus): the whole distance.</summary>
    internal const int RushDivisor = 1;

    /// <summary>The pull of its stare-down while it hovers near Samus: a sixth of the distance.</summary>
    internal const int StareDownDivisor = 6;

    /// <summary>The pull of its remorseful follow of the released Samus: an eighth of the distance.</summary>
    internal const int RemorseDivisor = 8;

    /// <summary>
    /// The pull of a drift (rising after it feeds and after it drains Samus, backing off left and right, and fleeing): a
    /// sixteenth of the distance.
    /// </summary>
    internal const int DriftDivisor = 16;

    /// <summary>The extra speed a reversal adds while the Shitroid is off screen: four pixels an update.</summary>
    internal static readonly SignedEightEight OffScreenReversalKick = SignedEightEight.FromPixels(4);

    /// <summary>The speed a spin-jumping Samus adds to the Shitroid's velocity along the negated bearing to her: a quarter pixel an update.</summary>
    internal static readonly SignedEightEight ContactPushSpeed = SignedEightEight.FromPixelFraction(1, 4);

    /// <summary>The acceleration of the Shitroid's approach to a latch, on each axis.</summary>
    internal static readonly SignedEightEight LatchAcceleration = SignedEightEight.FromPixels(2);

    /// <summary>The fastest the Shitroid approaches a latch, on each axis, in either direction.</summary>
    internal static readonly SignedEightEight LatchMaximumSpeed = SignedEightEight.FromPixels(5);
}
