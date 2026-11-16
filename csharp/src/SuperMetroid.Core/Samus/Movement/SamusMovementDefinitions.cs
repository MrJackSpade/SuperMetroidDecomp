namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The definitions Samus's movement and pose selection read: speeds, timers, frames and sentinels (#1411).</summary>
/// <remarks>
/// Definitions are grouped by the part of Samus's movement they define. Her live velocity, timers and the selected
/// movement do not belong in this catalog.
/// </remarks>
public static class SamusMovementDefinitions
{
    /// <summary>Ball landing: signed whole-speed threshold for the first automatic bounce.</summary>
    public const int FirstBallBounceMinimumSpeed = 3;

    /// <summary>Standing movement's held-shot animation timer before the animation tick.</summary>
    public const int StandingShotAnimationTimer = 16;

    /// <summary>Falling movement's minimum signed whole speed selecting fast-fall art.</summary>
    public const int FastFallAnimationSpeed = 5;
    /// <summary>Falling movement: fast-fall timer before the animation tick.</summary>
    public const int FastFallAnimationTimer = 8;

    /// <summary>
    /// The least whole downward speed at which Samus's landing is a hard landing, with its own sound; a slower landing
    /// with any fraction of speed is a soft one.
    /// </summary>
    public const int HardLandingWholeSpeed = 5;

    /// <summary>
    /// How far behind a spinning Samus a wall may lie for a wall jump, in pixels: the probe looks this far, and only a
    /// wall it finds closer launches her.
    /// </summary>
    public const int WallJumpReachPixels = 8;

    /// <summary>The pixels an update each speed echo travels away from Samus in her facing direction once the echoes depart.</summary>
    public const int SpeedEchoDepartureSpeed = 8;

    /// <summary>The pixels an update a departing speed echo closes on Samus's height until it reaches it.</summary>
    public const int SpeedEchoVerticalApproach = 2;

    /// <summary>Horizontal acceleration, Speed Booster, and animation cadence tables.</summary>
    public static class HorizontalMotion
    {
        /// <summary>Per-frame extra-run acceleration while charging the Speed Booster: 1/16 pixel.</summary>
        public static readonly UnsignedSixteenSixteen ExtraRunAcceleration = UnsignedSixteenSixteen.FromSixteenthsOfPixel(1);
    }
}
