using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Fatal-damage pose entry and state boundaries.</summary>
public static class SamusDeathSequenceDefinitions
{
    /// <summary>Sixteen calls display the ordinary death pose before flashing.</summary>
    public const ushort PreFlashFrameCount = 16;
    /// <summary>Sixty calls alternate palettes before the suit explodes.</summary>
    public const ushort FlashFrameCount = 60;
    /// <summary>Right-facing base index in Samus's death-explosion spritemap table.</summary>
    public const ushort RightExplosionSpritemap = 0x081c;
    /// <summary>Left-facing base index in Samus's death-explosion spritemap table.</summary>
    public const ushort LeftExplosionSpritemap = 0x0825;

    /// <summary>
    /// $9B:B420: the initial visible death-pose animation frame the movement type in effect before fatal-damage pose
    /// setup selects, without replacing the separately initialized animation timer: ball movement starts at one, the
    /// two unused glitch-ball types at zero, every other movement type at five.
    /// </summary>
    /// <param name="movement">The movement type before the death pose.</param>
    /// <returns>The initial death-pose animation frame.</returns>
    public static byte InitialFrame(SamusMovementType movement) => movement switch
    {
        SamusMovementType.MorphBallGround or SamusMovementType.MorphBallFalling or
        SamusMovementType.SpringBallGround or SamusMovementType.SpringBallInAir or
        SamusMovementType.SpringBallFalling => 1,
        SamusMovementType.UnusedGlitchBall or SamusMovementType.UnusedGlitchBallAlternate => 0,
        SamusMovementType.Standing or SamusMovementType.Running or SamusMovementType.NormalJumping or
            SamusMovementType.SpinJumping or SamusMovementType.Crouching or SamusMovementType.Falling or
            SamusMovementType.Knockback or SamusMovementType.Unused0B or SamusMovementType.Unused0C or
            SamusMovementType.Unused0D or SamusMovementType.TurningOnGround or
            SamusMovementType.PostureTransition or SamusMovementType.Moonwalking or
            SamusMovementType.WallJumping or SamusMovementType.RanIntoWall or SamusMovementType.Grappling or
            SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling or
            SamusMovementType.DamageBoost or SamusMovementType.DraygonHeld or SamusMovementType.Special => 5,
        _ => throw new InvalidDataException($"Death-pose frame table has no movement type {movement}."),
    };
}
