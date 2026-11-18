using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Verification's native view of a trail side: the bank-$90 address of the beat its animation plays next, or zero for a
/// side with no animation, as native trail RAM holds it (#2348), and the timer the native side counts its frame with. The
/// native program stores each repeated run written out (#2458), so an address names a beat counted through every play.
/// </summary>
internal static class SamusProjectileTrailSideNative
{
    /// <summary>The pointer native trail RAM's cleared zero names: no animation.</summary>
    private const ushort Cleared = 0;

    /// <summary>
    /// The program and the beat, counted through every play of its runs, stored at bank-$90 address
    /// <paramref name="address"/>; the beat count names the program's end word.
    /// </summary>
    private static (ProjectileTrailSegment Program, int Beat) BeatAt(ushort address)
    {
        foreach (ProjectileTrailSegment program in Enum.GetValues<ProjectileTrailSegment>())
        {
            for (int beat = 0; beat <= ProjectileTrailSparkleAnimations.PlayedBeats(program).Count; beat++)
            {
                if (ProjectileTrailSources.Address(program, beat) == address)
                    return (program, beat);
            }
        }
        throw new ArgumentOutOfRangeException(nameof(address), address, "No trail beat is stored there.");
    }

    /// <summary>
    /// The place of <paramref name="animation"/> whose next beat is <paramref name="beat"/>, counted through every play of
    /// its phases; the start of the animation for its first, and the end of its last phase for its beat count.
    /// </summary>
    private static AnimationPlace PlaceOf(Animation<ProjectileTrailRun> animation, int beat)
    {
        if (beat == 0)
            return AnimationPlace.StartOfAnimation;
        int left = beat;
        for (int phase = 0; phase < animation.Phases.Count; phase++)
        {
            AnimationPhase<ProjectileTrailRun> playing = animation.Phases[phase];
            int span = playing.BeatCount * playing.Plays;
            bool last = phase == animation.Phases.Count - 1;
            if (left < span || (last && left == span))
            {
                int play = Math.Min(left / playing.BeatCount, playing.Plays - 1);
                return new AnimationPlace(phase, left - play * playing.BeatCount)
                    .WithPlaysLeft(playing.IsRepeated ? playing.Plays - play : 0);
            }
            left -= span;
        }
        throw new ArgumentOutOfRangeException(nameof(beat), beat, "The animation has no such beat.");
    }

    /// <summary>The beat, counted through every play of its phases, that <paramref name="playback"/> plays next.</summary>
    private static int BeatOf(AnimationPlayback<ProjectileTrailRun> playback)
    {
        AnimationPlace place = playback.Place;
        IReadOnlyList<AnimationPhase<ProjectileTrailRun>> phases = playback.Animation.Phases;
        int before = phases.Take(place.Phase).Sum(phase => phase.BeatCount * phase.Plays);
        if (place.Beat == AnimationPlace.NotEntered)
            return before;
        AnimationPhase<ProjectileTrailRun> playing = phases[place.Phase];
        int playsBefore = playing.IsRepeated ? playing.Plays - place.PlaysLeft : 0;
        return before + playsBefore * playing.BeatCount + place.Beat;
    }

    /// <summary>The program whose animation <paramref name="playback"/> plays.</summary>
    private static ProjectileTrailSegment ProgramOf(AnimationPlayback<ProjectileTrailRun> playback) =>
        ProjectileTrailSparkleAnimations.StartingAt.Single(pair => ReferenceEquals(pair.Value, playback.Animation)).Key;

    /// <summary>The bank-$90 address of the beat <paramref name="playback"/> plays next, or of its program's end word.</summary>
    internal static ushort Address(AnimationPlayback<ProjectileTrailRun> playback) =>
        ProjectileTrailSources.Address(ProgramOf(playback), BeatOf(playback));

    /// <summary>A side's native pointer and timer.</summary>
    extension(SamusProjectileTrailSide side)
    {
        /// <summary>The bank-$90 address of the beat the side plays next, as native trail RAM holds it; zero once it has ended.</summary>
        internal int InstructionPointer => side.Animation is { HasStopped: false } animation ? Address(animation.Playback) : Cleared;

        /// <summary>The native timer: the updates the side's frame has left, one before its first frame; zero once it has ended.</summary>
        internal int FrameHold => side.Animation is { HasStopped: false } animation ? animation.UpdatesLeft : 0;

        /// <summary>
        /// Restores a native side: one whose timer has run out has ended; otherwise it plays on from the beat at
        /// <paramref name="instructionPointer"/>, showing <paramref name="shownPose"/>, the frame the native program stores
        /// just before that beat, for the <paramref name="frameHold"/> updates its timer has left, or, before its first
        /// frame, nothing until its first update.
        /// </summary>
        /// <exception cref="InvalidDataException">The shown pose is not the frame stored before the pointer.</exception>
        internal void RestoreNative(int instructionPointer, int frameHold, ProjectileTrailFrame? shownPose)
        {
            if (frameHold == 0)
            {
                side.Animation = null;
                return;
            }
            (ProjectileTrailSegment program, int beat) = BeatAt(checked((ushort)instructionPointer));
            Animation<ProjectileTrailRun> animation = ProjectileTrailSparkleAnimations.StartingAt[program];
            if (shownPose is not { } pose)
            {
                if (beat != 0)
                    throw new InvalidDataException($"A trail side at {program} beat {beat} shows no pose.");
                side.Animation = AnimationPlayer<ProjectileTrailRun, ProjectileTrailFrame, ProjectileTrailCue>.StartingAfter(
                    ProjectileTrailSparkleAnimations.Segments, animation, frameHold);
                return;
            }
            if (beat == 0 || ProjectileTrailSparkleAnimations.PlayedBeats(program)[beat - 1].ShownFrame != pose)
                throw new InvalidDataException($"A trail side at {program} beat {beat} cannot show {pose}.");
            var restored = AnimationPlayer<ProjectileTrailRun, ProjectileTrailFrame, ProjectileTrailCue>.Starting(
                ProjectileTrailSparkleAnimations.Segments, animation);
            restored.ShowAt(animation, PlaceOf(animation, beat - 1), frameHold);
            side.Animation = restored;
        }
    }
}
