namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Pinned NTSC vertical mechanics; presentation overrides do not alter physics.</summary>
internal static class SamusVerticalMotionDefinitions
{
    /// <summary>
    /// The upward speed of the Morph Ball's first rebound; the second rebound is one pixel per frame slower, and
    /// both NTSC rebounds have zero fractional speed.
    /// </summary>
    internal static UnsignedSixteenSixteen BallBounceSpeed { get; } = new(1, 0);

    /// <summary>The bomb jump's initial vertical speeds: bomb launch is weakened equally in both liquids,
    /// as unsigned 1/65536-pixel speeds by medium.</summary>
    internal static readonly IReadOnlyDictionary<SamusLiquidMedium, UnsignedSixteenSixteen> BombJumpSpeeds =
        new Dictionary<SamusLiquidMedium, UnsignedSixteenSixteen>
    {
        [SamusLiquidMedium.Air] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(44),
        [SamusLiquidMedium.Water] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(1),
        [SamusLiquidMedium.LavaOrAcid] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(1),
    };

    /// <summary>The knockback's initial vertical speeds: hurt launch ignores jump equipment and dash speed,
    /// as unsigned 1/65536-pixel speeds by medium.</summary>
    internal static readonly IReadOnlyDictionary<SamusLiquidMedium, UnsignedSixteenSixteen> KnockbackSpeeds =
        new Dictionary<SamusLiquidMedium, UnsignedSixteenSixteen>
    {
        [SamusLiquidMedium.Air] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(80),
        [SamusLiquidMedium.Water] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(32),
        [SamusLiquidMedium.LavaOrAcid] = UnsignedSixteenSixteen.FromSixteenthsOfPixel(32),
    };

    /// <summary>
    /// The initial Y speeds: the launch magnitude with High Jump equipped by medium and
    /// whether Samus kicks away from a wall, as unsigned 1/65536-pixel speeds. In lava or acid High Jump outweighs the wall kick.
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
    /// The initial Y speeds: the launch magnitude without High Jump by medium and whether
    /// Samus kicks away from a wall, as unsigned 1/65536-pixel speeds.
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

    /// <summary>YSubAcceleration/YAcceleration: each medium's gravity, a fraction of a pixel per frame in
    /// all three media.</summary>
    internal static readonly IReadOnlyDictionary<SamusLiquidMedium, UnsignedSixteenSixteen> Gravities =
        new Dictionary<SamusLiquidMedium, UnsignedSixteenSixteen>
    {
        [SamusLiquidMedium.Air] = new(0, 7168),
        [SamusLiquidMedium.Water] = new(0, 2048),
        [SamusLiquidMedium.LavaOrAcid] = new(0, 2304),
    };
}
