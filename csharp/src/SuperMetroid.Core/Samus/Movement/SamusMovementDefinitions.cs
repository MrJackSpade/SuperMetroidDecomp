using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The definitions Samus's movement and pose selection read: speeds, timers, frames and sentinels (#1411).</summary>
/// <remarks>
/// Addresses are grouped by their subsystem rather than by the C# class that happens
/// to read them. Full 24-bit locations use <see cref="int"/>;
/// values stored as short pointers use <see cref="ushort"/> so callers must explicitly supply the rest.
/// Runtime velocity, timers, and selected pointers do not belong in this catalog.
/// </remarks>
public static class SamusMovementDefinitions
{
    /// <summary>Ball-landing handlers: signed whole-speed threshold for the first automatic bounce.</summary>
    public const int FirstBallBounceMinimumSpeed = 3;

    /// <summary>Standing movement's held-shot animation timer before the animation tick.</summary>
    public const int StandingShotAnimationTimer = 16;

    /// <summary>Falling movement's minimum signed whole speed selecting fast-fall art.</summary>
    public const int FastFallAnimationSpeed = 5;
    /// <summary>Falling handler: first fast-fall animation frame.</summary>
    public const int FastFallAnimationFrame = 5;
    /// <summary>Falling handler: fast-fall timer before the animation tick.</summary>
    public const int FastFallAnimationTimer = 8;


    /// <summary>Pose definitions, animation lists, and controller-transition streams.</summary>
    public static class Poses
    {

        /// <summary>
        /// The no-input fallback that retains the current pose after input lookup failure. The game
        /// spells it as <see cref="SamusPoseId.AdjacentCodePoseFF"/>, so it is the same identity as <see cref="SamusPoseId.AdjacentCodePoseFF"/>: a record
        /// whose fallback names <see cref="SamusPoseId.AdjacentCodePoseFF"/> retains its pose.
        /// </summary>
        public const SamusPoseId RetainCurrentPoseFallback = SamusPoseId.AdjacentCodePoseFF;

        /// <summary>
        /// The pose definition's shot direction is absent: the pose has no shot direction. Hurt and
        /// damage-boost art store it, and the landing transition lands them through the facing pair.
        /// </summary>
        public const int NoShotDirection = 255;
    }

    /// <summary>Horizontal acceleration, Speed Booster, and animation cadence tables.</summary>
    public static class HorizontalMotion
    {
        /// <summary>Per-frame 16.16 extra-run acceleration while charging the Speed Booster: 1/16 pixel.</summary>
        public const uint ExtraRunAcceleration = 4096u;
    }
}
