using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Plays the Samus projectiles' typed animations (#1301, #1600, #2458) through the shared animation player, as the game's
/// projectile animation plays them: each frame shows for its duration, a still picture is kept for good (#2347), and an
/// animation that ends deletes the projectile.
/// </summary>
internal static class SamusProjectilePlayback
{
    /// <summary>
    /// A player of the animation starting at <paramref name="segment"/>, showing nothing until its first update shows its
    /// first frame; no animation is a projectile with no animation.
    /// </summary>
    internal static AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue>? Start(SamusProjectileSegment? segment) =>
        segment is { } selected
            ? AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue>.Starting(
                SamusProjectileAnimations.Segments, SamusProjectileAnimations.StartingWith(selected))
            : null;

    /// <summary>
    /// Runs one update of a projectile's <paramref name="animation"/> and returns the frame the update shows anew: the next
    /// frame once the shown one has held its updates, or null while it holds, while a still picture is kept and on the
    /// update the animation ends (<see cref="AnimationPlayer{TSegment, TFrame, TEvent}.HasStopped"/>).
    /// </summary>
    internal static SamusProjectileFrameId? StepToNewFrame(AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue> animation) =>
        Ensure.NotNull(animation).StepToNewFrame();

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
}
