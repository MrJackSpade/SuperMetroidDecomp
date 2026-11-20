using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile animation whose beats show the family's projectile frames outright (#1623), so each frame it reaches is
/// the one it presents.
/// </summary>
/// <typeparam name="TSegment">The family's segment identity.</typeparam>
/// <typeparam name="TEvent">The family's events.</typeparam>
internal abstract class EnemyProjectileFrameAnimation<TSegment, TEvent> : EnemyProjectileTypedAnimation<TSegment, EnemyProjectileFrameId, TEvent>
    where TSegment : struct
    where TEvent : notnull
{
    /// <summary>Starts <paramref name="animation"/> before its first play, its cues carried out by <paramref name="cues"/>.</summary>
    /// <param name="animation">The animation.</param>
    /// <param name="cues">The behaviour that owns the animation's cues.</param>
    private protected EnemyProjectileFrameAnimation(Animation<TSegment> animation, IEnemyProjectileCues<TEvent> cues)
        : base(animation, cues)
    {
    }

    /// <summary>
    /// Starts <paramref name="animation"/> before its first play, its cues carried out by <paramref name="cues"/>, whose
    /// reactions may continue the play elsewhere.
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="cues">The behaviour that owns the animation's cues.</param>
    private protected EnemyProjectileFrameAnimation(Animation<TSegment> animation, IEnemyProjectileBranchingCues<TSegment, TEvent> cues)
        : base(animation, cues)
    {
    }

    /// <summary>Starts <paramref name="animation"/> before its first play, its cues carried out by <paramref name="runCue"/>.</summary>
    /// <param name="animation">The animation.</param>
    /// <param name="runCue">Carries out each of the animation's cues.</param>
    private protected EnemyProjectileFrameAnimation(Animation<TSegment> animation, CueRunner runCue)
        : base(animation, runCue)
    {
    }

    /// <inheritdoc/>
    private protected sealed override EnemyProjectileFrameId Present(EnemyProjectileFrameId frame) => frame;
}
