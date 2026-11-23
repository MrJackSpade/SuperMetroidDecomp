namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Botwoon's movement behaviour: how his body travels each update. He heads for his target hole, walks an authored path
/// (<see cref="AlongPath"/>, which holds the walk he chose), or has finished and stands still until his next update
/// chooses what follows.
/// </summary>
public abstract partial class BotwoonMovement
{
    /// <summary>Closes the movements to the ones declared here.</summary>
    private BotwoonMovement()
    {
    }

    /// <summary>Heads straight for his target hole until he has crossed into or out of the wall.</summary>
    public static BotwoonMovement MoveTowardHole { get; } = new Standing();

    /// <summary>
    /// He has crossed into or out of the wall, or walked off his path's end: he stands still until his next update, which
    /// chooses what follows instead of moving.
    /// </summary>
    public static BotwoonMovement Finished { get; } = new Standing();

    /// <summary>A movement that holds nothing beyond which one it is.</summary>
    private sealed partial class Standing : BotwoonMovement
    {
    }

    /// <summary>
    /// Walks his authored path, as many steps an update as his speed stage takes, until he walks off its end.
    /// </summary>
    public sealed partial class AlongPath : BotwoonMovement
    {
        /// <summary>Starts walking <paramref name="walk"/> from its first step.</summary>
        /// <param name="walk">The walk he chose: its path, the way he walks it and the step he starts at.</param>
        internal AlongPath(BotwoonPathWalk walk)
        {
            Path = walk.Path;
            Traversal = walk.Traversal;
            Step = walk.FirstStep;
        }

        /// <summary>The authored path being walked.</summary>
        internal BotwoonPathDefinition Path { get; }

        /// <summary>Which way <see cref="Path"/> is walked: a backward walk takes the steps in reverse order, each reversed.</summary>
        public BotwoonPathTraversal Traversal { get; }

        /// <summary>
        /// The next step of <see cref="Path"/> to take. A forward walk ends when it passes the last step and a backward
        /// walk when it passes the first, so it ranges from one before the first step to one after the last.
        /// </summary>
        public int Step { get; internal set; }
    }
}
