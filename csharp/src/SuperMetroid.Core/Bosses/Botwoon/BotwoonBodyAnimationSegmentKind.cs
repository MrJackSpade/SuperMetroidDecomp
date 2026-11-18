namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>What a body segment shows.</summary>
internal enum BotwoonBodyAnimationSegmentKind
{
    /// <summary>An outside body link: four frames that cycle.</summary>
    Body,
    /// <summary>The outside tail cap: one frame.</summary>
    Tail,
    /// <summary>A link or tail inside the wall: nothing drawn.</summary>
    Hidden,
}
