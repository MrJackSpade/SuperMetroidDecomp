namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The animation segments of the Torizos' Chozo orbs (#641, #1788, #2235, #2554): each animation begins at the
/// segment named for what it shows; the runs it repeats, loops on or moves on to are segments of their own
/// (<see cref="TorizoChozoOrbAnimations"/>).
/// </summary>
internal enum TorizoChozoOrbSegment
{
    /// <summary>A Chozo orb flying left, for good.</summary>
    ChozoOrbMovingLeft,
    /// <summary>A Chozo orb flying right, for good.</summary>
    ChozoOrbMovingRight,
    /// <summary>A Chozo orb bursting on a wall: it stops, plays its five poses, then is deleted.</summary>
    ChozoOrbWallImpact,
    /// <summary>A Chozo orb bursting on the floor: it stops, turns harmful, plays its impact sound and six poses, then is deleted.</summary>
    ChozoOrbFloorImpact,
    /// <summary>A shot Chozo orb bursting: it stops, plays its five poses, drops its pickup, then is deleted.</summary>
    ChozoOrbShot,
}
