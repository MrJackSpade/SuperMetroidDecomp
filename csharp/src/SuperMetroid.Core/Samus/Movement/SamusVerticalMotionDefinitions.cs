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
    /// $90:9EB9..9EE8 InitialYSpeeds/InitialYSubSpeeds: the launch magnitude with High Jump equipped by medium and
    /// whether Samus kicks away from a wall, as 8.8 speeds. In lava or acid High Jump outweighs the wall kick.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusLaunchSpeedKey, int> HighJumpLaunchSpeeds =
        new Dictionary<SamusLaunchSpeedKey, int>
    {
        [new(SamusLiquidMedium.Air, SamusLaunchKind.WallJump)] = 0x0580,
        [new(SamusLiquidMedium.Air, SamusLaunchKind.Jump)] = 0x0600,
        [new(SamusLiquidMedium.Water, SamusLaunchKind.WallJump)] = 0x0080,
        [new(SamusLiquidMedium.Water, SamusLaunchKind.Jump)] = 0x0280,
        [new(SamusLiquidMedium.LavaOrAcid, SamusLaunchKind.WallJump)] = 0x0380,
        [new(SamusLiquidMedium.LavaOrAcid, SamusLaunchKind.Jump)] = 0x0380,
    };

    /// <summary>
    /// $90:9EB9..9EE8 InitialYSpeeds/InitialYSubSpeeds: the launch magnitude without High Jump by medium and whether
    /// Samus kicks away from a wall, as 8.8 speeds.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusLaunchSpeedKey, int> UnassistedLaunchSpeeds =
        new Dictionary<SamusLaunchSpeedKey, int>
    {
        [new(SamusLiquidMedium.Air, SamusLaunchKind.WallJump)] = 0x04a0,
        [new(SamusLiquidMedium.Air, SamusLaunchKind.Jump)] = 0x04e0,
        [new(SamusLiquidMedium.Water, SamusLaunchKind.WallJump)] = 0x0040,
        [new(SamusLiquidMedium.Water, SamusLaunchKind.Jump)] = 0x01c0,
        [new(SamusLiquidMedium.LavaOrAcid, SamusLaunchKind.WallJump)] = 0x02a0,
        [new(SamusLiquidMedium.LavaOrAcid, SamusLaunchKind.Jump)] = 0x02c0,
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
