using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A cue owner that runs no cue, for an animation a check builds only to see where a production setup places and starts it;
/// only verification uses it.
/// </summary>
/// <typeparam name="TSegment">The animation family's segments.</typeparam>
/// <typeparam name="TCue">The animation family's cues.</typeparam>
internal sealed class NoBranchingCues<TSegment, TCue> : IEnemyProjectileBranchingCues<TSegment, TCue>
    where TSegment : struct
{
    /// <inheritdoc/>
    public AnimationContinuation<TSegment>? RunCue(EnemyProjectileCueContext context, TCue cue, AnimationPlayback<TSegment> pastCue) => null;
}
