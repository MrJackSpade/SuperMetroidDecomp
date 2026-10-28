namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Pinned NTSC bank-$90 vertical mechanics; presentation overrides do not alter physics.</summary>
internal static class SamusVerticalMotionDefinitions
{
    /// <summary>
    /// $90:9EB5/9EB7 YSpeed/YSubSpeedWhenBouncingInMorphBall; the second rebound subtracts one from the whole word, and
    /// both NTSC rebounds have zero fractional speed.
    /// </summary>
    internal static UnsignedSixteenSixteen BallBounceSpeed { get; } = new(1, 0);

    /// <summary>$90:9EF5/9EFB InitialYSpeeds/InitialYSubSpeeds_BombJump: bomb launch is weakened equally in both liquids,
    /// as 8.8 speeds by medium.</summary>
    internal static readonly IReadOnlyDictionary<SamusLiquidMedium, int> BombJumpSpeeds = new Dictionary<SamusLiquidMedium, int>
    {
        [SamusLiquidMedium.Air] = 0x02c0,
        [SamusLiquidMedium.Water] = 0x0010,
        [SamusLiquidMedium.LavaOrAcid] = 0x0010,
    };

    /// <summary>$90:9EE9/9EEF InitialYSpeeds/InitialYSubSpeeds_Knockback: hurt launch ignores jump equipment and dash speed,
    /// as 8.8 speeds by medium.</summary>
    internal static readonly IReadOnlyDictionary<SamusLiquidMedium, int> KnockbackSpeeds = new Dictionary<SamusLiquidMedium, int>
    {
        [SamusLiquidMedium.Air] = 0x0500,
        [SamusLiquidMedium.Water] = 0x0200,
        [SamusLiquidMedium.LavaOrAcid] = 0x0200,
    };

    /// <summary>
    /// $90:9EB9..9EE8 InitialYSpeeds/InitialYSubSpeeds: the launch magnitude by medium, jump equipment and whether Samus
    /// kicks away from a wall, as 8.8 speeds. In lava or acid High Jump outweighs the wall kick.
    /// </summary>
    internal static readonly IReadOnlyDictionary<(SamusLiquidMedium Medium, bool HighJump, bool WallJump), int> LaunchSpeeds =
        new Dictionary<(SamusLiquidMedium Medium, bool HighJump, bool WallJump), int>
    {
        [(SamusLiquidMedium.Air, true, true)] = 0x0580,
        [(SamusLiquidMedium.Air, false, true)] = 0x04a0,
        [(SamusLiquidMedium.Air, true, false)] = 0x0600,
        [(SamusLiquidMedium.Air, false, false)] = 0x04e0,
        [(SamusLiquidMedium.Water, true, true)] = 0x0080,
        [(SamusLiquidMedium.Water, false, true)] = 0x0040,
        [(SamusLiquidMedium.Water, true, false)] = 0x0280,
        [(SamusLiquidMedium.Water, false, false)] = 0x01c0,
        [(SamusLiquidMedium.LavaOrAcid, true, true)] = 0x0380,
        [(SamusLiquidMedium.LavaOrAcid, false, true)] = 0x02a0,
        [(SamusLiquidMedium.LavaOrAcid, true, false)] = 0x0380,
        [(SamusLiquidMedium.LavaOrAcid, false, false)] = 0x02c0,
    };

    /// <summary>$90:9EA1/9EA7 YSubAcceleration/YAcceleration: the fraction of each medium's gravity; the whole is zero in
    /// all three media.</summary>
    internal static readonly IReadOnlyDictionary<SamusLiquidMedium, int> GravityFractions = new Dictionary<SamusLiquidMedium, int>
    {
        [SamusLiquidMedium.Air] = 0x1c00,
        [SamusLiquidMedium.Water] = 0x0800,
        [SamusLiquidMedium.LavaOrAcid] = 0x0900,
    };
}
