namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>The random field a projectile animation's random branch reads.</summary>
internal static class EnemyProjectileRandomBranchDefinitions
{
    /// <summary>The random field the one-in-four branch reads: the sample divided by 16384.</summary>
    internal static readonly RandomSampleField OneInFourBranch = new(14, 2);

    /// <summary>The <see cref="OneInFourBranch"/> value that takes the branch: both bits set.</summary>
    internal const int OneInFourBranchTaken = 3;
}
