namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// One general cue of a typed projectile animation whose family adds no behavior of its own; the projectile
/// engine carries it out for every such family.
/// </summary>
internal abstract record EnemyProjectileCommonCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private EnemyProjectileCommonCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this enemy projectile common cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    internal abstract void React(EnemyProjectileCommonCueReactions reactions);


    /// <summary>Scatters the projectile randomly about its position.</summary>
    /// <param name="Spread">The random repositioning a <see cref="Scatter"/> cue performs.</param>
    internal sealed record Scatter(EnemyProjectileScatter Spread) : EnemyProjectileCommonCue
    {
        /// <inheritdoc/>
        internal override void React(EnemyProjectileCommonCueReactions reactions) => Ensure.NotNull(reactions).Scatter(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(EnemyProjectileCommonCueKind Kind) : EnemyProjectileCommonCue
    {
        /// <inheritdoc/>
        internal override void React(EnemyProjectileCommonCueReactions reactions) => Ensure.NotNull(reactions).Signal(this);
    }
}

/// <summary>What the owner of a <see cref="EnemyProjectileCommonCue"/> does on each of its cases (#2553).</summary>
/// <param name="Scatter">Reacts to a <see cref="EnemyProjectileCommonCue.Scatter"/>.</param>
/// <param name="Signal">Reacts to a <see cref="EnemyProjectileCommonCue.Signal"/>.</param>
internal sealed record EnemyProjectileCommonCueReactions(Action<EnemyProjectileCommonCue.Scatter> Scatter, Action<EnemyProjectileCommonCue.Signal> Signal);
