namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// What a projectile's animation does when its frame expires (#1607): shows a frame for a duration, shows a frame for
/// good, or rests on the frame it shows.
/// </summary>
internal abstract record EnemyProjectileFrameStep
{
    /// <summary>Allows only the steps below.</summary>
    private EnemyProjectileFrameStep()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this enemy projectile frame step, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    internal abstract void React(EnemyProjectileFrameStepReactions reactions);


    /// <summary>The animation rests on the frame it shows: its timeline holds.</summary>
    internal static EnemyProjectileFrameStep Rests { get; } = new Rest();

    /// <summary>The animation shows <paramref name="Frame"/> for <paramref name="Duration"/> projectile passes.</summary>
    /// <param name="Frame">The frame shown.</param>
    /// <param name="Duration">How many projectile passes the frame lasts.</param>
    internal sealed record Show(EnemyProjectileFrameId Frame, int Duration) : EnemyProjectileFrameStep
    {
        /// <inheritdoc/>
        internal override void React(EnemyProjectileFrameStepReactions reactions) => Ensure.NotNull(reactions).Show(this);
    }

    /// <summary>The animation shows <paramref name="Frame"/> from now on and rests on it.</summary>
    /// <param name="Frame">The frame shown.</param>
    internal sealed record ShowForGood(EnemyProjectileFrameId Frame) : EnemyProjectileFrameStep
    {
        /// <inheritdoc/>
        internal override void React(EnemyProjectileFrameStepReactions reactions) => Ensure.NotNull(reactions).ShowForGood(this);
    }

    /// <summary>The animation rests on the frame it shows.</summary>
    internal sealed record Rest : EnemyProjectileFrameStep
    {
        /// <inheritdoc/>
        internal override void React(EnemyProjectileFrameStepReactions reactions) => Ensure.NotNull(reactions).Rest(this);
    }
}

/// <summary>What the owner of a <see cref="EnemyProjectileFrameStep"/> does on each of its cases (#2553).</summary>
/// <param name="Show">Reacts to a <see cref="EnemyProjectileFrameStep.Show"/>.</param>
/// <param name="ShowForGood">Reacts to a <see cref="EnemyProjectileFrameStep.ShowForGood"/>.</param>
/// <param name="Rest">Reacts to a <see cref="EnemyProjectileFrameStep.Rest"/>.</param>
internal sealed record EnemyProjectileFrameStepReactions(Action<EnemyProjectileFrameStep.Show> Show, Action<EnemyProjectileFrameStep.ShowForGood> ShowForGood, Action<EnemyProjectileFrameStep.Rest> Rest);
