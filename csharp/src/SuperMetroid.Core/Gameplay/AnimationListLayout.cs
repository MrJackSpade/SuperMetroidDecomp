using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The laid-out continuation of a family's animation lists (#1600), for owners whose gameplay reads which list a resting
/// animation stands in. A list that runs on into the list authored after it, directly after its last beat, continues
/// into that list as a frame read past its end does (<see cref="LaidOutProgramContinuation{TProgram, TStep}"/>): the
/// next list starts that many beats later, so a play resting after the last beat already stands at the next list's
/// entry, as the original game's script pointer then does, and belongs to the next segment.
/// </summary>
internal static class AnimationListLayout
{
    /// <summary>
    /// The continuation in which each of <paramref name="runOnIntoNext"/> runs on into the segment declared after it,
    /// which starts as many beats later as the run-on segment has; no list's operand place is read as a beat.
    /// </summary>
    /// <exception cref="ArgumentException">A run-on segment is the last declared, or has no beats.</exception>
    internal static LaidOutProgramContinuation<TSegment, AnimationBeat<TFrame, TEvent>> RunningOn<TSegment, TFrame, TEvent>(
        AnimationSegments<TSegment, TFrame, TEvent> segments, IReadOnlySet<TSegment> runOnIntoNext)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        Ensure.NotNull(segments);
        Ensure.NotNull(runOnIntoNext);
        TSegment[] authored = Enum.GetValues<TSegment>();
        var successors = new Dictionary<TSegment, LaidOutProgramContinuation<TSegment, AnimationBeat<TFrame, TEvent>>.Successor?>();
        for (int index = 0; index < authored.Length; index++)
        {
            TSegment segment = authored[index];
            if (!runOnIntoNext.Contains(segment))
            {
                successors[segment] = null;
                continue;
            }
            if (index + 1 == authored.Length)
                throw new ArgumentException($"Segment {segment} is declared last, so no list is laid out after it.", nameof(runOnIntoNext));
            successors[segment] = new(authored[index + 1], segments.Beats(segment).Count);
        }
        return new(successors, new Dictionary<TSegment, IReadOnlyDictionary<int, AnimationBeat<TFrame, TEvent>>>());
    }

    /// <summary>
    /// The segment <paramref name="playback"/> rests in: its phase's segment, or, resting after the last beat of a list
    /// that runs on into the next, that next segment. Before its first play, or in an animation of no phase, it rests in the
    /// segment it starts at, among <paramref name="startingAt"/>, as the original game's pointer then rests at that list's
    /// entry.
    /// </summary>
    /// <exception cref="ArgumentException">The playback's segment is not in <paramref name="layout"/>.</exception>
    /// <exception cref="InvalidOperationException">An unstarted playback's animation starts at no segment.</exception>
    internal static TSegment ResidentSegment<TSegment, TFrame, TEvent>(
        LaidOutProgramContinuation<TSegment, AnimationBeat<TFrame, TEvent>> layout,
        IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt, AnimationPlayback<TSegment> playback)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        Ensure.NotNull(layout);
        Ensure.NotNull(startingAt);
        Animation<TSegment> animation = playback.Animation;
        if (animation.Phases.Count == 0 || playback.Place == AnimationPlace.StartOfAnimation)
            return startingAt.Single(pair => ReferenceEquals(pair.Value, animation)).Key;
        AnimationPhase<TSegment> phase = animation.Phases[playback.Place.Phase];
        // A hold rests after the phase's last beat, as the end of its play does.
        int next = phase.FirstBeat + Math.Clamp(playback.Place.Beat, 0, phase.BeatCount);
        return layout.ReadPast(phase.Segment, next) is { } read ? read.Program : phase.Segment;
    }
}
