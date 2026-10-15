namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Vertical launch velocity and gravity table addresses in bank $90 that only verification reads.</summary>
internal static class SamusMovementRomDataVerticalMotionConstants
{
    /// <summary>$90:9EB5 YSpeedWhenBouncingInMorphBall; reference address for the shared ball rebound.</summary>
    public const int BallBounceSpeed = 0x909eb5;
    /// <summary>$90:9EB7 YSubSpeedWhenBouncingInMorphBall; reference address for the shared ball rebound fraction.</summary>
    public const int BallBounceSubspeed = 0x909eb7;
    /// <summary>Bomb-jump launch speed selected by the current liquid medium.</summary>
    public const int BombJumpSpeeds = 0x909ef5;
    /// <summary>Bomb-jump launch subspeed selected by the current liquid medium.</summary>
    public const int BombJumpSubspeeds = 0x909efb;
    /// <summary>Dry-air, water, and lava/acid whole gravity words.</summary>
    public const int GravityAccelerations = 0x909ea7;
    /// <summary>Dry-air, water, and lava/acid fractional gravity words.</summary>
    public const int GravitySubaccelerations = 0x909ea1;
    /// <summary>Hi-Jump speed selected by the current liquid medium.</summary>
    public const int HiJumpSpeeds = 0x909ec5;
    /// <summary>Hi-Jump subspeed selected by the current liquid medium.</summary>
    public const int HiJumpSubspeeds = 0x909ecb;
    /// <summary>Hi-Jump wall-jump speed selected by the current liquid medium.</summary>
    public const int HiWallJumpSpeeds = 0x909edd;
    /// <summary>Hi-Jump wall-jump subspeed selected by the current liquid medium.</summary>
    public const int HiWallJumpSubspeeds = 0x909ee3;
    /// <summary>Knockback launch speed selected by the current liquid medium.</summary>
    public const int KnockbackSpeeds = 0x909ee9;
    /// <summary>Knockback launch subspeed selected by the current liquid medium.</summary>
    public const int KnockbackSubspeeds = 0x909eef;
    /// <summary>Normal-jump speed selected by the current liquid medium.</summary>
    public const int NormalJumpSpeeds = 0x909eb9;
    /// <summary>Normal-jump subspeed selected by the current liquid medium.</summary>
    public const int NormalJumpSubspeeds = 0x909ebf;
    /// <summary>Wall-jump speed selected by the current liquid medium.</summary>
    public const int WallJumpSpeeds = 0x909ed1;
    /// <summary>Wall-jump subspeed selected by the current liquid medium.</summary>
    public const int WallJumpSubspeeds = 0x909ed7;
}
