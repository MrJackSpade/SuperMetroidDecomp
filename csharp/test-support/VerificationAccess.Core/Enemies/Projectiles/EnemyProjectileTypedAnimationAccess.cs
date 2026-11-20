using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The playback of a projectile's typed animation, which production keeps private and its families set (#2570): checks
/// read where the animation stands through <see cref="PrivateState"/>.
/// </summary>
internal static class EnemyProjectileTypedAnimationAccess
{
    /// <summary>Reads the animation's playback.</summary>
    extension<TSegment, TFrame, TEvent>(EnemyProjectileTypedAnimation<TSegment, TFrame, TEvent> self)
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull
    {
        /// <summary>The animation playing and where it stands; a check sets it to start a projectile mid-animation.</summary>
        internal AnimationPlayback<TSegment> Playback
        {
            get => PrivateState.Get<EnemyProjectileTypedAnimation<TSegment, TFrame, TEvent>, AnimationPlayback<TSegment>>(self, "Playback");
            set => PrivateState.Set(self, "Playback", value);
        }
    }
}
