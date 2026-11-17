namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The Ki-Hunter body's flight, swoop, hop, landing and spit animations and its wings' animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="KiHunterAnimations"/>.
/// </summary>
internal enum KiHunterSegment
{
    /// <summary>Hovering facing left, dipping once, before it flies on.</summary>
    FlyingLeft,
    /// <summary>
    /// Swooping facing left: its arms swinging back and in, a hover, then its legs reaching and stretched out, before
    /// it flies on.
    /// </summary>
    SwoopLeft,
    /// <summary>Hovering facing right, dipping once, before it flies on.</summary>
    FlyingRight,
    /// <summary>
    /// Swooping facing right: its arms swinging back and in, a hover, then its legs reaching and stretched out, before
    /// it flies on.
    /// </summary>
    SwoopRight,
    /// <summary>Its wings facing left flapping, raised, mid and level, repeating.</summary>
    WingsLeft,
    /// <summary>Its wings facing right flapping, raised, mid and level, repeating.</summary>
    WingsRight,
    /// <summary>Its wings torn off, held as they fall.</summary>
    DetachedWings,
    /// <summary>
    /// Crouching facing left, crouching low and springing, then crouching again before it leaps into a hop.
    /// </summary>
    JumpLeft,
    /// <summary>
    /// Crouching facing right, crouching low and springing, then crouching again before it leaps into a hop.
    /// </summary>
    JumpRight,
    /// <summary>
    /// Landing facing left: crouching low, springing and crouching again before it chooses its next move on foot.
    /// </summary>
    LandLeft,
    /// <summary>
    /// Landing facing right: crouching low, springing and crouching again before it chooses its next move on foot.
    /// </summary>
    LandRight,
    /// <summary>
    /// Crouched facing left, its mouth swelling with acid until it spits it left, then crouched again before it chooses
    /// its next move.
    /// </summary>
    SpitLeft,
    /// <summary>
    /// Crouched facing right, its mouth swelling with acid until it spits it right, then crouched again before it
    /// chooses its next move.
    /// </summary>
    SpitRight,
}
