namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Where Botwoon stands on the authored path he walks (#2553): the next step of <see cref="Path"/> to take, or one of the
/// two positions just outside it, one before its first step and one after its last, where a walk ends. A walk starts on
/// one of the path's steps and moves one step at a time only from a step, so no other position can be reached.
/// </summary>
internal sealed class BotwoonPathPosition
{
    /// <summary>Stands at <paramref name="step"/> of <paramref name="path"/>, which lies in range.</summary>
    private BotwoonPathPosition(BotwoonPathDefinition path, int step)
    {
        Path = path;
        Step = step;
    }

    /// <summary>The authored path being walked.</summary>
    internal BotwoonPathDefinition Path { get; }

    /// <summary>The next step of <see cref="Path"/> to take, from one before the first step to one after the last.</summary>
    internal int Step { get; }

    /// <summary>The start of <paramref name="walk"/>: its path at its first step, one of the path's steps.</summary>
    internal static BotwoonPathPosition StartOf(BotwoonPathWalk walk) => new(Ensure.NotNull(walk.Path), walk.FirstStep);

    /// <summary>
    /// Stands at <paramref name="step"/> of <paramref name="path"/>, as a check or a recorded state supplies it: a step of
    /// the path or one of the two positions just outside it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="step"/> lies further outside the path.</exception>
    internal static BotwoonPathPosition At(BotwoonPathDefinition path, int step)
    {
        Ensure.NotNull(path);
        return new(path, Ensure.BetweenInclusive(-1, path.Steps.Length, step));
    }

    /// <summary>
    /// Takes the step this position stands on, the way <paramref name="traversal"/> walks the path: its authored one-pixel
    /// movement and the position after it, one step on, which may be just past the path's end. A position past either end
    /// takes no step: the walk has ended there.
    /// </summary>
    /// <param name="traversal">Which way the path is walked.</param>
    /// <param name="movement">The step's movement as authored, before the traversal's reversal.</param>
    /// <param name="next">The position after the step.</param>
    /// <returns>Whether the position stood on a step, so the walk goes on.</returns>
    internal bool TryTake(BotwoonPathTraversal traversal, out Offset16<RoomPixels> movement, out BotwoonPathPosition next)
    {
        if (Step < 0 || Step >= Path.Steps.Length)
        {
            movement = Offset16<RoomPixels>.Zero;
            next = this;
            return false;
        }
        movement = BotwoonNavigationDefinitions.StepMovements[Path.Steps[Step]];
        next = new(Path, Step + BotwoonNavigationDefinitions.TraversalStrides[traversal]);
        return true;
    }
}
