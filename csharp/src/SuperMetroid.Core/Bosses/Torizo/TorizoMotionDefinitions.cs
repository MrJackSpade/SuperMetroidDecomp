namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The fixed speeds, offsets and turns Torizo's body and projectile motions use.</summary>
internal static class TorizoMotionDefinitions
{
    /// <summary>
    /// A speed of sixteen pixels per frame: a projectile that reaches it has run away (#1945). Each projectile tested
    /// for it starts below it and gains at most three sixteenths of a pixel an update each update, so it is met within one
    /// pixel of it.
    /// </summary>
    internal static readonly SignedEightEight RunawaySpeed = SignedEightEight.FromPixels(16);

    /// <summary>
    /// A resting Torizo's fall speed, one pixel an update: its setup, a landing and a walk off a ledge give it this
    /// speed, and a Torizo meeting the ground at it lands without a quake (<see cref="TorizoEnemyState.FallsAtRestingSpeed"/>).
    /// </summary>
    internal static readonly SignedEightEight RestingFallSpeed = SignedEightEight.FromPixels(1);

    /// <summary>The updates a landing Torizo's quake shakes the room.</summary>
    internal const int LandingQuakeUpdates = 32;

    /// <summary>Golden Torizo's forward leap's rise, five and three-quarter pixels an update.</summary>
    internal static readonly SignedEightEight ForwardLeapRise = SignedEightEight.FromPixelFraction(-23, 4);

    /// <summary>Golden Torizo's forward leap's sideways speed, toward the way it faces.</summary>
    internal static readonly SignedEightEight ForwardLeapSideSpeed = SignedEightEight.FromPixels(2);

    /// <summary>Either Torizo's backward jump's rise, four and a half pixels an update.</summary>
    internal static readonly SignedEightEight BackwardJumpRise = SignedEightEight.FromPixelFraction(-9, 2);

    /// <summary>Either Torizo's backward jump's sideways speed, away from the way it faces.</summary>
    internal static readonly SignedEightEight BackwardJumpSideSpeed = SignedEightEight.FromPixels(3);

    /// <summary>The gravity on a leaping Torizo, each update.</summary>
    internal static readonly SignedEightEight LeapGravity = SignedEightEight.FromPixelFraction(5, 32);

    /// <summary>A sonic boom's launch speed, about two and a half pixels an update, the way Torizo faces.</summary>
    internal static readonly SignedEightEight SonicBoomLaunchSpeed = SignedEightEight.FromPixelFraction(39, 16);

    /// <summary>The drag a falling drool's sideways speed loses each update.</summary>
    internal static readonly SignedEightEight DroolDrag = SignedEightEight.FromPixelFraction(1, 64);

    /// <summary>The sideways speed a drool keeps when its drag would carry it past rest, rightward.</summary>
    internal static readonly SignedEightEight DroolPastRestDrift = SignedEightEight.FromPixelFraction(3, 256);

    /// <summary>
    /// The gravity on Bomb Torizo's Chozo orb, each update. A falling orb that reaches
    /// <see cref="RunawaySpeed"/> is removed, so its speed never grows past that.
    /// </summary>
    internal static readonly SignedEightEight BombOrbGravity = SignedEightEight.FromPixelFraction(9, 128);

    /// <summary>
    /// The gravity on Golden Torizo's Chozo orb. Each floor bounce halves and reverses its fall, and the orb falls at
    /// most the height of its room, at most fifty screens (<see cref="Rooms.Scrolling.RoomScrollGrid.KeptCellCount"/>),
    /// before its terrain probe leaves the room and finds it solid, so its speed stays below 64 pixels an update.
    /// </summary>
    internal static readonly SignedEightEight GoldenOrbGravity = SignedEightEight.FromPixelFraction(3, 32);

    /// <summary>The gravity on the drool and the thrown Super Missile; at <see cref="RunawaySpeed"/> either is removed.</summary>
    internal static readonly SignedEightEight DroolAndMissileGravity = SignedEightEight.FromPixelFraction(1, 16);

    /// <summary>
    /// The sonic boom's acceleration along its flight. Flying right, it is removed at <see cref="RunawaySpeed"/>;
    /// flying left, it crosses at most the width of its room, at most fifty screens, before its terrain probe leaves the
    /// room and finds it solid, so its speed stays below 64 pixels an update.
    /// </summary>
    internal static readonly SignedEightEight SonicBoomAcceleration = SignedEightEight.FromPixelFraction(1, 16);

    /// <summary>
    /// Golden Torizo's egg's gravity while it bounces and falls, and its acceleration once it hatches and charges: a
    /// bouncing egg is removed at <see cref="RunawaySpeed"/>, and a charging or falling egg crosses at most the width or
    /// height of its room, at most fifty screens, before its terrain probe leaves the room and finds it solid, so its
    /// speed stays below 128 pixels an update.
    /// </summary>
    internal static readonly SignedEightEight EggAcceleration = SignedEightEight.FromPixelFraction(3, 16);

    /// <summary>
    /// The gravity on a falling Bomb Torizo. It falls at most the height of its room, at most fifty screens, before its
    /// terrain probe leaves the room and finds it solid, and landing resets its speed, so its speed stays below 64 pixels
    /// an update.
    /// </summary>
    internal static readonly SignedEightEight StatueGravity = SignedEightEight.FromPixelFraction(5, 32);

    /// <summary>The horizontal speed Golden Torizo's orb loses on each floor bounce.</summary>
    internal static readonly SignedEightEight GoldenOrbBounceDrag = SignedEightEight.FromPixelFraction(1, 4);

    /// <summary>The speed Golden Torizo's egg rolls on the way it faces as it starts to hatch: a pixel an update.</summary>
    internal static readonly SignedEightEight EggHatchingSpeed = SignedEightEight.FromPixels(1);

    /// <summary>The horizontal speed Golden Torizo's egg loses on each floor bounce.</summary>
    internal static readonly SignedEightEight EggBounceDrag = SignedEightEight.FromPixelFraction(1, 8);

    /// <summary>
    /// The upward speed, half a pixel per frame, below which a bounced Chozo orb's rebound has settled.
    /// </summary>
    internal static readonly SignedEightEight SettledBounceSpeed = SignedEightEight.FromPixelFraction(1, 2);

    /// <summary>The pixels the initial drool is lifted above the jittered spawn height.</summary>
    internal const int InitialDroolLift = 5;

    /// <summary>
    /// The initial drool's slowest fall, to which its random fall jitter adds: three sixteenths of a pixel an update, and
    /// the finest step a speed takes more, which the game adds whenever the lifted height lies at or below the room's
    /// top, as a Torizo's height always keeps it (#1620).
    /// </summary>
    internal static readonly SignedEightEight InitialDroolSlowestFall = SignedEightEight.FromPixelFraction(49, 256);

    /// <summary>The pixels ahead of the jittered spawn position, the way the Torizo faces, the initial drool is placed.</summary>
    internal const int InitialDroolSideOffset = 8;

    /// <summary>
    /// A facing Torizo's low-health drool heading facing right, an eighth of a turn from up toward its facing (#2032);
    /// headings point up at zero.
    /// </summary>
    internal static readonly TrigonometryAngle DroolHeadingFacingRight = new(32);

    /// <summary>A facing Torizo's low-health drool heading facing left, an eighth of a turn from up toward its facing.</summary>
    internal static readonly TrigonometryAngle DroolHeadingFacingLeft = new(224);

    /// <summary>
    /// The steps a facing Torizo's drool spread (<see cref="TorizoRandomDefinitions.DroolFacingSpread"/>) is turned back by,
    /// so the spread lies either side of the heading.
    /// </summary>
    internal const int DroolSpreadCentre = 8;

    /// <summary>The low-health drool's launch speed, one pixel an update along its heading.</summary>
    internal static readonly SignedEightEight DroolLaunchSpeed = SignedEightEight.FromPixels(1);

    /// <summary>
    /// Golden Torizo's eye-beam heading facing right, three eighths of a turn from up: ahead and down (#2032).
    /// </summary>
    internal static readonly TrigonometryAngle EyeBeamHeadingFacingRight = new(96);

    /// <summary>Golden Torizo's eye-beam heading facing left, three eighths of a turn from up the other way: ahead and down.</summary>
    internal static readonly TrigonometryAngle EyeBeamHeadingFacingLeft = new(160);

    /// <summary>
    /// The steps the eye beam's spread (<see cref="TorizoRandomDefinitions.EyeBeamSpread"/>) is turned back by, so the
    /// spread lies either side of the heading.
    /// </summary>
    internal const int EyeBeamSpreadCentre = 8;

    /// <summary>The eye beam's speed, eight pixels an update along its heading.</summary>
    internal static readonly SignedEightEight EyeBeamSpeed = SignedEightEight.FromPixels(8);

    /// <summary>A thrown Super Missile's speed, four pixels an update toward or away from Samus.</summary>
    internal static readonly SignedEightEight ThrownSuperMissileSpeed = SignedEightEight.FromPixels(4);

    /// <summary>
    /// The updates every Golden Torizo egg lasts before it hatches, hatching on the last: every egg waits the same
    /// (#1945).
    /// </summary>
    internal const int EggHatchUpdates = 67;
}
