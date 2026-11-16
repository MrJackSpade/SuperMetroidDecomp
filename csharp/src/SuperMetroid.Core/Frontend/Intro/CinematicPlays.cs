using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The common shapes of a cinematic family's animations (#2196): a still picture, and one run of a segment.</summary>
internal static class CinematicPlays
{
    /// <summary>The animation of a still picture: <paramref name="animation"/>'s one held frame.</summary>
    internal static Animation<TAnimation> Still<TAnimation>(TAnimation animation)
        where TAnimation : struct => new([new(animation, 0, 1, 1)], AnimationEnd.Holds);

    /// <summary>The animation that plays <paramref name="animation"/>'s <paramref name="beats"/> beats once, then ends as <paramref name="end"/> says.</summary>
    internal static Animation<TAnimation> Through<TAnimation>(TAnimation animation, int beats, AnimationEnd end)
        where TAnimation : struct => new([new(animation, 0, beats, 1)], end);
}
