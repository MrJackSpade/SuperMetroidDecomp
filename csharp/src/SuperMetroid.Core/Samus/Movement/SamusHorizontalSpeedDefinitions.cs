using M = SuperMetroid.Core.Samus.Movement.SamusMovementType;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's horizontal speeds: each movement's acceleration, maximum and deceleration in air, in water and in lava or
/// acid, and the speeds of the two movements that set their own (a diagonal bomb jump and a grapple release).
/// </summary>
internal static class SamusHorizontalSpeedDefinitions
{
    /// <summary>The extra run speed the Speed Booster's dash is held at: seven pixels an update.</summary>
    internal static readonly UnsignedSixteenSixteen BoostedExtraRunSpeedCap = UnsignedSixteenSixteen.FromWholePixels(7);

    /// <summary>The extra run speed a dash without the Speed Booster is held at: two pixels an update.</summary>
    internal static readonly UnsignedSixteenSixteen UnboostedExtraRunSpeedCap = UnsignedSixteenSixteen.FromWholePixels(2);

    /// <summary>The most times the slowdown level halves the total speed, however high the level (#2280).</summary>
    internal const int LargestSpeedHalvings = 4;

    /// <summary>The most whole pixels Samus's speed moves her in one update, either way (#2280).</summary>
    internal const int LargestWholeDisplacement = 15;

    /// <summary>Deceleration of every movement in air, half a pixel per update per update.</summary>
    private static UnsignedSixteenSixteen AirDeceleration { get; } = UnsignedSixteenSixteen.FromPixelFraction(1, 2);
    /// <summary>Deceleration of every movement in water, a thirty-second of a pixel per update per update.</summary>
    private static UnsignedSixteenSixteen WaterDeceleration { get; } = UnsignedSixteenSixteen.FromPixelFraction(1, 32);
    /// <summary>Deceleration of every movement in lava or acid, a quarter of a pixel per update per update.</summary>
    private static UnsignedSixteenSixteen LavaDeceleration { get; } = UnsignedSixteenSixteen.FromPixelFraction(1, 4);
    /// <summary>The maximum of a movement that holds Samus still: standing, crouching, turning and running into a wall.</summary>
    private static UnsignedSixteenSixteen StillMaximum { get; } = UnsignedSixteenSixteen.FromWholePixels(0);
    /// <summary>Maximum running speed in air and water.</summary>
    private static UnsignedSixteenSixteen RunningMaximum { get; } = UnsignedSixteenSixteen.FromPixelFraction(11, 4);
    /// <summary>Maximum running speed in lava or acid.</summary>
    private static UnsignedSixteenSixteen LavaRunningMaximum { get; } = UnsignedSixteenSixteen.FromPixelFraction(7, 4);
    /// <summary>Maximum speed of normal jumps, posture transitions, airborne spring ball and grappling.</summary>
    private static UnsignedSixteenSixteen JumpingMaximum { get; } = UnsignedSixteenSixteen.FromPixelFraction(5, 4);
    /// <summary>Maximum speed of spin and wall jumps.</summary>
    private static UnsignedSixteenSixteen SpinJumpingMaximum { get; } = UnsignedSixteenSixteen.FromPixelFraction(11, 8);
    /// <summary>Maximum speed of a grounded morph or spring ball in air.</summary>
    private static UnsignedSixteenSixteen AirMorphBallGroundMaximum { get; } = UnsignedSixteenSixteen.FromPixelFraction(13, 4);
    /// <summary>Maximum speed of a grounded morph or spring ball in a liquid.</summary>
    private static UnsignedSixteenSixteen LiquidMorphBallGroundMaximum { get; } = UnsignedSixteenSixteen.FromPixelFraction(11, 4);
    /// <summary>Maximum speed of falling, and of a falling ball in air.</summary>
    private static UnsignedSixteenSixteen FallingMaximum { get; } = UnsignedSixteenSixteen.FromWholePixels(1);
    /// <summary>Maximum speed of a falling morph or spring ball in water.</summary>
    private static UnsignedSixteenSixteen WaterBallFallingMaximum { get; } = UnsignedSixteenSixteen.FromPixelFraction(3, 2);
    /// <summary>Maximum speed of a falling morph or spring ball in lava or acid.</summary>
    private static UnsignedSixteenSixteen LavaBallFallingMaximum { get; } = UnsignedSixteenSixteen.FromPixelFraction(11, 8);
    /// <summary>Maximum speed of the glitch ball's knockback and the compact airborne poses.</summary>
    private static UnsignedSixteenSixteen GlitchBallMaximum { get; } = UnsignedSixteenSixteen.FromWholePixels(2);
    /// <summary>Maximum speed of knockback, Draygon's hold, special movement and an airborne damage boost.</summary>
    private static UnsignedSixteenSixteen KnockbackMaximum { get; } = UnsignedSixteenSixteen.FromWholePixels(5);
    /// <summary>Maximum speed of moonwalking and a damage boost in a liquid, half a pixel per update.</summary>
    private static UnsignedSixteenSixteen MoonwalkingMaximum { get; } = UnsignedSixteenSixteen.FromPixelFraction(1, 2);
    /// <summary>Running acceleration in air.</summary>
    private static UnsignedSixteenSixteen AirRunningAcceleration { get; } = UnsignedSixteenSixteen.FromPixelFraction(3, 16);
    /// <summary>
    /// Acceleration of running in a liquid and of a morph or spring ball in a liquid, a sixty-fourth of a pixel per
    /// update per update.
    /// </summary>
    private static UnsignedSixteenSixteen LiquidGroundAcceleration { get; } = UnsignedSixteenSixteen.FromPixelFraction(1, 64);
    /// <summary>Acceleration of the glitch ball and of its knockback.</summary>
    private static UnsignedSixteenSixteen GlitchBallAcceleration { get; } = UnsignedSixteenSixteen.FromWholePixels(2);
    /// <summary>Knockback acceleration.</summary>
    private static UnsignedSixteenSixteen KnockbackAcceleration { get; } = UnsignedSixteenSixteen.FromPixelFraction(3, 2);
    /// <summary>The acceleration of every other movement, three quarters of a pixel per update per update.</summary>
    private static UnsignedSixteenSixteen DefaultAcceleration { get; } = UnsignedSixteenSixteen.FromPixelFraction(3, 4);

    /// <summary>
    /// Each movement's speeds in air. Draygon's hold and special movement have none of their own in air: they take
    /// water's standing and running speeds (<see cref="WaterSpeedsInAir"/>).
    /// </summary>
    internal static IReadOnlyDictionary<M, SamusHorizontalSpeeds> Air { get; } = new Dictionary<M, SamusHorizontalSpeeds>
    {
        [M.Standing] = new(DefaultAcceleration, StillMaximum, AirDeceleration),
        [M.Running] = new(AirRunningAcceleration, RunningMaximum, AirDeceleration),
        [M.NormalJumping] = new(DefaultAcceleration, JumpingMaximum, AirDeceleration),
        [M.SpinJumping] = new(DefaultAcceleration, SpinJumpingMaximum, AirDeceleration),
        [M.MorphBallGround] = new(DefaultAcceleration, AirMorphBallGroundMaximum, AirDeceleration),
        [M.Crouching] = new(DefaultAcceleration, StillMaximum, AirDeceleration),
        [M.Falling] = new(DefaultAcceleration, FallingMaximum, AirDeceleration),
        [M.GlitchBall] = new(GlitchBallAcceleration, FallingMaximum, AirDeceleration),
        [M.MorphBallFalling] = new(DefaultAcceleration, FallingMaximum, AirDeceleration),
        [M.GlitchBallKnockback] = new(GlitchBallAcceleration, GlitchBallMaximum, AirDeceleration),
        [M.Knockback] = new(KnockbackAcceleration, KnockbackMaximum, AirDeceleration),
        [M.CompactAirborne] = new(DefaultAcceleration, GlitchBallMaximum, AirDeceleration),
        [M.TurningOnGround] = new(DefaultAcceleration, StillMaximum, AirDeceleration),
        [M.PostureTransition] = new(DefaultAcceleration, JumpingMaximum, AirDeceleration),
        [M.Moonwalking] = new(DefaultAcceleration, MoonwalkingMaximum, AirDeceleration),
        [M.SpringBallGround] = new(DefaultAcceleration, AirMorphBallGroundMaximum, AirDeceleration),
        [M.SpringBallInAir] = new(DefaultAcceleration, JumpingMaximum, AirDeceleration),
        [M.SpringBallFalling] = new(DefaultAcceleration, FallingMaximum, AirDeceleration),
        [M.WallJumping] = new(DefaultAcceleration, SpinJumpingMaximum, AirDeceleration),
        [M.RanIntoWall] = new(DefaultAcceleration, StillMaximum, AirDeceleration),
        [M.Grappling] = new(DefaultAcceleration, JumpingMaximum, AirDeceleration),
        [M.TurningWhileJumping] = new(DefaultAcceleration, StillMaximum, AirDeceleration),
        [M.TurningWhileFalling] = new(DefaultAcceleration, StillMaximum, AirDeceleration),
        [M.DamageBoost] = new(DefaultAcceleration, KnockbackMaximum, AirDeceleration),
    };

    /// <summary>Each movement's speeds in water.</summary>
    internal static IReadOnlyDictionary<M, SamusHorizontalSpeeds> Water { get; } = new Dictionary<M, SamusHorizontalSpeeds>
    {
        [M.Standing] = new(DefaultAcceleration, StillMaximum, WaterDeceleration),
        [M.Running] = new(LiquidGroundAcceleration, RunningMaximum, WaterDeceleration),
        [M.NormalJumping] = new(DefaultAcceleration, JumpingMaximum, WaterDeceleration),
        [M.SpinJumping] = new(DefaultAcceleration, SpinJumpingMaximum, WaterDeceleration),
        [M.MorphBallGround] = new(LiquidGroundAcceleration, LiquidMorphBallGroundMaximum, WaterDeceleration),
        [M.Crouching] = new(DefaultAcceleration, StillMaximum, WaterDeceleration),
        [M.Falling] = new(DefaultAcceleration, FallingMaximum, WaterDeceleration),
        [M.GlitchBall] = new(GlitchBallAcceleration, FallingMaximum, WaterDeceleration),
        [M.MorphBallFalling] = new(LiquidGroundAcceleration, WaterBallFallingMaximum, WaterDeceleration),
        [M.GlitchBallKnockback] = new(GlitchBallAcceleration, GlitchBallMaximum, WaterDeceleration),
        [M.Knockback] = new(KnockbackAcceleration, KnockbackMaximum, WaterDeceleration),
        [M.CompactAirborne] = new(DefaultAcceleration, GlitchBallMaximum, WaterDeceleration),
        [M.TurningOnGround] = new(DefaultAcceleration, StillMaximum, WaterDeceleration),
        [M.PostureTransition] = new(DefaultAcceleration, JumpingMaximum, WaterDeceleration),
        [M.Moonwalking] = new(DefaultAcceleration, MoonwalkingMaximum, WaterDeceleration),
        [M.SpringBallGround] = new(LiquidGroundAcceleration, LiquidMorphBallGroundMaximum, WaterDeceleration),
        [M.SpringBallInAir] = new(LiquidGroundAcceleration, JumpingMaximum, WaterDeceleration),
        [M.SpringBallFalling] = new(LiquidGroundAcceleration, WaterBallFallingMaximum, WaterDeceleration),
        [M.WallJumping] = new(DefaultAcceleration, SpinJumpingMaximum, WaterDeceleration),
        [M.RanIntoWall] = new(DefaultAcceleration, StillMaximum, WaterDeceleration),
        [M.Grappling] = new(DefaultAcceleration, JumpingMaximum, WaterDeceleration),
        [M.TurningWhileJumping] = new(DefaultAcceleration, StillMaximum, WaterDeceleration),
        [M.TurningWhileFalling] = new(DefaultAcceleration, StillMaximum, WaterDeceleration),
        [M.DamageBoost] = new(DefaultAcceleration, MoonwalkingMaximum, WaterDeceleration),
        [M.DraygonHeld] = new(DefaultAcceleration, KnockbackMaximum, WaterDeceleration),
        [M.Special] = new(DefaultAcceleration, KnockbackMaximum, WaterDeceleration),
    };

    /// <summary>Each movement's speeds in lava or acid.</summary>
    internal static IReadOnlyDictionary<M, SamusHorizontalSpeeds> LavaOrAcid { get; } = new Dictionary<M, SamusHorizontalSpeeds>
    {
        [M.Standing] = new(DefaultAcceleration, StillMaximum, LavaDeceleration),
        [M.Running] = new(LiquidGroundAcceleration, LavaRunningMaximum, LavaDeceleration),
        [M.NormalJumping] = new(DefaultAcceleration, JumpingMaximum, LavaDeceleration),
        [M.SpinJumping] = new(DefaultAcceleration, SpinJumpingMaximum, LavaDeceleration),
        [M.MorphBallGround] = new(LiquidGroundAcceleration, LiquidMorphBallGroundMaximum, LavaDeceleration),
        [M.Crouching] = new(DefaultAcceleration, StillMaximum, LavaDeceleration),
        [M.Falling] = new(DefaultAcceleration, FallingMaximum, LavaDeceleration),
        [M.GlitchBall] = new(GlitchBallAcceleration, FallingMaximum, LavaDeceleration),
        [M.MorphBallFalling] = new(LiquidGroundAcceleration, LavaBallFallingMaximum, LavaDeceleration),
        [M.GlitchBallKnockback] = new(GlitchBallAcceleration, GlitchBallMaximum, LavaDeceleration),
        [M.Knockback] = new(KnockbackAcceleration, KnockbackMaximum, LavaDeceleration),
        [M.CompactAirborne] = new(DefaultAcceleration, GlitchBallMaximum, LavaDeceleration),
        [M.TurningOnGround] = new(DefaultAcceleration, StillMaximum, LavaDeceleration),
        [M.PostureTransition] = new(DefaultAcceleration, JumpingMaximum, LavaDeceleration),
        [M.Moonwalking] = new(DefaultAcceleration, MoonwalkingMaximum, LavaDeceleration),
        [M.SpringBallGround] = new(LiquidGroundAcceleration, LiquidMorphBallGroundMaximum, LavaDeceleration),
        [M.SpringBallInAir] = new(LiquidGroundAcceleration, JumpingMaximum, LavaDeceleration),
        [M.SpringBallFalling] = new(LiquidGroundAcceleration, LavaBallFallingMaximum, LavaDeceleration),
        [M.WallJumping] = new(DefaultAcceleration, SpinJumpingMaximum, LavaDeceleration),
        [M.RanIntoWall] = new(DefaultAcceleration, StillMaximum, LavaDeceleration),
        [M.Grappling] = new(DefaultAcceleration, JumpingMaximum, LavaDeceleration),
        [M.TurningWhileJumping] = new(DefaultAcceleration, StillMaximum, LavaDeceleration),
        [M.TurningWhileFalling] = new(DefaultAcceleration, StillMaximum, LavaDeceleration),
        [M.DamageBoost] = new(DefaultAcceleration, MoonwalkingMaximum, LavaDeceleration),
        [M.DraygonHeld] = new(DefaultAcceleration, KnockbackMaximum, LavaDeceleration),
        [M.Special] = new(DefaultAcceleration, KnockbackMaximum, LavaDeceleration),
    };

    /// <summary>Each medium's movement speeds.</summary>
    internal static IReadOnlyDictionary<SamusLiquidMedium, IReadOnlyDictionary<M, SamusHorizontalSpeeds>> ByMedium { get; } =
        new Dictionary<SamusLiquidMedium, IReadOnlyDictionary<M, SamusHorizontalSpeeds>>
        {
            [SamusLiquidMedium.Air] = Air,
            [SamusLiquidMedium.Water] = Water,
            [SamusLiquidMedium.LavaOrAcid] = LavaOrAcid,
        };

    /// <summary>
    /// The speeds Draygon's hold and special movement take in air (<see cref="WaterSpeedsInAir{TMovement, TSpeeds}"/>):
    /// held by Draygon, water's standing speeds; in special movement, water's running speeds.
    /// </summary>
    internal static IReadOnlyDictionary<M, SamusHorizontalSpeeds> WaterSpeedsInAir { get; } = new Dictionary<M, SamusHorizontalSpeeds>
    {
        [M.DraygonHeld] = Water[M.Standing],
        [M.Special] = Water[M.Running],
    };

    /// <summary>
    /// A diagonal bomb jump's speeds, which it sets whatever Samus's movement and medium: three sixteenths of a pixel per
    /// update per update up to three pixels per update, slowing by a thirty-second.
    /// </summary>
    internal static SamusHorizontalSpeeds DiagonalBombJump { get; } = new(
        Acceleration: UnsignedSixteenSixteen.FromPixelFraction(3, 16),
        MaximumSpeed: UnsignedSixteenSixteen.FromWholePixels(3),
        Deceleration: UnsignedSixteenSixteen.FromPixelFraction(1, 32));

    /// <summary>
    /// A grapple release's speeds, the same in every medium, which it sets whatever Samus's movement: three sixteenths of
    /// a pixel per update per update up to fifteen pixels per update, slowing by a sixteenth.
    /// </summary>
    internal static SamusHorizontalSpeeds GrappleRelease { get; } = new(
        Acceleration: UnsignedSixteenSixteen.FromPixelFraction(3, 16),
        MaximumSpeed: UnsignedSixteenSixteen.FromWholePixels(15),
        Deceleration: UnsignedSixteenSixteen.FromPixelFraction(1, 16));
}
