namespace SuperMetroid.Core.Animation;

/// <summary>An owner's playing animation (#1600): the animation, and where it stands in it.</summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <param name="Animation">The animation playing.</param>
/// <param name="Place">Where it stands.</param>
internal readonly record struct AnimationPlayback<TSegment>(Animation<TSegment> Animation, AnimationPlace Place)
    where TSegment : struct
{
    /// <summary>The segment the playing phase plays; before the first play, the first phase's (#1886).</summary>
    /// <exception cref="InvalidOperationException">The animation has no phase.</exception>
    internal TSegment PlayingSegment => Animation.Phases.Count > Place.Phase
        ? Animation.Phases[Place.Phase].Segment
        : throw new InvalidOperationException("An animation of no phase plays no segment.");

    /// <summary>
    /// Whether the play has shown the playing phase's last beat and rests after it (#1886): on its last frame until it plays
    /// on, or where the animation holds for good.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation has no phase.</exception>
    internal bool HasShownPhaseEnd => Animation.Phases.Count > Place.Phase
        ? Place.Beat >= Animation.Phases[Place.Phase].BeatCount
        : throw new InvalidOperationException("An animation of no phase has no phase to finish.");
}
