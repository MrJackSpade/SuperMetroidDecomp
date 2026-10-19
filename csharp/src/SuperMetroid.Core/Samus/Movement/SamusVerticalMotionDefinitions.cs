namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Pinned NTSC bank-$90 vertical mechanics; presentation overrides do not alter physics.</summary>
internal static class SamusVerticalMotionDefinitions
{
    /// <summary>
    /// $90:9EB5/9EB7 YSpeed/YSubSpeedWhenBouncingInMorphBall; the second rebound subtracts one from the whole word, and
    /// both NTSC rebounds have zero fractional speed.
    /// </summary>
    internal static SnesUnsignedSixteenSixteen BallBounceSpeed { get; } = new(1, 0);

    /// <summary>$90:9EF5/9EFB InitialYSpeeds/InitialYSubSpeeds_BombJump: bomb launch is weakened equally in both liquids.</summary>
    internal static SnesUnsignedSixteenSixteen BombJump(SamusLiquidMedium medium) => SplitSpeed(medium switch
    {
        SamusLiquidMedium.Air => 0x02c0,
        SamusLiquidMedium.Water or SamusLiquidMedium.LavaOrAcid => 0x0010,
        _ => throw new InvalidOperationException($"Undefined SamusLiquidMedium {medium}."),
    });

    /// <summary>$90:9EE9/9EEF InitialYSpeeds/InitialYSubSpeeds_Knockback: hurt launch ignores jump equipment and dash speed.</summary>
    internal static SnesUnsignedSixteenSixteen Knockback(SamusLiquidMedium medium) => SplitSpeed(medium switch
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
    internal static SnesUnsignedSixteenSixteen Launch(SamusLiquidMedium medium, bool highJump, bool wallJump) =>
        SplitSpeed(medium switch
        {
            SamusLiquidMedium.Air => wallJump ? highJump ? 0x0580 : 0x04a0 : highJump ? 0x0600 : 0x04e0,
            SamusLiquidMedium.Water => wallJump ? highJump ? 0x0080 : 0x0040 : highJump ? 0x0280 : 0x01c0,
            SamusLiquidMedium.LavaOrAcid => highJump ? 0x0380 : wallJump ? 0x02a0 : 0x02c0,
            _ => throw new InvalidOperationException($"Undefined SamusLiquidMedium {medium}."),
        });

    /// <summary>$90:9EA1/9EA7 YSubAcceleration/YAcceleration: whole acceleration is zero in all three media.</summary>
    internal static SnesUnsignedSixteenSixteen Gravity(SamusLiquidMedium medium) => new SnesUnsignedSixteenSixteen(0, medium switch
    {
        SamusLiquidMedium.Air => 0x1c00,
        SamusLiquidMedium.Water => 0x0800,
        SamusLiquidMedium.LavaOrAcid => 0x0900,
        _ => throw new InvalidOperationException($"Undefined SamusLiquidMedium {medium}."),
    });

    /// <summary>Splits a native 8.8 launch magnitude into the separate whole and 16.16 fractional words Samus stores.</summary>
    private static SnesUnsignedSixteenSixteen SplitSpeed(int speed) =>
        new(SnesEightEight.WholeOf(speed), SnesSixteenSixteen.FractionOf(SnesSixteenSixteen.FromEightEight(speed)));
}
