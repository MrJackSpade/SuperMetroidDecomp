namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Pinned vertical mechanics; presentation overrides do not alter physics.</summary>
internal static class SamusVerticalMotionDefinitions
{
    /// <summary>
    /// The upward speed of the Morph Ball's first rebound; the second rebound is one pixel per frame slower, and
    /// both rebounds have zero fractional speed.
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

    /// <summary>
    /// Each medium's gravity, the downward speed Samus gains per update: seven sixty-fourths of a pixel in air, a
    /// thirty-second in water and nine 256ths in lava or acid.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusLiquidMedium, UnsignedSixteenSixteen> Gravities =
        new Dictionary<SamusLiquidMedium, UnsignedSixteenSixteen>
    {
        [SamusLiquidMedium.Air] = UnsignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(28),
        [SamusLiquidMedium.Water] = UnsignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(8),
        [SamusLiquidMedium.LavaOrAcid] = UnsignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(9),
    };

    /// <summary>The slowest fall in air from which Space Jump restarts the spin jump: two and a half pixels per update.</summary>
    internal static UnsignedSixteenSixteen SpaceJumpAirMinimumFallingSpeed { get; } = UnsignedSixteenSixteen.FromSixteenthsOfPixel(40);

    /// <summary>
    /// The slowest fall in a liquid from which Space Jump restarts the spin jump: half a pixel per update, the medium
    /// being the one the preceding animation pass remembered.
    /// </summary>
    internal static UnsignedSixteenSixteen SpaceJumpLiquidMinimumFallingSpeed { get; } = UnsignedSixteenSixteen.FromSixteenthsOfPixel(8);

    /// <summary>The falling speed at and above which Space Jump no longer restarts the spin jump: five pixels per update.</summary>
    internal static UnsignedSixteenSixteen SpaceJumpFallingSpeedLimit { get; } = new(5, 0);

    /// <summary>
    /// The whole pixels an update Ceres Ridley's shove drops Samus at from its start, and below which its fall gathers
    /// speed.
    /// </summary>
    internal const int CeresRidleyShoveFallingSpeed = 5;

    /// <summary>The whole pixels an update at which a falling bouncing ball stops gathering speed.</summary>
    internal const int BallBounceFallingSpeed = 5;
}
