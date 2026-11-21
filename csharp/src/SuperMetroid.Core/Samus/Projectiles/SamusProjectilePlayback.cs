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
    /// Plays a shot's or a bomb's <paramref name="animation"/> one update on: each frame it shows anew publishes its picture
    /// and radius through <paramref name="show"/>, a projectile showing a still picture keeps it, and an animation that
    /// ends deletes its projectile through <paramref name="delete"/>.
    /// </summary>
    /// <param name="animation">The projectile's animation.</param>
    /// <param name="frames">The installed pictures of the projectiles' frames.</param>
    /// <param name="show">Publishes the picture and the radius of the frame shown anew.</param>
    /// <param name="delete">Deletes the projectile.</param>
    internal static void PlayOn(
        AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue> animation,
        InstalledProjectileFrameBindings frames,
        Action<ProjectileSpriteId, Extent16<RoomPixels>> show,
        Action delete)
    {
        Ensure.NotNull(frames);
        Ensure.NotNull(show);
        Ensure.NotNull(delete);
        if (StepToNewFrame(animation) is { } frame)
            show(frames.Resolve(frame), SamusProjectileFrameDefinitions.Radius(frame));
        else if (animation.HasStopped)
            delete();
    }

    /// <summary>
    /// Where <paramref name="playback"/> stands: before its first play, at the start of the segment its animation starts
    /// at; otherwise in its playing phase's segment, after the beats of it played.
    /// </summary>
    internal static SamusProjectileSegmentBeat BeatOf(AnimationPlayback<SamusProjectileSegment> playback)
    {
        if (playback.Place == AnimationPlace.StartOfAnimation)
            return new(playback.Animation.Phases[0].Segment, 0);
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
    internal static Extent16<RoomPixels> InitialRadius(SamusProjectileSegment? segment) =>
        segment is { } selected ? OpeningRadii[selected] : Extent16<RoomPixels>.Zero;

    /// <summary>
    /// The hitbox each animation declares through its first step, read once from every animation as they are built, so an
    /// animation that does not open with a frame fails there and no selection checks again (#2553).
    /// </summary>
    private static Dictionary<SamusProjectileSegment, Extent16<RoomPixels>> OpeningRadii { get; } =
        SamusProjectileAnimations.StartingAt.ToDictionary(entry => entry.Key, entry => OpeningRadius(entry.Key, entry.Value));

    /// <summary>The hitbox <paramref name="animation"/>, the one starting at <paramref name="segment"/>, declares through its first step.</summary>
    /// <exception cref="InvalidDataException">The animation does not open with a frame.</exception>
    private static Extent16<RoomPixels> OpeningRadius(SamusProjectileSegment segment, Animation<SamusProjectileSegment> animation)
    {
        AnimationPhase<SamusProjectileSegment> first = animation.Phases[0];
        if (SamusProjectileAnimations.Segments.Beats(first.Segment)[first.FirstBeat].ShownFrame is not { } frame)
            throw new InvalidDataException($"Samus projectile animation {segment} does not open with a frame.");
        return SamusProjectileFrameDefinitions.Radius(frame);
    }
}
