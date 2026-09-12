namespace SuperMetroid.Core.Game;

/// <summary>Pinned NTSC bank-$90 vertical mechanics; presentation overrides do not alter physics.</summary>
internal static class SamusVerticalMotionDefinitions
{
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
