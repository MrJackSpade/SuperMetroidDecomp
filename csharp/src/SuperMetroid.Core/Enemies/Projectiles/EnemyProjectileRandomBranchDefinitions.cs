namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>The random field a projectile animation's random choice of its next animation reads.</summary>
internal static class EnemyProjectileRandomBranchDefinitions
{
    /// <summary>The random field the one-in-four choice reads: the sample divided by 16384.</summary>
    internal static readonly RandomSampleField OneInFourBranch = new(14, 2);

    /// <summary>The <see cref="OneInFourBranch"/> value that changes the animation: the last of its four.</summary>
    internal const int OneInFourBranchTaken = 3;
}
