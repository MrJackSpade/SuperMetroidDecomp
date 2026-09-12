namespace SuperMetroid.Core.Game;

/// <summary>Pinned NTSC bank-$90 vertical mechanics; presentation overrides do not alter physics.</summary>
internal static class SamusVerticalMotionDefinitions
{
    /// <summary>$90:9EB5 YSpeedWhenBouncingInMorphBall; second rebound subtracts one from this whole word.</summary>
    internal const ushort BallBounceSpeed = 1;

    /// <summary>$90:9EB7 YSubSpeedWhenBouncingInMorphBall; both NTSC rebounds have zero fractional speed.</summary>
    internal const ushort BallBounceSubspeed = 0;

    /// <summary>$90:9EB9/9EBF InitialYSpeeds/InitialYSubSpeeds_Jumping, air/water/lava.</summary>
    private static ReadOnlySpan<ushort> Jump => [4, 0xe000, 1, 0xc000, 2, 0xc000];

    /// <summary>$90:9EC5/9ECB InitialYSpeeds/InitialYSubSpeeds_HiJumpJumping.</summary>
    private static ReadOnlySpan<ushort> HighJump => [6, 0, 2, 0x8000, 3, 0x8000];

    /// <summary>$90:9ED1/9ED7 InitialYSpeeds/InitialYSubSpeeds_WallJumping.</summary>
    private static ReadOnlySpan<ushort> WallJump => [4, 0xa000, 0, 0x4000, 2, 0xa000];

    /// <summary>$90:9EDD/9EE3 InitialYSpeeds/InitialYSubSpeeds_HiJumpWallJumping.</summary>
    private static ReadOnlySpan<ushort> HighWallJump => [5, 0x8000, 0, 0x8000, 3, 0x8000];

    /// <summary>$90:9EA1/9EA7 YSubAcceleration/YAcceleration, air/water/lava.</summary>
    private static ReadOnlySpan<ushort> GravityFractions => [0x1c00, 0x0800, 0x0900];

    /// <summary>$90:9EE9/9EEF InitialYSpeeds/InitialYSubSpeeds_Knockback, air/water/lava.</summary>
    private static ReadOnlySpan<ushort> KnockbackWords => [5, 0, 2, 0, 2, 0];

    /// <summary>$90:9EF5/9EFB InitialYSpeeds/InitialYSubSpeeds_BombJump, air/water/lava.</summary>
    private static ReadOnlySpan<ushort> BombJumpWords => [2, 0xc000, 0, 0x1000, 0, 0x1000];

    /// <summary>Bomb launch does not apply Hi-Jump or extra-run bonuses and does not refresh gravity.</summary>
    internal static (ushort Whole, ushort Fraction) BombJump(ushort medium) =>
        (BombJumpWords[medium * 2], BombJumpWords[medium * 2 + 1]);

    /// <summary>Hurt launch has its own magnitude, independent of jump equipment and dash speed.</summary>
    internal static (ushort Whole, ushort Fraction) Knockback(ushort medium) =>
        (KnockbackWords[medium * 2], KnockbackWords[medium * 2 + 1]);

    /// <summary>Selects the independently stored whole/fraction launch words after native medium selection.</summary>
    internal static (ushort Whole, ushort Fraction) Launch(ushort medium, bool highJump, bool wallJump)
    {
        ReadOnlySpan<ushort> words = wallJump
            ? highJump ? HighWallJump : WallJump
            : highJump ? HighJump : Jump;
        return (words[medium * 2], words[medium * 2 + 1]);
    }

    /// <summary>Native whole acceleration is zero in all three media; preserve the fractional word exactly.</summary>
    internal static (ushort Whole, ushort Fraction) Gravity(ushort medium) => (0, GravityFractions[medium]);
}
