namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>The random field a projectile animation's random branch reads.</summary>
internal static class EnemyProjectileRandomBranchDefinitions
{
    /// <summary>The random choice the one-in-four branch reads: one of the four quarters of the random number's range.</summary>
    internal static readonly RandomSampleField OneInFourBranch = new(14, 2);

    /// <summary>The <see cref="OneInFourBranch"/> value that takes the branch: the last of its four.</summary>
    internal const int OneInFourBranchTaken = 3;
}
