namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// A segment of a family's <c>Segments</c> (#1600), or the animation of its <c>StartingAt</c> starting at a segment: a
/// program a slot is started in is the animation starting at it, which plays the segments its phases name.
/// </summary>
/// <param name="IsStart">Whether it is the animation starting at <paramref name="Name"/>, rather than that segment's beats.</param>
/// <param name="Name">The segment.</param>
internal readonly record struct AnimationNode(bool IsStart, string Name)
{
    /// <summary>The animation starting at <paramref name="name"/>: a program a slot or object is started in.</summary>
    public static AnimationNode Start(string name) => new(IsStart: true, name);

    /// <summary>The beats of segment <paramref name="name"/>.</summary>
    public static AnimationNode Segment(string name) => new(IsStart: false, name);
}
