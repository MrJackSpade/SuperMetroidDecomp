namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// A Norfair Pipe Bug's part in its formation (#1840): when it departs and the stagger it runs after rising, the leader's
/// that holds its height or a follower's move to its place.
/// </summary>
/// <param name="StaggerTarget">The formation count at which the member departs.</param>
internal abstract partial record NorfairPipeBugRole(int StaggerTarget)
{
    /// <summary>The stagger the member runs once it has risen past Samus.</summary>
    internal abstract NorfairPipeBugAction PostRiseAction { get; }

    /// <summary>The leader, which holds its height and only counts.</summary>
    /// <param name="StaggerTarget">The formation count at which the leader departs.</param>
    internal sealed partial record Leader(int StaggerTarget) : NorfairPipeBugRole(StaggerTarget)
    {
        /// <inheritdoc/>
        internal override NorfairPipeBugAction PostRiseAction => NorfairPipeBugAction.LeaderStagger;
    }

    /// <summary>A follower, which moves to its place after rising.</summary>
    /// <param name="StaggerTarget">The formation count at which the follower departs.</param>
    /// <param name="Stagger">
    /// The move its <see cref="NorfairPipeBugAction.Stagger"/> makes from where its rise ended (#2071, #2473).
    /// </param>
    internal sealed partial record Follower(int StaggerTarget, NorfairPipeBugStagger Stagger) : NorfairPipeBugRole(StaggerTarget)
    {
        /// <inheritdoc/>
        internal override NorfairPipeBugAction PostRiseAction => NorfairPipeBugAction.Stagger;
    }
}
