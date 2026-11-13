using M = SuperMetroid.Core.Samus.Movement.SamusMovementType;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's horizontal speeds: each movement's acceleration, maximum and deceleration in air, in water and in lava or
/// acid, and the speeds of the two movements that set their own (a diagonal bomb jump and a grapple release).
/// </summary>
internal static class SamusHorizontalSpeedDefinitions
{
    /// <summary>Deceleration of every movement in air, half a pixel per update per update.</summary>
    private static SignedSixteenSixteen AirDeceleration { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(8);
    /// <summary>Deceleration of every movement in water, a thirty-second of a pixel per update per update.</summary>
    private static SignedSixteenSixteen WaterDeceleration { get; } = SignedSixteenSixteen.FromParts(0, 2048);
    /// <summary>Deceleration of every movement in lava or acid, a quarter of a pixel per update per update.</summary>
    private static SignedSixteenSixteen LavaDeceleration { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(4);
    /// <summary>The maximum of a movement that holds Samus still: standing, crouching, turning and running into a wall.</summary>
    private static SignedSixteenSixteen StillMaximum { get; } = SignedSixteenSixteen.Zero;
    /// <summary>Maximum running speed in air and water.</summary>
    private static SignedSixteenSixteen RunningMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(44);
    /// <summary>Maximum running speed in lava or acid.</summary>
    private static SignedSixteenSixteen LavaRunningMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(28);
    /// <summary>Maximum speed of normal jumps, posture transitions, airborne spring ball and grappling.</summary>
    private static SignedSixteenSixteen JumpingMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(20);
    /// <summary>Maximum speed of spin and wall jumps.</summary>
    private static SignedSixteenSixteen SpinJumpingMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(22);
    /// <summary>Maximum speed of a grounded morph or spring ball in air.</summary>
    private static SignedSixteenSixteen AirMorphBallGroundMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(52);
    /// <summary>Maximum speed of a grounded morph or spring ball in a liquid.</summary>
    private static SignedSixteenSixteen LiquidMorphBallGroundMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(44);
    /// <summary>Maximum speed of falling, and of a falling ball in air.</summary>
    private static SignedSixteenSixteen FallingMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(16);
    /// <summary>Maximum speed of a falling morph or spring ball in water.</summary>
    private static SignedSixteenSixteen WaterBallFallingMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(24);
    /// <summary>Maximum speed of a falling morph or spring ball in lava or acid.</summary>
    private static SignedSixteenSixteen LavaBallFallingMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(22);
    /// <summary>Maximum speed of the alternate unused glitch ball and the compact airborne poses.</summary>
    private static SignedSixteenSixteen GlitchBallMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(32);
    /// <summary>Maximum speed of knockback, Draygon's hold, special movement and an airborne damage boost.</summary>
    private static SignedSixteenSixteen KnockbackMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(80);
    /// <summary>Maximum speed of moonwalking and a damage boost in a liquid, half a pixel per update.</summary>
    private static SignedSixteenSixteen MoonwalkingMaximum { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(8);
    /// <summary>Running acceleration in air.</summary>
    private static SignedSixteenSixteen AirRunningAcceleration { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(3);
    /// <summary>Acceleration of running in a liquid and of a morph or spring ball in a liquid.</summary>
    private static SignedSixteenSixteen LiquidGroundAcceleration { get; } = SignedSixteenSixteen.FromParts(0, 1024);
    /// <summary>Acceleration of both unused glitch balls.</summary>
    private static SignedSixteenSixteen GlitchBallAcceleration { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(32);
    /// <summary>Knockback acceleration.</summary>
    private static SignedSixteenSixteen KnockbackAcceleration { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(24);
    /// <summary>The acceleration of every other movement, three quarters of a pixel per update per update.</summary>
    private static SignedSixteenSixteen DefaultAcceleration { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(12);

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

    /// <summary>A diagonal bomb jump's speeds, which it sets whatever Samus's movement and medium.</summary>
    internal static SamusHorizontalSpeeds DiagonalBombJump { get; } = new(
        Acceleration: SignedSixteenSixteen.FromParts(0, 12288),
        MaximumSpeed: SignedSixteenSixteen.FromParts(3, 0),
        Deceleration: SignedSixteenSixteen.FromParts(0, 2048));

    /// <summary>A grapple release's speeds, the same in every medium, which it sets whatever Samus's movement.</summary>
    internal static SamusHorizontalSpeeds GrappleRelease { get; } = new(
        Acceleration: SignedSixteenSixteen.FromParts(0, 12288),
        MaximumSpeed: SignedSixteenSixteen.FromParts(15, 0),
        Deceleration: SignedSixteenSixteen.FromParts(0, 4096));
}
