namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The fixed speeds, offsets and turns Torizo's body and projectile motions use.</summary>
internal static class TorizoMotionDefinitions
{
    /// <summary>
    /// A speed of sixteen pixels per frame: a projectile that reaches it has run away (#1945). Each projectile tested
    /// for it starts below it and gains at most 48 256ths of a pixel an update, so it is met within one pixel of it.
    /// </summary>
    internal static readonly SignedEightEight RunawaySpeed = SignedEightEight.FromParts(16, 0);

    /// <summary>
    /// A resting Torizo's fall speed, one pixel an update: its setup, a landing and a walk off a ledge give it this
    /// speed, and a Torizo meeting the ground at it lands without a quake (<see cref="TorizoEnemyState.FallsAtRestingSpeed"/>).
    /// </summary>
    internal static readonly SignedEightEight RestingFallSpeed = SignedEightEight.FromPixels(1);

    /// <summary>The updates a landing Torizo's quake shakes the room.</summary>
    internal const int LandingQuakeUpdates = 32;

    /// <summary>Golden Torizo's forward leap's rise, five and three-quarter pixels an update.</summary>
    internal static readonly SignedEightEight ForwardLeapRise = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-1472);

    /// <summary>Golden Torizo's forward leap's sideways speed, toward the way it faces.</summary>
    internal static readonly SignedEightEight ForwardLeapSideSpeed = SignedEightEight.FromPixels(2);

    /// <summary>Either Torizo's backward jump's rise, four and a half pixels an update.</summary>
    internal static readonly SignedEightEight BackwardJumpRise = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-1152);

    /// <summary>Either Torizo's backward jump's sideways speed, away from the way it faces.</summary>
    internal static readonly SignedEightEight BackwardJumpSideSpeed = SignedEightEight.FromPixels(3);

    /// <summary>The gravity on a leaping Torizo, forty 256ths of a pixel an update, each update.</summary>
    internal static readonly SignedEightEight LeapGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(40);

    /// <summary>A sonic boom's launch speed, about two and a half pixels an update, the way Torizo faces.</summary>
    internal static readonly SignedEightEight SonicBoomLaunchSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(624);

    /// <summary>The drag a falling drool's sideways speed loses each update.</summary>
    internal static readonly SignedEightEight DroolDrag = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(4);

    /// <summary>The sideways speed a drool keeps when its drag would carry it past rest: three 256ths of a pixel rightward.</summary>
    internal static readonly SignedEightEight DroolPastRestDrift = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(3);

    /// <summary>
    /// The gravity on Bomb Torizo's Chozo orb, in 256ths of a pixel an update, each update. A falling orb that reaches
    /// <see cref="RunawaySpeed"/> is removed, so its speed never leaves the 1/256-pixel range.
    /// </summary>
    internal static readonly SignedEightEight BombOrbGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(18);

    /// <summary>
    /// The gravity on Golden Torizo's Chozo orb. Each floor bounce halves and reverses its fall, and the orb falls at
    /// most the height of its room, at most fifty screens (<see cref="Rooms.Scrolling.RoomScrollGrid.KeptCellCount"/>),
    /// before its terrain probe leaves the room and finds it solid, so its speed stays below 64 pixels an update.
    /// </summary>
    internal static readonly SignedEightEight GoldenOrbGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(24);

    /// <summary>The gravity on the drool and the thrown Super Missile; at <see cref="RunawaySpeed"/> either is removed.</summary>
    internal static readonly SignedEightEight DroolAndMissileGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(16);

    /// <summary>
    /// The sonic boom's acceleration along its flight. Flying right, it is removed at <see cref="RunawaySpeed"/>;
    /// flying left, it crosses at most the width of its room, at most fifty screens, before its terrain probe leaves the
    /// room and finds it solid, so its speed stays below 64 pixels an update.
    /// </summary>
    internal static readonly SignedEightEight SonicBoomAcceleration = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(16);

    /// <summary>
    /// Golden Torizo's egg's gravity while it bounces and falls, and its acceleration once it hatches and charges: a
    /// bouncing egg is removed at <see cref="RunawaySpeed"/>, and a charging or falling egg crosses at most the width or
    /// height of its room, at most fifty screens, before its terrain probe leaves the room and finds it solid, so its
    /// speed stays below 128 pixels an update.
    /// </summary>
    internal static readonly SignedEightEight EggAcceleration = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(48);

    /// <summary>
    /// The gravity on a falling Bomb Torizo. It falls at most the height of its room, at most fifty screens, before its
    /// terrain probe leaves the room and finds it solid, and landing resets its speed, so its speed stays below 64 pixels
    /// an update.
    /// </summary>
    internal static readonly SignedEightEight StatueGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(40);

    /// <summary>The horizontal speed Golden Torizo's orb loses on each floor bounce.</summary>
    internal static readonly SignedEightEight GoldenOrbBounceDrag = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(64);

    /// <summary>The horizontal speed Golden Torizo's egg loses on each floor bounce.</summary>
    internal static readonly SignedEightEight EggBounceDrag = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(32);

    /// <summary>
    /// The upward speed, half a pixel per frame, below which a bounced Chozo orb's rebound has settled.
    /// </summary>
    internal static readonly SignedEightEight SettledBounceSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(128);

    /// <summary>The pixels the initial drool is lifted above the jittered spawn height.</summary>
    internal const int InitialDroolLift = 5;

    /// <summary>
    /// The initial drool's slowest fall, in 256ths of a pixel an update, to which its random fall jitter adds: 48, and
    /// the one 256th the game adds whenever the lifted height lies at or below the room's top, which a Torizo's height
    /// always keeps it (#1620).
    /// </summary>
    internal const int InitialDroolSlowestFall = 49;

    /// <summary>
    /// The pixels the initial drool is placed to the facing side of the jittered spawn position (the game
    /// adds eight facing right and minus eight facing left).
    /// </summary>
    internal const int InitialDroolSideOffset = 8;

    /// <summary>
    /// Golden Torizo's egg hatch timer, 66 updates for every egg: the egg's initializer meant to draw it at random but
    /// reads a fixed value (#1945).
    /// </summary>
    internal const int EggHatchTimer = 66;
}
