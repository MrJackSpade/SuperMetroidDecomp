namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>A Norfair Pipe Bug's part in its formation (#1840): when it departs and the stagger it runs after rising.</summary>
/// <param name="StaggerTarget">The formation count at which the member departs.</param>
/// <param name="PostRiseAction">The stagger the member runs once it has risen past Samus.</param>
/// <param name="StaggerOffset">
/// How far, in pixels, a member's <see cref="PipeBugAction.NorfairStagger"/> moves it from where its rise ended, up when
/// negative (#2071); the leader, which holds its height, moves none.
/// </param>
internal sealed record NorfairPipeBugRole(int StaggerTarget, PipeBugAction PostRiseAction, int StaggerOffset);
