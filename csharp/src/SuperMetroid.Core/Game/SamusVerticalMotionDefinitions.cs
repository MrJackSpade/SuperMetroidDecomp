namespace SuperMetroid.Core.Game;

/// <summary>Pinned NTSC bank-$90 vertical mechanics; presentation overrides do not alter physics.</summary>
internal static class SamusVerticalMotionDefinitions
{
    /// <summary>$90:9EB5 YSpeedWhenBouncingInMorphBall; second rebound subtracts one from this whole word.</summary>
    internal const ushort BallBounceSpeed = 1;

    /// <summary>$90:9EB7 YSubSpeedWhenBouncingInMorphBall; both NTSC rebounds have zero fractional speed.</summary>
    internal const ushort BallBounceSubspeed = 0;


    /// <summary>$90:9EF5/9EFB InitialYSpeeds/InitialYSubSpeeds_BombJump: bomb launch is weakened equally in both liquids.</summary>
    internal static (ushort Whole, ushort Fraction) BombJump(SamusLiquidMedium medium) => SplitSpeed(medium switch
    {
        SamusLiquidMedium.Air => 0x02c0,
        SamusLiquidMedium.Water or SamusLiquidMedium.LavaOrAcid => 0x0010,
        _ => throw new InvalidOperationException($"Undefined SamusLiquidMedium {medium}."),
    });

    /// <summary>$90:9EE9/9EEF InitialYSpeeds/InitialYSubSpeeds_Knockback: hurt launch ignores jump equipment and dash speed.</summary>
    internal static (ushort Whole, ushort Fraction) Knockback(SamusLiquidMedium medium) => SplitSpeed(medium switch
    {
        SamusLiquidMedium.Air => 0x0500,
        SamusLiquidMedium.Water or SamusLiquidMedium.LavaOrAcid => 0x0200,
        _ => throw new InvalidOperationException($"Undefined SamusLiquidMedium {medium}."),
    });

    /// <summary>
    /// $90:9EB9..9EE8 InitialYSpeeds/InitialYSubSpeeds: choose the launch magnitude
    /// by medium, jump equipment and whether Samus kicks away from a wall. Each
    /// original whole/fraction pair is the split representation of one 8.8 speed.
    /// </summary>
    internal static (ushort Whole, ushort Fraction) Launch(SamusLiquidMedium medium, bool highJump, bool wallJump) =>
        SplitSpeed(medium switch
        {
            SamusLiquidMedium.Air => wallJump ? highJump ? 0x0580 : 0x04a0 : highJump ? 0x0600 : 0x04e0,
            SamusLiquidMedium.Water => wallJump ? highJump ? 0x0080 : 0x0040 : highJump ? 0x0280 : 0x01c0,
            SamusLiquidMedium.LavaOrAcid => highJump ? 0x0380 : wallJump ? 0x02a0 : 0x02c0,
            _ => throw new InvalidOperationException($"Undefined SamusLiquidMedium {medium}."),
        });

    /// <summary>$90:9EA1/9EA7 YSubAcceleration/YAcceleration: whole acceleration is zero in all three media.</summary>
    internal static (ushort Whole, ushort Fraction) Gravity(SamusLiquidMedium medium) => (0, medium switch
    {
        SamusLiquidMedium.Air => 0x1c00,
        SamusLiquidMedium.Water => 0x0800,
        SamusLiquidMedium.LavaOrAcid => 0x0900,
        _ => throw new InvalidOperationException($"Undefined SamusLiquidMedium {medium}."),
    });

    private static (ushort Whole, ushort Fraction) SplitSpeed(int speed) =>
        ((ushort)(speed >> 8), (ushort)((speed & 0xff) << 8));
}
