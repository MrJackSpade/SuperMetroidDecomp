using System.Collections.Frozen;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Pinned bank-$90 horizontal mechanics, separate from animation and palette definitions.</summary>
internal static class SamusHorizontalMotionDefinitions
{
    /// <summary>Deceleration of every air record, half a pixel per update per update.</summary>
    internal static SignedSixteenSixteen AirDeceleration { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(8);
    /// <summary>Deceleration of every water record.</summary>
    internal static SignedSixteenSixteen WaterDeceleration { get; } = SignedSixteenSixteen.FromParts(0, 2048);
    /// <summary>Deceleration of every lava-or-acid record.</summary>
    internal static SignedSixteenSixteen LavaDeceleration { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(4);
    /// <summary>Maximum running speed in air and water.</summary>
    internal static SignedSixteenSixteen RunningMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(44);
    /// <summary>Maximum running speed in lava or acid.</summary>
    internal static SignedSixteenSixteen LavaRunningMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(28);
    /// <summary>Maximum speed of normal jumps, posture transitions, airborne spring ball and grappling.</summary>
    internal static SignedSixteenSixteen JumpingMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(20);
    /// <summary>Maximum speed of spin and wall jumps.</summary>
    internal static SignedSixteenSixteen SpinJumpingMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(22);
    /// <summary>Maximum speed of a grounded morph or spring ball in air.</summary>
    internal static SignedSixteenSixteen AirMorphBallGroundMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(52);
    /// <summary>Maximum speed of a grounded morph or spring ball in a liquid.</summary>
    internal static SignedSixteenSixteen LiquidMorphBallGroundMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(44);
    /// <summary>Maximum speed of falling, and of a falling ball in air.</summary>
    internal static SignedSixteenSixteen FallingMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(16);
    /// <summary>Maximum speed of a falling morph or spring ball in water.</summary>
    internal static SignedSixteenSixteen WaterBallFallingMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(24);
    /// <summary>Maximum speed of a falling morph or spring ball in lava or acid.</summary>
    internal static SignedSixteenSixteen LavaBallFallingMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(22);
    /// <summary>Maximum speed of the alternate unused glitch ball and unused movement $0D.</summary>
    internal static SignedSixteenSixteen GlitchBallMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(32);
    /// <summary>Maximum speed of knockback, Draygon's hold, special movement and an airborne damage boost.</summary>
    internal static SignedSixteenSixteen KnockbackMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(80);
    /// <summary>Maximum speed of moonwalking and a grounded damage boost, half a pixel per update.</summary>
    internal static SignedSixteenSixteen MoonwalkingMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(8);
    /// <summary>Running acceleration in air.</summary>
    internal static SignedSixteenSixteen AirRunningAcceleration { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(3);
    /// <summary>Acceleration of running in a liquid and of a morph or spring ball in a liquid.</summary>
    internal static SignedSixteenSixteen LiquidGroundAcceleration { get; } = SignedSixteenSixteen.FromParts(0, 1024);
    /// <summary>Acceleration of both unused glitch balls.</summary>
    internal static SignedSixteenSixteen GlitchBallAcceleration { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(32);
    /// <summary>Knockback acceleration.</summary>
    internal static SignedSixteenSixteen KnockbackAcceleration { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(24);
    /// <summary>The authored acceleration of every other movement, three quarters of a pixel per update per update.</summary>
    internal static SignedSixteenSixteen DefaultAcceleration { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(12);

    /// <summary>$90:9F55..A08C normal-air speed records end after damage boost.</summary>
    internal const int AirRecordCount = 26;
    /// <summary>$90:A08D and A1DD water/lava records additionally include held and special movement.</summary>
    internal const int LiquidRecordCount = 28;
    /// <summary>Records across the three adjacent tables: air, then water, then lava/acid.</summary>
    internal const int RowCount = AirRecordCount + 2 * LiquidRecordCount;

    /// <summary>
    /// The movement type each table's records describe, in record order. The air table stops after damage boost, so
    /// selecting the air table's held or special record reads the water table's first records.
    /// </summary>
    internal static readonly SamusMovementType[] RecordOrder =
    [
        SamusMovementType.Standing, SamusMovementType.Running, SamusMovementType.NormalJumping,
        SamusMovementType.SpinJumping, SamusMovementType.MorphBallGround, SamusMovementType.Crouching,
        SamusMovementType.Falling, SamusMovementType.UnusedGlitchBall, SamusMovementType.MorphBallFalling,
        SamusMovementType.UnusedGlitchBallAlternate, SamusMovementType.Knockback, SamusMovementType.Unused0B,
        SamusMovementType.Unused0C, SamusMovementType.Unused0D, SamusMovementType.TurningOnGround,
        SamusMovementType.PostureTransition, SamusMovementType.Moonwalking, SamusMovementType.SpringBallGround,
        SamusMovementType.SpringBallInAir, SamusMovementType.SpringBallFalling, SamusMovementType.WallJumping,
        SamusMovementType.RanIntoWall, SamusMovementType.Grappling, SamusMovementType.TurningWhileJumping,
        SamusMovementType.TurningWhileFalling, SamusMovementType.DamageBoost, SamusMovementType.DraygonHeld,
        SamusMovementType.Special,
    ];

    /// <summary>Each movement type's position in <see cref="RecordOrder"/>.</summary>
    internal static readonly FrozenDictionary<SamusMovementType, int> RecordPosition =
        Enumerable.Range(0, RecordOrder.Length).ToFrozenDictionary(position => RecordOrder[position]);

    /// <summary>$90:9F25 XAccelSpeeds_DiagonalBombJump: acceleration, maximum and deceleration whole/fraction pairs.</summary>
    internal static readonly SpeedTableEntry DiagonalBombJump = new(
        Acceleration: SignedSixteenSixteen.FromParts(0, 12288),
        MaximumSpeed: SignedSixteenSixteen.FromParts(3, 0),
        Deceleration: SignedSixteenSixteen.FromParts(0, 2048));

    /// <summary>$90:9F31/$9F3D/$9F49 XAccelSpeeds_DisconnectGrappleInAir/InWater/InLavaAcid: three identical authored records.</summary>
    internal static readonly SpeedTableEntry GrappleRelease = new(
        Acceleration: SignedSixteenSixteen.FromParts(0, 12288),
        MaximumSpeed: SignedSixteenSixteen.FromParts(15, 0),
        Deceleration: SignedSixteenSixteen.FromParts(0, 4096));
}
