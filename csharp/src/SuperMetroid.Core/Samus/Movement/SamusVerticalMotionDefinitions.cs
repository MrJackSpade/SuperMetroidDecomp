namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Samus's vertical motion: the speeds she rebounds, launches and jumps at, the falls Space Jump restarts from, and the gravity in each medium.</summary>
internal static class SamusVerticalMotionDefinitions
{
    /// <summary>
    /// The upward speed of the Morph Ball's first rebound; the second rebound is one pixel per frame slower, and
    /// both rebounds have zero fractional speed.
    /// </summary>
    internal static UnsignedSixteenSixteen BallBounceSpeed { get; } = new(1, 0);

    /// <summary>The bomb jump's initial vertical speeds: bomb launch is weakened equally in both liquids,
    /// as upward speeds by medium.</summary>
    internal static readonly IReadOnlyDictionary<SamusLiquidMedium, UnsignedSixteenSixteen> BombJumpSpeeds =
        new Dictionary<SamusLiquidMedium, UnsignedSixteenSixteen>
    {
        [SamusLiquidMedium.Air] = UnsignedSixteenSixteen.FromPixelFraction(11, 4),
        [SamusLiquidMedium.Water] = UnsignedSixteenSixteen.FromPixelFraction(1, 16),
        [SamusLiquidMedium.LavaOrAcid] = UnsignedSixteenSixteen.FromPixelFraction(1, 16),
    };

    /// <summary>The knockback's initial vertical speeds: hurt launch ignores jump equipment and dash speed,
    /// as upward speeds by medium.</summary>
    internal static readonly IReadOnlyDictionary<SamusLiquidMedium, UnsignedSixteenSixteen> KnockbackSpeeds =
        new Dictionary<SamusLiquidMedium, UnsignedSixteenSixteen>
    {
        [SamusLiquidMedium.Air] = UnsignedSixteenSixteen.FromWholePixels(5),
        [SamusLiquidMedium.Water] = UnsignedSixteenSixteen.FromWholePixels(2),
        [SamusLiquidMedium.LavaOrAcid] = UnsignedSixteenSixteen.FromWholePixels(2),
    };

    /// <summary>
    /// The initial Y speeds: the launch magnitude with High Jump equipped by medium and
    /// whether Samus kicks away from a wall, as upward speeds. In lava or acid High Jump outweighs the wall kick.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusLaunchSpeedKey, UnsignedSixteenSixteen> HighJumpLaunchSpeeds =
        new Dictionary<SamusLaunchSpeedKey, UnsignedSixteenSixteen>
    {
        [new(SamusLiquidMedium.Air, SamusLaunchKind.WallJump)] = UnsignedSixteenSixteen.FromPixelFraction(11, 2),
        [new(SamusLiquidMedium.Air, SamusLaunchKind.Jump)] = UnsignedSixteenSixteen.FromWholePixels(6),
        [new(SamusLiquidMedium.Water, SamusLaunchKind.WallJump)] = UnsignedSixteenSixteen.FromPixelFraction(1, 2),
        [new(SamusLiquidMedium.Water, SamusLaunchKind.Jump)] = UnsignedSixteenSixteen.FromPixelFraction(5, 2),
        [new(SamusLiquidMedium.LavaOrAcid, SamusLaunchKind.WallJump)] = UnsignedSixteenSixteen.FromPixelFraction(7, 2),
        [new(SamusLiquidMedium.LavaOrAcid, SamusLaunchKind.Jump)] = UnsignedSixteenSixteen.FromPixelFraction(7, 2),
    };

    /// <summary>
    /// The initial Y speeds: the launch magnitude without High Jump by medium and whether
    /// Samus kicks away from a wall, as upward speeds.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusLaunchSpeedKey, UnsignedSixteenSixteen> UnassistedLaunchSpeeds =
        new Dictionary<SamusLaunchSpeedKey, UnsignedSixteenSixteen>
    {
        [new(SamusLiquidMedium.Air, SamusLaunchKind.WallJump)] = UnsignedSixteenSixteen.FromPixelFraction(37, 8),
        [new(SamusLiquidMedium.Air, SamusLaunchKind.Jump)] = UnsignedSixteenSixteen.FromPixelFraction(39, 8),
        [new(SamusLiquidMedium.Water, SamusLaunchKind.WallJump)] = UnsignedSixteenSixteen.FromPixelFraction(1, 4),
        [new(SamusLiquidMedium.Water, SamusLaunchKind.Jump)] = UnsignedSixteenSixteen.FromPixelFraction(7, 4),
        [new(SamusLiquidMedium.LavaOrAcid, SamusLaunchKind.WallJump)] = UnsignedSixteenSixteen.FromPixelFraction(21, 8),
        [new(SamusLiquidMedium.LavaOrAcid, SamusLaunchKind.Jump)] = UnsignedSixteenSixteen.FromPixelFraction(11, 4),
    };

    /// <summary>
    /// Each medium's gravity, the downward speed Samus gains per update: seven sixty-fourths of a pixel in air, a
    /// thirty-second in water and a thirty-second and a 256th in lava or acid.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusLiquidMedium, UnsignedSixteenSixteen> Gravities =
        new Dictionary<SamusLiquidMedium, UnsignedSixteenSixteen>
    {
        [SamusLiquidMedium.Air] = UnsignedSixteenSixteen.FromPixelFraction(7, 64),
        [SamusLiquidMedium.Water] = UnsignedSixteenSixteen.FromPixelFraction(1, 32),
        [SamusLiquidMedium.LavaOrAcid] = UnsignedSixteenSixteen.FromPixelFraction(9, 256),
    };

    /// <summary>The slowest fall in air from which Space Jump restarts the spin jump: two and a half pixels per update.</summary>
    internal static UnsignedSixteenSixteen SpaceJumpAirMinimumFallingSpeed { get; } = UnsignedSixteenSixteen.FromPixelFraction(5, 2);

    /// <summary>
    /// The slowest fall in a liquid from which Space Jump restarts the spin jump: half a pixel per update, the medium
    /// being the one the preceding animation pass remembered.
    /// </summary>
    internal static UnsignedSixteenSixteen SpaceJumpLiquidMinimumFallingSpeed { get; } = UnsignedSixteenSixteen.FromPixelFraction(1, 2);

    /// <summary>The falling speed at and above which Space Jump no longer restarts the spin jump: five pixels per update.</summary>
    internal static UnsignedSixteenSixteen SpaceJumpFallingSpeedLimit { get; } = new(5, 0);

    /// <summary>
    /// The whole pixels an update Ceres Ridley's shove drops Samus at from its start, and below which its fall gathers
    /// speed.
    /// </summary>
    internal const int CeresRidleyShoveFallingSpeed = 5;

    /// <summary>The whole pixels an update at which a falling bouncing ball stops gathering speed.</summary>
    internal const int BallBounceFallingSpeed = 5;

    /// <summary>
    /// The whole pixels an update at which a falling Samus stops gathering speed: gravity is added to her fall only while
    /// its whole speed is not exactly this, so a fall set faster by something else keeps its speed.
    /// </summary>
    internal const int TerminalFallWholeSpeed = 5;

    /// <summary>The upward speed of the Morph Ball's second rebound: one pixel an update slower than its first, so none.</summary>
    internal static UnsignedSixteenSixteen SecondBallBounceSpeed { get; } = new(0, 0);
}
