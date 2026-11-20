using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The cue runner of a projectile animation that publishes no cues (#2554): its timeline holds frames and flow only, so no
/// cue reaches it (<see cref="NoAnimationCue"/> has no values).
/// </summary>
internal static class EnemyProjectileCuelessAnimations
{
    /// <summary>Answers a cue of a timeline that publishes none, which only an undefined value can be.</summary>
    /// <typeparam name="TSegment">The family's segment identity.</typeparam>
    /// <param name="cue">The undefined cue.</param>
    /// <exception cref="InvalidOperationException">Always: <see cref="NoAnimationCue"/> has no values.</exception>
    internal static AnimationContinuation<TSegment>? Undefined<TSegment>(NoAnimationCue cue)
        where TSegment : struct =>
        throw new InvalidOperationException($"Undefined {nameof(NoAnimationCue)} {cue}.");
}
