using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>The runner of the families whose events are the shared ones, which the engine carries out (#2571).</summary>
internal static class EnemyProjectileCueRunners
{
    /// <summary>The runner of a family whose events are the shared events, which the engine carries out.</summary>
    /// <typeparam name="TSegment">The family's segment identity.</typeparam>
    internal static EnemyProjectileCueRunner<TSegment, EnemyProjectileCommonCue> Common<TSegment>() where TSegment : struct =>
        CommonRunner<TSegment>.Instance;

    /// <summary>Carries out the shared events in the engine.</summary>
    private sealed class CommonRunner<TSegment> : EnemyProjectileCueRunner<TSegment, EnemyProjectileCommonCue>
        where TSegment : struct
    {
        /// <summary>The one runner per segment identity.</summary>
        internal static CommonRunner<TSegment> Instance { get; } = new();

        /// <inheritdoc/>
        internal override AnimationShown<TSegment, TFrame> PlayToNextFrame<TFrame>(
            AnimationSegments<TSegment, TFrame, EnemyProjectileCommonCue> segments, EnemyProjectileCueContext context,
            ref Animation<TSegment> animation, ref AnimationPlace place) =>
            Ensure.NotNull(segments).PlayToNextFrame(ref animation, ref place, cue =>
            {
                context.Projectiles.ApplyCommonCue(context.Projectile, cue);
                return null;
            });
    }
}
