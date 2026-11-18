using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Reads which of <see cref="TorizoAnimations"/> a body playback plays, which only verification asks.</summary>
internal static class TorizoAnimationsAccess
{
    /// <summary>The members, read as members of <see cref="TorizoAnimations"/>.</summary>
    extension(TorizoAnimations)
    {
        /// <summary>The animation <paramref name="playback"/> plays, found by the animation itself.</summary>
        /// <exception cref="InvalidOperationException">The playback plays none of the body animations.</exception>
        internal static TorizoAnimation Playing(AnimationPlayback<TorizoBodySegment> playback) =>
            TorizoAnimations.StartingAt.Single(pair => ReferenceEquals(pair.Value, playback.Animation)).Key;
    }
}
