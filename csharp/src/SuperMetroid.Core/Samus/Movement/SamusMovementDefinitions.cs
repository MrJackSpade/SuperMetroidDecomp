using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The definitions Samus's movement and pose selection read: speeds, timers, frames and sentinels (#1411).</summary>
/// <remarks>
/// Addresses are grouped by their native subsystem rather than by the C# class that happens
/// to read them. Full 24-bit addresses use <see cref="int"/>; values stored natively as
/// same-bank pointers use <see cref="ushort"/> so callers must explicitly supply the bank.
/// Runtime velocity, timers, and selected pointers do not belong in this catalog.
/// </remarks>
public static class SamusMovementDefinitions
{
    /// <summary>$91:F1FC/$F25E ball-landing handlers: signed whole-speed threshold for the first automatic bounce.</summary>
    public const short FirstBallBounceMinimumSpeed = 3;

    /// <summary>$90:A3C1, SamusMovement_Standing: held-shot animation timer before the animation tick.</summary>
    public const ushort StandingShotAnimationTimer = 16;

    /// <summary>$90:A58D, Samus_Movement_06_Falling: minimum signed whole speed selecting fast-fall art.</summary>
    public const short FastFallAnimationSpeed = 5;
    /// <summary>$90:A58D falling handler: first fast-fall animation frame.</summary>
    public const ushort FastFallAnimationFrame = 5;
    /// <summary>$90:A58D falling handler: fast-fall timer before the animation tick.</summary>
    public const ushort FastFallAnimationTimer = 8;


    /// <summary>Pose definitions, animation lists, and controller-transition streams.</summary>
    public static class Poses
    {

        /// <summary>
        /// The no-input fallback that retains the current pose after input lookup failure ($91:82F9). The cartridge
        /// spells it as pose $FF, so it is the same identity as <see cref="SamusPoseId.AdjacentCodePoseFF"/>: a record
        /// whose fallback names pose $FF retains its pose.
        /// </summary>
        public const SamusPoseId RetainCurrentPoseFallback = SamusPoseId.AdjacentCodePoseFF;

        /// <summary>
        /// $FF in pose-definition byte three: the pose has no shot direction. Hurt and
        /// damage-boost art store it, and $91:E95D lands them through the facing pair.
        /// </summary>
        public const byte NoShotDirection = 0xff;
    }

    /// <summary>Horizontal acceleration, Speed Booster, and animation cadence tables.</summary>
    public static class HorizontalMotion
    {
        /// <summary>Per-frame 16.16 extra-run acceleration while charging the Speed Booster: 1/16 pixel.</summary>
        public const uint ExtraRunAcceleration = 0x00001000u;
    }

    /// <summary>Liquid damage, splashes, footsteps, and atmospheric-effect animation data.</summary>
    public static class Environment
    {

        /// <summary>Four words per authored direct small-OBJ effect list at $90:8C0F/$90:8C17.</summary>
        public const int DirectAtmosphericFrameCount = 4;
    }
}
