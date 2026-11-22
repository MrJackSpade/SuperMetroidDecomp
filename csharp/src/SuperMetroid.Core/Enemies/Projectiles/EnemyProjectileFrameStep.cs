namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// What a projectile's animation does when its frame expires (#1607): shows a frame for a duration, shows a frame for
/// good, shows nothing for a duration (#2558), rests on the frame it shows, or ends by deleting its projectile.
/// </summary>
internal abstract record EnemyProjectileFrameStep
{
    /// <summary>Allows only the steps below.</summary>
    private EnemyProjectileFrameStep()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this enemy projectile frame step, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(EnemyProjectileFrameStepReactions<TContext> reactions, TContext context);

    /// <summary>The animation rests on the frame it shows: its timeline holds.</summary>
    internal static EnemyProjectileFrameStep Rests { get; } = new Rest();

    /// <summary>The animation ends, deleting its projectile.</summary>
    internal static EnemyProjectileFrameStep Deletes { get; } = new Delete();

    /// <summary>
    /// The animation shows <paramref name="Frame"/> for <paramref name="Duration"/> projectile passes, drawn
    /// <paramref name="Placement"/> off the projectile's place.
    /// </summary>
    /// <param name="Frame">The frame shown.</param>
    /// <param name="Duration">How many projectile passes the frame lasts.</param>
    /// <param name="Placement">Where the frame is drawn from the projectile's place (#2557).</param>
    internal sealed record Show(EnemyProjectileFrameId Frame, int Duration, Offset16<RoomPixels> Placement) : EnemyProjectileFrameStep
    {
        /// <inheritdoc/>
        internal override void React<TContext>(EnemyProjectileFrameStepReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Show(this, context);
    }

    /// <summary>The animation shows <paramref name="Frame"/> from now on, drawn <paramref name="Placement"/> off the projectile's place, and rests on it.</summary>
    /// <param name="Frame">The frame shown.</param>
    /// <param name="Placement">Where the frame is drawn from the projectile's place (#2557).</param>
    internal sealed record ShowForGood(EnemyProjectileFrameId Frame, Offset16<RoomPixels> Placement) : EnemyProjectileFrameStep
    {
        /// <inheritdoc/>
        internal override void React<TContext>(EnemyProjectileFrameStepReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).ShowForGood(this, context);
    }

    /// <summary>The animation rests on the frame it shows.</summary>
    internal sealed record Rest : EnemyProjectileFrameStep
    {
        /// <inheritdoc/>
        internal override void React<TContext>(EnemyProjectileFrameStepReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Rest(this, context);
    }

    /// <summary>The projectile shows nothing for <paramref name="Duration"/> updates (#2558), holding no shown frame meanwhile.</summary>
    /// <param name="Duration">The updates the projectile stays hidden.</param>
    internal sealed record Hide(int Duration) : EnemyProjectileFrameStep
    {
        /// <inheritdoc/>
        internal override void React<TContext>(EnemyProjectileFrameStepReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Hide(this, context);
    }

    /// <summary>The animation ends, and its end deletes the projectile: no step follows it.</summary>
    internal sealed record Delete : EnemyProjectileFrameStep
    {
        /// <inheritdoc/>
        internal override void React<TContext>(EnemyProjectileFrameStepReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Delete(this, context);
    }
}
