namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The fixed speeds, offsets and turns Torizo's movement routines and projectile pre-instructions use.</summary>
internal static class TorizoMotionDefinitions
{
    /// <summary>
    /// A raw 8.8 speed of sixteen pixels per frame: a falling projectile at this speed and below twice it has run away.
    /// </summary>
    internal const int RunawaySpeed = 4096;

    /// <summary>
    /// The upward raw 8.8 speed, half a pixel per frame, below which a bounced Chozo orb's rebound has settled.
    /// </summary>
    internal const int SettledBounceSpeed = 128;

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
