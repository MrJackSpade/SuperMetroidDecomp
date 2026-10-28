using System.Diagnostics.CodeAnalysis;
using System.Collections.Frozen;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Pinned bank-$90 horizontal mechanics, separate from animation and palette definitions.</summary>
internal static class SamusHorizontalMotionDefinitions
{
    /// <summary>Deceleration fraction of every air record.</summary>
    [SuppressMessage("Design", "SME6447", Justification =
        "The fraction of a 16.16 deceleration of half a pixel per update, not a cartridge address.")]
    internal const int AirDecelerationFraction = 0x8000;
    /// <summary>Deceleration fraction of every water record.</summary>
    internal const int WaterDecelerationFraction = 0x0800;
    /// <summary>Deceleration fraction of every lava-or-acid record.</summary>
    internal const int LavaDecelerationFraction = 0x4000;
    /// <summary>Maximum running speed in air and water.</summary>
    internal const uint RunningMaximum = 0x2c000u;
    /// <summary>Maximum running speed in lava or acid.</summary>
    internal const uint LavaRunningMaximum = 0x1c000u;
    /// <summary>Maximum speed of normal jumps, posture transitions, airborne spring ball and grappling.</summary>
    internal const uint JumpingMaximum = 0x14000u;
    /// <summary>Maximum speed of spin and wall jumps.</summary>
    internal const uint SpinJumpingMaximum = 0x16000u;
    /// <summary>Maximum speed of a grounded morph or spring ball in air.</summary>
    internal const uint AirMorphBallGroundMaximum = 0x34000u;
    /// <summary>Maximum speed of a grounded morph or spring ball in a liquid.</summary>
    internal const uint LiquidMorphBallGroundMaximum = 0x2c000u;
    /// <summary>Maximum speed of falling, and of a falling ball in air.</summary>
    internal const uint FallingMaximum = 0x10000u;
    /// <summary>Maximum speed of a falling morph or spring ball in water.</summary>
    internal const uint WaterBallFallingMaximum = 0x18000u;
    /// <summary>Maximum speed of a falling morph or spring ball in lava or acid.</summary>
    internal const uint LavaBallFallingMaximum = 0x16000u;
    /// <summary>Maximum speed of the alternate unused glitch ball and unused movement $0D.</summary>
    internal const uint GlitchBallMaximum = 0x20000u;
    /// <summary>Maximum speed of knockback, Draygon's hold, special movement and an airborne damage boost.</summary>
    internal const uint KnockbackMaximum = 0x50000u;
    /// <summary>Maximum speed of moonwalking and a grounded damage boost.</summary>
    [SuppressMessage("Design", "SME6447", Justification =
        "A 16.16 maximum speed of half a pixel per update, not a cartridge address.")]
    internal const uint MoonwalkingMaximum = 0x08000u;
    /// <summary>Running acceleration in air.</summary>
    internal const uint AirRunningAcceleration = 0x3000u;
    /// <summary>Acceleration of running in a liquid and of a morph or spring ball in a liquid.</summary>
    internal const uint LiquidGroundAcceleration = 0x0400u;
    /// <summary>Acceleration of both unused glitch balls.</summary>
    internal const uint GlitchBallAcceleration = 0x20000u;
    /// <summary>Knockback acceleration.</summary>
    internal const uint KnockbackAcceleration = 0x18000u;
    /// <summary>The authored acceleration of every other movement.</summary>
    [SuppressMessage("Design", "SME6447", Justification =
        "A 16.16 acceleration of three quarters of a pixel per update, not a cartridge address.")]
    internal const uint DefaultAcceleration = 0xc000u;

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
        Acceleration: SignedSixteenSixteen.FromParts(0, 0x3000),
        MaximumSpeed: SignedSixteenSixteen.FromParts(3, 0),
        Deceleration: SignedSixteenSixteen.FromParts(0, 0x0800));

    /// <summary>$90:9F31/$9F3D/$9F49 XAccelSpeeds_DisconnectGrappleInAir/InWater/InLavaAcid: three identical authored records.</summary>
    internal static readonly SpeedTableEntry GrappleRelease = new(
        Acceleration: SignedSixteenSixteen.FromParts(0, 0x3000),
        MaximumSpeed: SignedSixteenSixteen.FromParts(15, 0),
        Deceleration: SignedSixteenSixteen.FromParts(0, 0x1000));
}
