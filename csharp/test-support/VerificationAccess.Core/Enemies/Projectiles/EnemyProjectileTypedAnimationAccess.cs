using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The playback of a projectile's typed animation, which production keeps private and its families set (#2570): checks
/// read and set where the animation stands through <see cref="PrivateState"/>. The methods serve a family whose animation
/// derives through another generic animation, from which C# infers their type arguments but no extension block's.
/// </summary>
internal static class EnemyProjectileTypedAnimationAccess
{
    /// <summary>Reads the animation's playback.</summary>
    extension<TSegment, TFrame, TEvent>(EnemyProjectileTypedAnimation<TSegment, TFrame, TEvent> self)
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull
    {
        /// <summary>The animation playing and where it stands.</summary>
        internal AnimationPlayback<TSegment> Playback => self.PlaybackOf();
    }

    /// <summary>The animation playing and where it stands, for a family whose animation C# infers no block for.</summary>
    internal static AnimationPlayback<TSegment> PlaybackOf<TSegment, TFrame, TEvent>(this EnemyProjectileTypedAnimation<TSegment, TFrame, TEvent> animation)
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull =>
        PrivateState.Get<EnemyProjectileTypedAnimation<TSegment, TFrame, TEvent>, AnimationPlayback<TSegment>>(animation, "Playback");

    /// <summary>Puts the animation at <paramref name="playback"/>, to start a projectile mid-animation.</summary>
    internal static void SetPlayback<TSegment, TFrame, TEvent>(this EnemyProjectileTypedAnimation<TSegment, TFrame, TEvent> animation,
        AnimationPlayback<TSegment> playback)
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull =>
        PrivateState.Set(animation, "Playback", playback);
}
