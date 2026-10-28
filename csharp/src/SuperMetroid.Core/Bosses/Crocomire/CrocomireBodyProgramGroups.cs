using System.Collections.Frozen;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The body programs Crocomire's fight and death test as groups (#1452). The cartridge compares the body's
/// instruction-list pointer against one list's address, so each test admits the programs it lays out on one side of
/// that list; each group names those programs instead of ordering the program identities.
/// </summary>
internal static class CrocomireBodyProgramGroups
{
    /// <summary>
    /// The programs laid out before the stepping-back list. The stepping fight function steps forward again only once
    /// the continuation has left them.
    /// </summary>
    internal static readonly FrozenSet<CrocomireBodyProgram> BeforeSteppingBack = new[]
    {
        CrocomireBodyProgram.Initial, CrocomireBodyProgram.UnusedChargeForwardOneStep, CrocomireBodyProgram.ProjectileAttack,
        CrocomireBodyProgram.ProjectileAttackLoop, CrocomireBodyProgram.StepForwardAfterDelay, CrocomireBodyProgram.StepForward,
        CrocomireBodyProgram.StepBack,
    }.ToFrozenSet();

    /// <summary>
    /// The programs laid out before the roar's closing-mouth loop. The damage wait roars again only once the
    /// continuation has left them.
    /// </summary>
    internal static readonly FrozenSet<CrocomireBodyProgram> BeforeRoarCloseMouthLoop = new[]
    {
        CrocomireBodyProgram.Initial, CrocomireBodyProgram.UnusedChargeForwardOneStep, CrocomireBodyProgram.ProjectileAttack,
        CrocomireBodyProgram.ProjectileAttackLoop, CrocomireBodyProgram.StepForwardAfterDelay, CrocomireBodyProgram.StepForward,
        CrocomireBodyProgram.StepBack, CrocomireBodyProgram.SteppingBack, CrocomireBodyProgram.WaitForFirstSecondDamage,
        CrocomireBodyProgram.MovingClaws, CrocomireBodyProgram.Roar, CrocomireBodyProgram.RoarCloseMouth,
    }.ToFrozenSet();

    /// <summary>The skeleton has come to rest: stable, dead, or flowing down the river.</summary>
    internal static readonly FrozenSet<CrocomireBodyProgram> SkeletonAtRest = new[]
    {
        CrocomireBodyProgram.SkeletonStable, CrocomireBodyProgram.Dead, CrocomireBodyProgram.SkeletonFlowingDownRiver,
    }.ToFrozenSet();
}
