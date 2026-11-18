using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Plays the Samus projectiles' typed animations (#1301, #1600) as the game's projectile animation plays them: each
/// frame installs its duration, picture and radii.
/// </summary>
internal static class SamusProjectilePlayback
{
    /// <summary>
    /// Plays <paramref name="playback"/> on to its next frame and returns it with its duration, or no duration for a still
    /// picture the projectile keeps from now on (#2347), leaving the playback past it; returns null when the animation
    /// ends, as the game's animation deletes the projectile.
    /// </summary>
    internal static SamusProjectileFrameId? PlayToNextFrame(ref AnimationPlayback<SamusProjectileSegment> playback, out int? duration)
    {
        AnimationPlace place = playback.Place;
        AnimationShown<SamusProjectileSegment, SamusProjectileFrameId> shown =
            SamusProjectileAnimations.Segments.PlayToNextFrame(playback.Animation, ref place, static _ => { });
        playback = playback with { Place = place };
        switch (shown)
        {
            case AnimationShown<SamusProjectileSegment, SamusProjectileFrameId>.Frame frame:
                duration = frame.Duration;
                return frame.Shown;
            case AnimationShown<SamusProjectileSegment, SamusProjectileFrameId>.HeldFrame held:
                duration = null;
                return held.Shown;
            case AnimationShown<SamusProjectileSegment, SamusProjectileFrameId>.Ended:
                duration = null;
                return null;
            default:
                throw new InvalidOperationException($"Samus projectile animation at {BeatOf(playback)} ended a play with {shown}.");
        }
    }

    /// <summary>A playback of the animation starting at <paramref name="segment"/>; no animation is a projectile with no animation.</summary>
    internal static AnimationPlayback<SamusProjectileSegment>? Start(SamusProjectileSegment? segment) =>
        segment is { } selected ? new AnimationPlayback<SamusProjectileSegment>(SamusProjectileAnimations.StartingWith(selected), AnimationPlace.StartOfAnimation) : null;

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
        if (beat.Beat < 0 || beat.Beat > animation.Phases[0].BeatCount)
            throw new InvalidDataException($"Samus projectile segment {beat.Segment} has no place after beat {beat.Beat}.");
        return new(animation, beat.Beat == 0 ? AnimationPlace.StartOfAnimation : new AnimationPlace(0, beat.Beat));
    }

    /// <summary>
    /// The hitbox a freshly selected animation declares through its first step, as the setups' reads of the animation's
    /// radii see it; a projectile with no animation (the safe left-facing Murder Beam) reads zero.
    /// </summary>
    internal static Extent16<RoomPixels> InitialRadius(SamusProjectileSegment? segment)
    {
        if (segment is not { } selected)
            return Extent16<RoomPixels>.Zero;
        AnimationPhase<SamusProjectileSegment> first = SamusProjectileAnimations.StartingWith(selected).Phases[0];
        if (SamusProjectileAnimations.Segments.Beats(first.Segment)[first.FirstBeat].ShownFrame is not { } frame)
            throw new InvalidDataException($"Samus projectile animation {selected} does not open with a frame.");
        return SamusProjectileFrameDefinitions.Radius(frame);
    }

    /// <summary>
    /// The frame a projectile whose animation stands at <paramref name="playback"/> shows: the frame its animation last
    /// showed, or null before its animation has shown one.
    /// </summary>
    internal static SamusProjectileFrameId? ShownFrame(AnimationPlayback<SamusProjectileSegment> playback)
    {
        SamusProjectileSegmentBeat at = BeatOf(playback);
        if (at.Beat == 0)
            return null;
        return SamusProjectileAnimations.Segments.Beats(at.Segment)[at.Beat - 1].ShownFrame
            ?? throw new InvalidOperationException($"Samus projectile segment {at.Segment} continues past a beat that shows no frame.");
    }
}
