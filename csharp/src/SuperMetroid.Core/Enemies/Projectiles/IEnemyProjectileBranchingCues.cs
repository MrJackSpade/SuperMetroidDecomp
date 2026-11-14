using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile behaviour that carries out the cues of type <typeparamref name="TCue"/> its kinds' typed animations
/// publish, where a cue may continue the play elsewhere (#1760, #1788): in another animation from its start, as a
/// conditional branch does, or at the start of the run of steps the cue is in, as a conditional replay does.
/// </summary>
/// <typeparam name="TSegment">The animation family's identity.</typeparam>
/// <typeparam name="TCue">The cues of the animation family.</typeparam>
internal interface IEnemyProjectileBranchingCues<TSegment, in TCue>
    where TSegment : struct
{
    /// <summary>
    /// Carries out <paramref name="cue"/>, published on the context's projectile, returning where the play continues, or
    /// null to run on.
    /// </summary>
    /// <param name="context">The projectile, Samus and the camera of the pass running the animation.</param>
    /// <param name="cue">The cue the animation published.</param>
    /// <param name="pastCue">Where the play stands just past the cue.</param>
    AnimationContinuation<TSegment>? RunCue(EnemyProjectileCueContext context, TCue cue, AnimationPlayback<TSegment> pastCue);
}
