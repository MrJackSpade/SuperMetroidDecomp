using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The tools' and checks' native view of <see cref="SamusMovementType"/> (#1481): the movement byte of a pose definition, which
/// indexes the cartridge's per-movement tables. Core's values are arbitrary.
/// </summary>
internal static class SamusMovementTypeNativeWords
{
    /// <summary>The movement byte naming each <see cref="SamusMovementType"/>.</summary>
    internal static readonly NativeWordIdentities<SamusMovementType> Types = new(new Dictionary<SamusMovementType, ushort>
    {
        [SamusMovementType.Standing] = 0x00,
        [SamusMovementType.Running] = 0x01,
        [SamusMovementType.NormalJumping] = 0x02,
        [SamusMovementType.SpinJumping] = 0x03,
        [SamusMovementType.MorphBallGround] = 0x04,
        [SamusMovementType.Crouching] = 0x05,
        [SamusMovementType.Falling] = 0x06,
        [SamusMovementType.GlitchBall] = 0x07,
        [SamusMovementType.MorphBallFalling] = 0x08,
        [SamusMovementType.GlitchBallKnockback] = 0x09,
        [SamusMovementType.Knockback] = 0x0a,
        [SamusMovementType.CompactAirborne] = 0x0d,
        [SamusMovementType.TurningOnGround] = 0x0e,
        [SamusMovementType.PostureTransition] = 0x0f,
        [SamusMovementType.Moonwalking] = 0x10,
        [SamusMovementType.SpringBallGround] = 0x11,
        [SamusMovementType.SpringBallInAir] = 0x12,
        [SamusMovementType.SpringBallFalling] = 0x13,
        [SamusMovementType.WallJumping] = 0x14,
        [SamusMovementType.RanIntoWall] = 0x15,
        [SamusMovementType.Grappling] = 0x16,
        [SamusMovementType.TurningWhileJumping] = 0x17,
        [SamusMovementType.TurningWhileFalling] = 0x18,
        [SamusMovementType.DamageBoost] = 0x19,
        [SamusMovementType.DraygonHeld] = 0x1a,
        [SamusMovementType.Special] = 0x1b,
    });

    /// <summary>The movement byte of <paramref name="movement"/>.</summary>
    internal static byte Byte(SamusMovementType movement) => checked((byte)Types.Word(movement));
}
