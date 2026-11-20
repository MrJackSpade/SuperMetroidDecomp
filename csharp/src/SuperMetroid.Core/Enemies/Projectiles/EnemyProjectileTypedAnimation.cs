using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile animation whose typed timeline raises events (#1600, #1760): each event goes to the cue runner its owner
/// bound it to when it created it (#2571, <see cref="EnemyProjectileCueRunner{TSegment, TEvent}"/>): the family's cue owner,
/// the behaviour that started it (#2554), which may continue the play elsewhere, as a conditional change to another
/// animation or a conditional replay of the cue's run does, or hand the projectile over to another family's animation; or
/// the engine for the shared <see cref="EnemyProjectileCommonCue"/> events.
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
    /// <param name="animation">The animation.</param>
    /// <param name="cues">Who carries out the animation's events.</param>
    private protected EnemyProjectileTypedAnimation(Animation<TSegment> animation, EnemyProjectileCueRunner<TSegment, TEvent> cues)
        : base(animation) => _cues = Ensure.NotNull(cues);

    /// <summary>Starts <paramref name="animation"/> before its first play, its cues carried out by <paramref name="cues"/>.</summary>
    /// <param name="animation">The animation.</param>
    /// <param name="cues">The behaviour that owns the animation's cues.</param>
    private protected EnemyProjectileTypedAnimation(Animation<TSegment> animation, IEnemyProjectileCues<TEvent> cues)
        : this(animation, EnemyProjectileCueRunner<TSegment, TEvent>.By(cues))
    {
    }

    /// <summary>
    /// Starts <paramref name="animation"/> before its first play, its cues carried out by <paramref name="cues"/>, whose
    /// reactions may continue the play elsewhere.
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="cues">The behaviour that owns the animation's cues.</param>
    private protected EnemyProjectileTypedAnimation(Animation<TSegment> animation, IEnemyProjectileBranchingCues<TSegment, TEvent> cues)
        : this(animation, EnemyProjectileCueRunner<TSegment, TEvent>.By(cues))
    {
    }

    /// <summary>
    /// Starts <paramref name="animation"/> before its first play, its cues carried out by <paramref name="cues"/>, whose
    /// reactions may hand the play over to another family's animation: the animation's own play is then over, and the
    /// projectile plays on in the animation handed to within the same pass (#2554).
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="cues">The behaviour that owns the animation's cues.</param>
    private protected EnemyProjectileTypedAnimation(Animation<TSegment> animation, IEnemyProjectileHandingOverCues<TEvent> cues)
        : this(animation, EnemyProjectileCueRunner<TSegment, TEvent>.HandingOverBy(cues))
    {
    }

    /// <inheritdoc/>
    private protected sealed override AnimationShown<TSegment, TFrame> PlayToNextFrame(EnemyProjectileCueContext context,
        ref Animation<TSegment> animation, ref AnimationPlace place) =>
        _cues.PlayToNextFrame(Segments, context, ref animation, ref place);

    /// <inheritdoc/>
    private protected sealed override EnemyProjectileAnimation? HandedOverTo => _cues.HandedOverTo;
}
