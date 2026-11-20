namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// What a projectile's animation does when its frame expires (#1607): shows a frame for a duration, shows a frame for
/// good, waits on the frame it shows, rests on it, or ends (#2553).
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

    /// <summary>The animation has ended.</summary>
    internal static EnemyProjectileFrameStep Ends { get; } = new End();

    /// <summary>The animation shows <paramref name="Frame"/> for <paramref name="Duration"/> projectile passes.</summary>
    /// <param name="Frame">The frame shown.</param>
    /// <param name="Duration">How many projectile passes the frame lasts.</param>
    internal sealed record Show(EnemyProjectileFrameId Frame, int Duration) : EnemyProjectileFrameStep
    {
        /// <inheritdoc/>
        internal override void React<TContext>(EnemyProjectileFrameStepReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Show(this, context);
    }

    /// <summary>The animation shows <paramref name="Frame"/> from now on and rests on it.</summary>
    /// <param name="Frame">The frame shown.</param>
    internal sealed record ShowForGood(EnemyProjectileFrameId Frame) : EnemyProjectileFrameStep
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

    /// <summary>The animation keeps the frame it shows for <paramref name="Duration"/> projectile passes, showing no new one.</summary>
    /// <param name="Duration">How many projectile passes the wait lasts.</param>
    internal sealed record Wait(int Duration) : EnemyProjectileFrameStep
    {
        /// <inheritdoc/>
        internal override void React<TContext>(EnemyProjectileFrameStepReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Wait(this, context);
    }

    /// <summary>The animation has ended.</summary>
    internal sealed record End : EnemyProjectileFrameStep
    {
        /// <inheritdoc/>
        internal override void React<TContext>(EnemyProjectileFrameStepReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).End(this, context);
    }
}

/// <summary>What the owner of a <see cref="EnemyProjectileFrameStep"/> does on each of its cases (#2553).</summary>
/// <param name="Show">Reacts to a <see cref="EnemyProjectileFrameStep.Show"/>.</param>
/// <param name="ShowForGood">Reacts to a <see cref="EnemyProjectileFrameStep.ShowForGood"/>.</param>
/// <param name="Rest">Reacts to a <see cref="EnemyProjectileFrameStep.Rest"/>.</param>
/// <param name="Wait">Reacts to a <see cref="EnemyProjectileFrameStep.Wait"/>.</param>
/// <param name="End">Reacts to a <see cref="EnemyProjectileFrameStep.End"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record EnemyProjectileFrameStepReactions<TContext>(
    Action<EnemyProjectileFrameStep.Show, TContext> Show,
    Action<EnemyProjectileFrameStep.ShowForGood, TContext> ShowForGood,
    Action<EnemyProjectileFrameStep.Rest, TContext> Rest,
    Action<EnemyProjectileFrameStep.Wait, TContext> Wait,
    Action<EnemyProjectileFrameStep.End, TContext> End);
