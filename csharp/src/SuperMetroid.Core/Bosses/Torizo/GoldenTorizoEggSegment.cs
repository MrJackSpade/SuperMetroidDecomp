namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The animation segments of Golden Torizo's eggs and their hatchlings (#641, #1788, #2235, #2554): each animation begins at the
/// segment named for what it shows; the runs it repeats, loops on or moves on to are segments of their own
/// (<see cref="GoldenTorizoEggAnimations"/>).
/// </summary>
internal enum GoldenTorizoEggSegment
{
    /// <summary>An egg bouncing left until its hatch timer wakes it into <see cref="EggHatchingLeft"/>.</summary>
    EggBouncingLeft,
    /// <summary>An egg hatching facing left: it stops, cracks open, turns harmful and shot-blocking, then hatches into <see cref="EggHatchedLeft"/>.</summary>
    EggHatchingLeft,
    /// <summary>An egg bouncing right until its hatch timer wakes it into <see cref="EggHatchingRight"/>.</summary>
    EggBouncingRight,
    /// <summary>An egg hatching facing right: it stops, cracks open, turns harmful and shot-blocking, then hatches into <see cref="EggHatchedRight"/>.</summary>
    EggHatchingRight,
    /// <summary>A cracked egg's hatch, either way: it turns harmful and shot-blocking, then continues in its hatched animation.</summary>
    EggHatch,
    /// <summary>A hatched egg's creature charging left: its sound, then its four poses for good.</summary>
    EggHatchedLeft,
    /// <summary>A hatched egg's creature charging left: its four poses, looping.</summary>
    HatchlingChargingLeft,
    /// <summary>A hatched egg's creature charging right: its sound, then its four poses for good.</summary>
    EggHatchedRight,
    /// <summary>A hatched egg's creature charging right: its four poses, looping.</summary>
    HatchlingChargingRight,
    /// <summary>A shot egg breaking facing left: it stops, plays its five poses, its flattened remains two updates longer than facing right, then is deleted.</summary>
    EggBreakLeft,
    /// <summary>A shot egg breaking facing right: it stops, plays its five poses, then is deleted.</summary>
    EggBreakRight,
    /// <summary>A shot egg or hatchling bursting as a Chozo orb does on a wall: it stops, plays its five poses, then is deleted.</summary>
    Shot,
}
