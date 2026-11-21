namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// One general cue of a typed projectile animation whose family adds no behavior of its own; the projectile
/// engine carries it out for every such family.
/// </summary>
internal abstract partial record EnemyProjectileCommonCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private EnemyProjectileCommonCue()
    {
    }

    /// <summary>Scatters the projectile randomly about its position.</summary>
    /// <param name="Spread">The random repositioning a <see cref="Scatter"/> cue performs.</param>
    internal sealed partial record Scatter(EnemyProjectileScatter Spread) : EnemyProjectileCommonCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(EnemyProjectileCommonCueKind Kind) : EnemyProjectileCommonCue;
}
