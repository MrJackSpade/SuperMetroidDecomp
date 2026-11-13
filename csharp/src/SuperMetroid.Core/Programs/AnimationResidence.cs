namespace SuperMetroid.Core.Programs;

/// <summary>
/// Which segment a resting animation stands in (#1600), for owners whose gameplay compares it. A segment that runs
/// on into the segment authored after it, directly after its last beat, hands its play to that segment there, so a play resting
/// after its last beat already stands in the next segment, as the game's animation then rests at its entry.
/// </summary>
internal static class AnimationResidence
{
    /// <summary>
    /// The segment <paramref name="playback"/> rests in: its playing phase's segment, or, resting after the last beat of a
    /// segment that runs on into another, that other segment. Before its first play, or in an animation of no phase, it
    /// rests in the segment it starts at, among <paramref name="startingAt"/>.
    /// </summary>
    /// <param name="segments">The family's segments.</param>
    /// <param name="startingAt">The family's animations, by the segment each starts at.</param>
    /// <param name="runsOnInto">The segment each run-on segment continues into, directly after its last beat.</param>
    /// <param name="playback">The playing animation and where it stands.</param>
    /// <exception cref="InvalidOperationException">An unstarted playback's animation starts at no segment.</exception>
    internal static TSegment ResidentSegment<TSegment, TFrame, TEvent>(
        AnimationSegments<TSegment, TFrame, TEvent> segments,
        IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt,
        IReadOnlyDictionary<TSegment, TSegment> runsOnInto,
        AnimationPlayback<TSegment> playback)
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull
    {
        Ensure.NotNull(segments);
        Ensure.NotNull(startingAt);
        Ensure.NotNull(runsOnInto);
        Animation<TSegment> animation = Ensure.NotNull(playback.Animation);
        if (animation.Phases.Count == 0 || playback.Place == AnimationPlace.StartOfAnimation)
            return startingAt.Single(pair => ReferenceEquals(pair.Value, animation)).Key;
        AnimationPhase<TSegment> phase = animation.Phases[playback.Place.Phase];
        // A hold rests after the phase's last beat, as the end of its play does.
        bool pastListEnd = playback.Place.Beat >= phase.BeatCount
            && phase.FirstBeat + phase.BeatCount == segments.Beats(phase.Segment).Count;
        return pastListEnd && runsOnInto.TryGetValue(phase.Segment, out TSegment next) ? next : phase.Segment;
    }
}
