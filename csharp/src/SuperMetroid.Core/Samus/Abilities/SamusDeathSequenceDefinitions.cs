using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Fatal-damage pose entry and state boundaries.</summary>
public static class SamusDeathSequenceDefinitions
{
    /// <summary>Sixteen calls display the ordinary death pose before flashing.</summary>
    public const int PreFlashFrameCount = 16;
    /// <summary>Sixty calls alternate palettes before the suit explodes.</summary>
    public const int FlashFrameCount = 60;
    /// <summary>Right-facing base index in Samus's death-explosion spritemap table.</summary>
    public const int RightExplosionSpritemap = 2076;
    /// <summary>Left-facing base index in Samus's death-explosion spritemap table.</summary>
    public const int LeftExplosionSpritemap = 2085;

    /// <summary>The value each key selects; InitialFrame looks it up.</summary>
    public static readonly IReadOnlyDictionary<SamusMovementType, byte> InitialFrames = new Dictionary<SamusMovementType, byte>
    {
        [SamusMovementType.MorphBallGround] = 1,
        [SamusMovementType.MorphBallFalling] = 1,
        [SamusMovementType.SpringBallGround] = 1,
        [SamusMovementType.SpringBallInAir] = 1,
        [SamusMovementType.SpringBallFalling] = 1,
        [SamusMovementType.UnusedGlitchBall] = 0,
        [SamusMovementType.UnusedGlitchBallAlternate] = 0,
        [SamusMovementType.Standing] = 5,
        [SamusMovementType.Running] = 5,
        [SamusMovementType.NormalJumping] = 5,
        [SamusMovementType.SpinJumping] = 5,
        [SamusMovementType.Crouching] = 5,
        [SamusMovementType.Falling] = 5,
        [SamusMovementType.Knockback] = 5,
        [SamusMovementType.Unused0B] = 5,
        [SamusMovementType.Unused0C] = 5,
        [SamusMovementType.Unused0D] = 5,
        [SamusMovementType.TurningOnGround] = 5,
        [SamusMovementType.PostureTransition] = 5,
        [SamusMovementType.Moonwalking] = 5,
        [SamusMovementType.WallJumping] = 5,
        [SamusMovementType.RanIntoWall] = 5,
        [SamusMovementType.Grappling] = 5,
        [SamusMovementType.TurningWhileJumping] = 5,
        [SamusMovementType.TurningWhileFalling] = 5,
        [SamusMovementType.DamageBoost] = 5,
        [SamusMovementType.DraygonHeld] = 5,
        [SamusMovementType.Special] = 5,
    };
}
