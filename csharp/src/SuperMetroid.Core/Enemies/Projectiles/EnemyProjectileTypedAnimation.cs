using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile animation whose typed timeline raises events (#1600, #1760): each event goes to the cue runner its owner
/// bound it to when it created it (#2571, <see cref="EnemyProjectileCueRunner{TSegment, TEvent}"/>): the family's cue owner,
/// which may continue the play elsewhere, as a conditional change to another animation or a conditional replay of the
/// cue's run does, or the engine for the shared <see cref="EnemyProjectileCommonCue"/> events.
/// </summary>
/// <typeparam name="TSegment">The family's segment identity.</typeparam>
/// <typeparam name="TFrame">The family's frame identity.</typeparam>
/// <typeparam name="TEvent">The family's events.</typeparam>
internal abstract class EnemyProjectileTypedAnimation<TSegment, TFrame, TEvent> : EnemyProjectileTimelineAnimation<TSegment, TFrame, TEvent>
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>Who carries out the events the animation raises, bound when its owner created it (#2571).</summary>
    private readonly EnemyProjectileCueRunner<TSegment, TEvent> _cues;

    /// <summary>Starts <paramref name="animation"/> before its first play, its events carried out by <paramref name="cues"/>.</summary>
    private protected EnemyProjectileTypedAnimation(Animation<TSegment> animation, EnemyProjectileCueRunner<TSegment, TEvent> cues)
        : base(animation) => _cues = Ensure.NotNull(cues);

    /// <inheritdoc/>
    private protected sealed override AnimationShown<TSegment, TFrame> PlayToNextFrame(EnemyProjectileCueContext context,
        EnemyProjectileEngine projectiles, ref Animation<TSegment> animation, ref AnimationPlace place) =>
        Segments.PlayToNextFrame(ref animation, ref place, (raised, pastCue) => _cues.Run(context, projectiles, raised, pastCue));
}
