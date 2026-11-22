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

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this enemy projectile common cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(EnemyProjectileCommonCueReactions<TContext> reactions, TContext context);


    /// <summary>Scatters the projectile randomly about its position.</summary>
    /// <param name="Spread">The random repositioning a <see cref="Scatter"/> cue performs.</param>
    internal sealed partial record Scatter(EnemyProjectileScatter Spread) : EnemyProjectileCommonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(EnemyProjectileCommonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Scatter(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(EnemyProjectileCommonCueKind Kind) : EnemyProjectileCommonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(EnemyProjectileCommonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Signal(this, context);
    }
}
