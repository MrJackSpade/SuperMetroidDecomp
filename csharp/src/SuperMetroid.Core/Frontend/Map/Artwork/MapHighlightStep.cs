namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// One step of the installed map highlight cycle (#1512, #2359): the highlighted sprites show its shade while it lasts. The
/// cycle's steps are installed, so a step is known by its place in the cycle, counted from the first.
/// </summary>
/// <param name="Place">How many steps of the cycle come before it.</param>
public readonly record struct MapHighlightStep(int Place)
{
    /// <summary>The cycle's first step, which the highlight shows before its first update.</summary>
    public static MapHighlightStep First => new(0);
}
