namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>The random field a projectile animation's random choice of its next animation reads.</summary>
internal static class EnemyProjectileRandomBranchDefinitions
{
<<<<<<< HEAD
    /// <summary>The random choice the one-in-four branch reads: one of the four quarters of the random number's range.</summary>
||||||| 273b556990
    /// <summary>The random field the one-in-four branch reads: the sample divided by 16384.</summary>
=======
    /// <summary>The random field the one-in-four choice reads: the sample divided by 16384.</summary>
>>>>>>> worktree-agent-a25205e6d337c0739
    internal static readonly RandomSampleField OneInFourBranch = new(14, 2);

    /// <summary>The <see cref="OneInFourBranch"/> value that changes the animation: the last of its four.</summary>
    internal const int OneInFourBranchTaken = 3;
}
