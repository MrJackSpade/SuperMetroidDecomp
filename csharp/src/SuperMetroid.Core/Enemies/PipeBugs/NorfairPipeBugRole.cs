namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// A Norfair Pipe Bug's part in its formation (#1840, #2554): when it departs and the stagger it runs after rising, the
/// leader's that holds its height or a member's move to its place.
/// </summary>
/// <param name="StaggerTarget">The formation count at which the member departs.</param>
internal abstract record NorfairPipeBugRole(int StaggerTarget)
{
    /// <summary>
    /// The stagger the member runs once it has risen past Samus: the leader's that only counts, or every other member's
    /// move to its place.
    /// </summary>
    internal abstract NorfairPipeBugAction PostRiseAction { get; }

    /// <summary>The leader, which holds its height after rising.</summary>
    /// <param name="StaggerTarget">The formation count at which it departs.</param>
    internal sealed record Leader(int StaggerTarget) : NorfairPipeBugRole(StaggerTarget)
    {
        /// <inheritdoc/>
        internal override NorfairPipeBugAction PostRiseAction => NorfairPipeBugAction.LeaderStagger;
    }

    /// <summary>A member other than the leader, which moves on from where its rise ended (#2071, #2473).</summary>
    /// <param name="StaggerTarget">The formation count at which it departs.</param>
    /// <param name="Stagger">The move its <see cref="NorfairPipeBugAction.Stagger"/> makes from where its rise ended.</param>
    internal sealed record Follower(int StaggerTarget, NorfairPipeBugStagger Stagger) : NorfairPipeBugRole(StaggerTarget)
    {
        /// <inheritdoc/>
        internal override NorfairPipeBugAction PostRiseAction => NorfairPipeBugAction.Stagger;
    }
}
