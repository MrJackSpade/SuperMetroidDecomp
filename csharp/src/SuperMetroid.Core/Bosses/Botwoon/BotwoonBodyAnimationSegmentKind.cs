namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>What a body-segment actor shows.</summary>
internal enum BotwoonBodyAnimationSegmentKind
{
    /// <summary>An outside body link: four frames that cycle.</summary>
    Body,
    /// <summary>The outside tail cap: one frame.</summary>
    Tail,
    /// <summary>A link or tail inside the wall: nothing drawn.</summary>
    Hidden,
    /// <summary>
    /// Not a segment: a spit, which runs the same projectile animation through its cycling frames
    /// (the spit's own cycling animation).
    /// </summary>
    Spit,
}
