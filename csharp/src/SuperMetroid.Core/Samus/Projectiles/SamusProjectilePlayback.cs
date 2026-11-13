using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Plays the Samus projectiles' typed animations (#1301, #1600) as the projectile animation runs their lists: each
/// timed record installs its duration, frame and radii.
/// </summary>
internal static class SamusProjectilePlayback
{
    /// <summary>
    /// Plays <paramref name="playback"/> on to the next timed record and returns it with its duration, leaving the
    /// playback past it; returns null when the animation ends, as the list deletes the projectile.
    /// </summary>
    internal static SamusProjectileFrame? RunToNextRecord(ref AnimationPlayback<SamusProjectileSegment> playback, out int duration)
    {
        AnimationPlace place = playback.Place;
        // The projectiles' animations count no repeats.
        WrappingCountdown repeats = default;
        AnimationShown<SamusProjectileSegment, SamusProjectileFrame> shown =
            SamusProjectileAnimations.Segments.PlayToNextFrame(playback.Animation, ref place, ref repeats, static _ => { });
        playback = playback with { Place = place };
        switch (shown)
        {
            case AnimationShown<SamusProjectileSegment, SamusProjectileFrame>.Frame frame:
                duration = frame.Duration;
                return frame.Shown;
            case AnimationShown<SamusProjectileSegment, SamusProjectileFrame>.Ended:
                duration = 0;
                return null;
            default:
                throw new InvalidOperationException($"Samus projectile animation at {BeatOf(playback)} ended a play with {shown}.");
        }
    }

    /// <summary>A playback of the animation starting at <paramref name="program"/>; no program is a projectile with no list.</summary>
    internal static AnimationPlayback<SamusProjectileSegment>? Start(SamusProjectileSegment? program) =>
        program is { } selected ? new AnimationPlayback<SamusProjectileSegment>(SamusProjectileAnimations.StartingWith(selected), AnimationPlace.StartOfAnimation) : null;

    /// <summary>
    /// Where <paramref name="playback"/> stands: before its first play, at the start of the segment its animation starts
    /// at; otherwise in its playing phase's segment, after the beats of it played.
    /// </summary>
    internal static SamusProjectileSegmentBeat BeatOf(AnimationPlayback<SamusProjectileSegment> playback)
    {
        if (playback.Place == AnimationPlace.StartOfAnimation)
            return new(SamusProjectileAnimations.StartingAt.Single(pair => ReferenceEquals(pair.Value, playback.Animation)).Key, 0);
        AnimationPhase<SamusProjectileSegment> phase = playback.Animation.Phases[playback.Place.Phase];
        return new(phase.Segment, phase.FirstBeat + Math.Clamp(playback.Place.Beat, 0, phase.BeatCount));
    }

    /// <summary>
    /// A playback standing at <paramref name="beat"/>: in the animation starting at its segment, which plays the whole
    /// segment first, before its first play at the segment's start.
    /// </summary>
    /// <exception cref="InvalidDataException">The place is past the segment's end.</exception>
    internal static AnimationPlayback<SamusProjectileSegment> PlaybackAt(SamusProjectileSegmentBeat beat)
    {
        Animation<SamusProjectileSegment> animation = SamusProjectileAnimations.StartingWith(beat.Segment);
        if ((uint)beat.Beat > (uint)animation.Phases[0].BeatCount)
            throw new InvalidDataException($"Samus projectile segment {beat.Segment} has no place after beat {beat.Beat}.");
        return new(animation, beat.Beat == 0 ? AnimationPlace.StartOfAnimation : new AnimationPlace(0, beat.Beat));
    }

    /// <summary>
    /// The hitbox a freshly selected list declares through its first record, as the initializers' reads of the list's
    /// radii see it; a projectile with no list (the safe left-facing Murder Beam) reads zero.
    /// </summary>
    internal static Extent16<RoomPixels> InitialRadius(SamusProjectileSegment? program)
    {
        if (program is not { } selected)
            return Extent16<RoomPixels>.Zero;
        AnimationPhase<SamusProjectileSegment> first = SamusProjectileAnimations.StartingWith(selected).Phases[0];
        if (SamusProjectileAnimations.Segments.Beats(first.Segment)[first.FirstBeat] is not AnimationBeat<SamusProjectileFrame, NoAnimationCue>.Frame frame)
            throw new InvalidDataException($"Samus projectile animation {selected} does not open with a timed record.");
        SamusProjectileRecord record = SamusProjectileRecordDefinitions.Get(frame.Shown);
        return record.Radius;
    }

    /// <summary>
    /// The animation frame of the record a projectile whose animation stands at <paramref name="playback"/> shows: the
    /// record its animation last installed, or null before its list has installed one.
    /// </summary>
    internal static int? ShownAnimationFrame(AnimationPlayback<SamusProjectileSegment> playback)
    {
        SamusProjectileSegmentBeat at = BeatOf(playback);
        if (at.Beat == 0)
            return null;
        if (SamusProjectileAnimations.Segments.Beats(at.Segment)[at.Beat - 1] is not AnimationBeat<SamusProjectileFrame, NoAnimationCue>.Frame frame)
            throw new InvalidOperationException($"Samus projectile segment {at.Segment} continues past a beat that is no record.");
        return SamusProjectileRecordDefinitions.Get(frame.Shown).AnimationFrame;
    }
}
