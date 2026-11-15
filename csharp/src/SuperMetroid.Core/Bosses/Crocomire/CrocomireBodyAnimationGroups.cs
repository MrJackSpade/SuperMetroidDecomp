using System.Collections.Frozen;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The body animations Crocomire's fight and death test as groups (#1452, #1886), each named for what its test decides and
/// listing only the animations his body can stand in when the test is made.
/// </summary>
internal static class CrocomireBodyAnimationGroups
{
    /// <summary>
    /// The step forward still under way: the delayed step and the step itself, the animations his body plays throughout the
    /// stepping fight phase. That phase starts the step again only once his body stands in none of them.
    /// </summary>
    internal static readonly FrozenSet<CrocomireBodySegment> StepForwardInProgress = new[]
    {
        CrocomireBodySegment.StepForwardAfterDelay, CrocomireBodySegment.StepForward,
    }.ToFrozenSet();

    /// <summary>
    /// The damage wait before its roar has closed his mouth: the step back a hit starts, the wait itself, the moving claws,
    /// the roar and the mouth closing. The wait roars again only once his body has reached the closed-mouth loop or the
    /// reaction it runs on into.
    /// </summary>
    internal static readonly FrozenSet<CrocomireBodySegment> RoarYetToCloseMouth = new[]
    {
        CrocomireBodySegment.StepBack, CrocomireBodySegment.SteppingBack, CrocomireBodySegment.WaitForFirstSecondDamage,
        CrocomireBodySegment.MovingClaws, CrocomireBodySegment.Roar, CrocomireBodySegment.RoarCloseMouth,
    }.ToFrozenSet();

    /// <summary>The skeleton has come to rest: stable, dead, or flowing down the river.</summary>
    internal static readonly FrozenSet<CrocomireBodySegment> SkeletonAtRest = new[]
    {
        CrocomireBodySegment.SkeletonStable, CrocomireBodySegment.Dead, CrocomireBodySegment.SkeletonFlowingDownRiver,
    }.ToFrozenSet();
}
