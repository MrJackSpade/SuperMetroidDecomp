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
    /// as unsigned 16.16 speeds by medium.</summary>
    internal static readonly IReadOnlyDictionary<SamusLiquidMedium, UnsignedSixteenSixteen> BombJumpSpeeds =
        new Dictionary<SamusLiquidMedium, UnsignedSixteenSixteen>
    {
        [SamusLiquidMedium.Air] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(44),
        [SamusLiquidMedium.Water] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(1),
        [SamusLiquidMedium.LavaOrAcid] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(1),
    };

    /// <summary>$90:9EE9/9EEF InitialYSpeeds/InitialYSubSpeeds_Knockback: hurt launch ignores jump equipment and dash speed,
    /// as unsigned 16.16 speeds by medium.</summary>
    internal static readonly IReadOnlyDictionary<SamusLiquidMedium, UnsignedSixteenSixteen> KnockbackSpeeds =
        new Dictionary<SamusLiquidMedium, UnsignedSixteenSixteen>
    {
        [SamusLiquidMedium.Air] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(80),
        [SamusLiquidMedium.Water] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(32),
        [SamusLiquidMedium.LavaOrAcid] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(32),
    };

    /// <summary>
    /// $90:9EB9..9EE8 InitialYSpeeds/InitialYSubSpeeds: the launch magnitude with High Jump equipped by medium and
    /// whether Samus kicks away from a wall, as unsigned 16.16 speeds. In lava or acid High Jump outweighs the wall kick.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusLaunchSpeedKey, UnsignedSixteenSixteen> HighJumpLaunchSpeeds =
        new Dictionary<SamusLaunchSpeedKey, UnsignedSixteenSixteen>
    {
        [new(SamusLiquidMedium.Air, SamusLaunchKind.WallJump)] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(88),
        [new(SamusLiquidMedium.Air, SamusLaunchKind.Jump)] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(96),
        [new(SamusLiquidMedium.Water, SamusLaunchKind.WallJump)] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(8),
        [new(SamusLiquidMedium.Water, SamusLaunchKind.Jump)] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(40),
        [new(SamusLiquidMedium.LavaOrAcid, SamusLaunchKind.WallJump)] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(56),
        [new(SamusLiquidMedium.LavaOrAcid, SamusLaunchKind.Jump)] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(56),
    };

    /// <summary>
    /// $90:9EB9..9EE8 InitialYSpeeds/InitialYSubSpeeds: the launch magnitude without High Jump by medium and whether
    /// Samus kicks away from a wall, as unsigned 16.16 speeds.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusLaunchSpeedKey, UnsignedSixteenSixteen> UnassistedLaunchSpeeds =
        new Dictionary<SamusLaunchSpeedKey, UnsignedSixteenSixteen>
    {
        [new(SamusLiquidMedium.Air, SamusLaunchKind.WallJump)] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(74),
        [new(SamusLiquidMedium.Air, SamusLaunchKind.Jump)] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(78),
        [new(SamusLiquidMedium.Water, SamusLaunchKind.WallJump)] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(4),
        [new(SamusLiquidMedium.Water, SamusLaunchKind.Jump)] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(28),
        [new(SamusLiquidMedium.LavaOrAcid, SamusLaunchKind.WallJump)] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(42),
        [new(SamusLiquidMedium.LavaOrAcid, SamusLaunchKind.Jump)] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(44),
    };

    /// <summary>$90:9EA1/9EA7 YSubAcceleration/YAcceleration: each medium's gravity, a fraction of a pixel per frame in
    /// all three media.</summary>
    internal static readonly IReadOnlyDictionary<SamusLiquidMedium, UnsignedSixteenSixteen> Gravities =
        new Dictionary<SamusLiquidMedium, UnsignedSixteenSixteen>
    {
        [SamusLiquidMedium.Air] = new(0, 7168),
        [SamusLiquidMedium.Water] = new(0, 2048),
        [SamusLiquidMedium.LavaOrAcid] = new(0, 2304),
    };
}
