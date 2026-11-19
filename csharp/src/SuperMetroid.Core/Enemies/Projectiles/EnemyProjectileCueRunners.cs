using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>The runners of the families no cue owner of their own carries out events for (#2571).</summary>
internal static class EnemyProjectileCueRunners
{
    /// <summary>The runner of a family whose events are the shared events, which the engine carries out.</summary>
    /// <typeparam name="TSegment">The family's segment identity.</typeparam>
    internal static EnemyProjectileCueRunner<TSegment, EnemyProjectileCommonCue> Common<TSegment>() where TSegment : struct =>
        CommonRunner<TSegment>.Instance;

    /// <summary>The runner of a family whose timelines raise no events (<see cref="NoAnimationCue"/> has no member).</summary>
    /// <typeparam name="TSegment">The family's segment identity.</typeparam>
    internal static EnemyProjectileCueRunner<TSegment, NoAnimationCue> None<TSegment>() where TSegment : struct =>
        NoRunner<TSegment>.Instance;

    /// <summary>Carries out the shared events in the engine.</summary>
    private sealed class CommonRunner<TSegment> : EnemyProjectileCueRunner<TSegment, EnemyProjectileCommonCue>
        where TSegment : struct
    {
        /// <summary>The one runner per segment identity.</summary>
        internal static CommonRunner<TSegment> Instance { get; } = new();

        /// <inheritdoc/>
        internal override AnimationContinuation<TSegment>? Run(EnemyProjectileCueContext context, EnemyProjectileEngine projectiles,
            EnemyProjectileCommonCue cue, AnimationPlayback<TSegment> pastCue)
        {
            projectiles.ApplyCommonCue(context.Projectile, cue);
            return null;
        }
    }

    /// <summary>The runner of timelines that raise no events: <see cref="NoAnimationCue"/> has no value an event could carry.</summary>
    private sealed class NoRunner<TSegment> : EnemyProjectileCueRunner<TSegment, NoAnimationCue>
        where TSegment : struct
    {
        /// <summary>The one runner per segment identity.</summary>
        internal static NoRunner<TSegment> Instance { get; } = new();

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">A timeline of no events raised one: its family's data is corrupt.</exception>
        internal override AnimationContinuation<TSegment>? Run(EnemyProjectileCueContext context, EnemyProjectileEngine projectiles,
            NoAnimationCue cue, AnimationPlayback<TSegment> pastCue) =>
            throw new InvalidOperationException(
                $"Enemy projectile {context.Projectile.Kind} in place {context.Projectile.Place} raised {cue} from a timeline of no events.");
    }
}
