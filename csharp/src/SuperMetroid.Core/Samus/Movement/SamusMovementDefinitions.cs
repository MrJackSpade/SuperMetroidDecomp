using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The definitions Samus's movement and pose selection read: speeds, timers, frames and sentinels (#1411).</summary>
/// <remarks>
/// Definitions are grouped by their subsystem rather than by the C# class that happens
/// to read them. Runtime velocity, timers, and the selected movement do not belong in this catalog.
/// </remarks>
public static class SamusMovementDefinitions
{
    /// <summary>Ball landing: signed whole-speed threshold for the first automatic bounce.</summary>
    public const int FirstBallBounceMinimumSpeed = 3;

    /// <summary>Standing movement's held-shot animation timer before the animation tick.</summary>
    public const int StandingShotAnimationTimer = 16;

    /// <summary>Falling movement's minimum signed whole speed selecting fast-fall art.</summary>
    public const int FastFallAnimationSpeed = 5;
    /// <summary>Falling movement: first fast-fall animation frame.</summary>
    public const int FastFallAnimationFrame = 5;
    /// <summary>Falling movement: fast-fall timer before the animation tick.</summary>
    public const int FastFallAnimationTimer = 8;


    /// <summary>Pose definitions, animations, and controller-transition streams.</summary>
    public static class Poses
    {

        /// <summary>
        /// The no-input fallback that retains the current pose after input lookup failure. The game
        /// spells it as <see cref="SamusPoseId.ForwardStillCrouchHeightPose"/>, so it is the same identity: a pose
        /// whose fallback names <see cref="SamusPoseId.ForwardStillCrouchHeightPose"/> retains its pose.
        /// </summary>
        public const SamusPoseId RetainCurrentPoseFallback = SamusPoseId.ForwardStillCrouchHeightPose;
    }

    /// <summary>Horizontal acceleration, Speed Booster, and animation cadence tables.</summary>
    public static class HorizontalMotion
    {
        /// <summary>Per-frame extra-run acceleration while charging the Speed Booster: 1/16 pixel.</summary>
        public static readonly UnsignedSixteenSixteen ExtraRunAcceleration = UnsignedSixteenSixteen.FromSixteenthsOfPixel(1);
    }
}
