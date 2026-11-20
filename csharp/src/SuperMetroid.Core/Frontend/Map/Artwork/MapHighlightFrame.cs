namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// One frame of the map highlight cycle (#2361): the highlighted sprites show each frame's colours in turn, the first
/// following the last. How many frames the cycle holds is installed (<see cref="MapHighlightCycle"/>).
/// </summary>
public readonly record struct MapHighlightFrame
{
    /// <summary>The frame at <paramref name="frame"/> of the cycle.</summary>
    private MapHighlightFrame(int frame) => Frame = frame;

    /// <summary>The frame's place in the cycle, from zero: the step of its installed shade.</summary>
    public int Frame { get; }

    /// <summary>The frame at <paramref name="frame"/> of a cycle, counting from zero.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The place is negative.</exception>
    internal static MapHighlightFrame At(int frame) => new(Ensure.AtLeastZero(frame));
}
