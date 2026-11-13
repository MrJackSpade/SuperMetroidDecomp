using System.Collections.Frozen;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The body programs Crocomire's fight and death test as groups (#1452). The original game tests the body's program
/// by its place in the program layout, so each test admits the programs laid out on one side of one animation; each
/// group names those programs instead of ordering the program identities.
/// </summary>
internal static class CrocomireBodyAnimationGroups
{
    /// <summary>
    /// The programs laid out before the stepping-back list. The stepping fight function steps forward again only once
    /// the continuation has left them.
    /// </summary>
    internal static readonly FrozenSet<CrocomireBodySegment> BeforeSteppingBack = new[]
    {
        CrocomireBodySegment.Initial, CrocomireBodySegment.UnusedChargeForwardOneStep, CrocomireBodySegment.ProjectileAttack,
        CrocomireBodySegment.ProjectileAttackLoop, CrocomireBodySegment.StepForwardAfterDelay, CrocomireBodySegment.StepForward,
        CrocomireBodySegment.StepBack,
    }.ToFrozenSet();

    /// <summary>
    /// The programs laid out before the roar's closing-mouth loop. The damage wait roars again only once the
    /// continuation has left them.
    /// </summary>
    internal static readonly FrozenSet<CrocomireBodySegment> BeforeRoarCloseMouthLoop = new[]
    {
        CrocomireBodySegment.Initial, CrocomireBodySegment.UnusedChargeForwardOneStep, CrocomireBodySegment.ProjectileAttack,
        CrocomireBodySegment.ProjectileAttackLoop, CrocomireBodySegment.StepForwardAfterDelay, CrocomireBodySegment.StepForward,
        CrocomireBodySegment.StepBack, CrocomireBodySegment.SteppingBack, CrocomireBodySegment.WaitForFirstSecondDamage,
        CrocomireBodySegment.MovingClaws, CrocomireBodySegment.Roar, CrocomireBodySegment.RoarCloseMouth,
    }.ToFrozenSet();

    /// <summary>The skeleton has come to rest: stable, dead, or flowing down the river.</summary>
    internal static readonly FrozenSet<CrocomireBodySegment> SkeletonAtRest = new[]
    {
        CrocomireBodySegment.SkeletonStable, CrocomireBodySegment.Dead, CrocomireBodySegment.SkeletonFlowingDownRiver,
    }.ToFrozenSet();
}
