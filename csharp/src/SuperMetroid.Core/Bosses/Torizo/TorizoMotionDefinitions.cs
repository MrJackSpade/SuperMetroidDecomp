namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The fixed speeds, offsets and turns Torizo's movement routines and projectile frame behaviours use.</summary>
internal static class TorizoMotionDefinitions
{
    /// <summary>
    /// A speed of sixteen pixels per frame: a falling projectile at this speed and below twice it has run away.
    /// </summary>
    internal static readonly SignedEightEight RunawaySpeed = SignedEightEight.FromParts(16, 0);

    /// <summary>Twice <see cref="RunawaySpeed"/>, thirty-two pixels per frame: the top of the runaway range.</summary>
    internal static readonly SignedEightEight TwiceRunawaySpeed = SignedEightEight.FromPixels(32);

    /// <summary>The drag a falling drool's sideways speed loses each update.</summary>
    internal static readonly SignedEightEight DroolDrag = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(4);

    /// <summary>The sideways speed a drool keeps when its drag would carry it past rest: three 256ths of a pixel rightward.</summary>
    internal static readonly SignedEightEight DroolPastRestDrift = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(3);

    /// <summary>
    /// The gravity on Bomb Torizo's Chozo orb, in 256ths of a pixel an update, each update. A falling orb that reaches
    /// <see cref="RunawaySpeed"/> is removed, so its speed never leaves the 8.8 range.
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

    /// <summary>The pixels of one room block: a landing projectile is placed relative to its block's top.</summary>
    internal const int BlockPixels = 16;

    /// <summary>
    /// The pixels the initial drool is lifted above the jittered spawn height. The original game adds this as
    /// minus five, whose carry (set unless the lift wraps) its fall speed adds.
    /// </summary>
    internal const int InitialDroolLift = 5;

    /// <summary>
    /// The pixels the initial drool is placed to the facing side of the jittered spawn position (the game
    /// adds eight facing right and minus eight facing left).
    /// </summary>
    internal const int InitialDroolSideOffset = 8;

    /// <summary>
    /// Golden Torizo's egg hatch timer. The egg's initializer masks a fixed value rather than a random number, an
    /// authentic bug that fixes the timer at 66 updates.
    /// </summary>
    internal const int EggHatchTimer = (226 & 0b1_1111) + 64;
}
