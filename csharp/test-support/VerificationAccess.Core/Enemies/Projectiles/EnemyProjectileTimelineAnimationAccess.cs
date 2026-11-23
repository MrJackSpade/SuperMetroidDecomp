using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The playback of a projectile's timeline animation, which its own updates advance (#2570, #2602): checks read and set
/// where the animation stands. The methods serve a family whose animation derives through
/// another generic animation, from which C# infers their type arguments but no extension block's.
/// </summary>
internal static class EnemyProjectileTimelineAnimationAccess
{
    /// <summary>Reads the animation's playback.</summary>
    extension<TSegment, TFrame, TEvent>(EnemyProjectileTimelineAnimation<TSegment, TFrame, TEvent> self)
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull
    {
        /// <summary>The animation playing and where it stands.</summary>
        internal AnimationPlayback<TSegment> Playback => self.PlaybackOf();
    }

    /// <summary>The animation playing and where it stands, for a family whose animation C# infers no block for.</summary>
    internal static AnimationPlayback<TSegment> PlaybackOf<TSegment, TFrame, TEvent>(this EnemyProjectileTimelineAnimation<TSegment, TFrame, TEvent> animation)
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull =>
        animation.Playback;

    /// <summary>Puts the animation at <paramref name="playback"/>, to start a projectile mid-animation.</summary>
    internal static void SetPlayback<TSegment, TFrame, TEvent>(this EnemyProjectileTimelineAnimation<TSegment, TFrame, TEvent> animation,
        AnimationPlayback<TSegment> playback)
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull =>
        animation.Playback = playback;
}
