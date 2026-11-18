namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>A Norfair Pipe Bug's part in its formation (#1840): when it departs and the stagger it runs after rising.</summary>
/// <param name="StaggerTarget">The formation count at which the member departs.</param>
/// <param name="Stagger">
/// The move a member's <see cref="NorfairPipeBugAction.Stagger"/> makes from where its rise ended (#2071, #2473); the
/// leader, which holds its height, has none.
/// </param>
internal sealed record NorfairPipeBugRole(int StaggerTarget, NorfairPipeBugStagger? Stagger)
{
    /// <summary>
    /// The stagger the member runs once it has risen past Samus: the leader's that only counts, or every other member's
    /// move to its place.
    /// </summary>
    internal NorfairPipeBugAction PostRiseAction => Stagger is null ? NorfairPipeBugAction.LeaderStagger : NorfairPipeBugAction.Stagger;
}
