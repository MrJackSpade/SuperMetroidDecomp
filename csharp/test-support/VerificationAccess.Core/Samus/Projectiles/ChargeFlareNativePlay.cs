using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Compatibility;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A component's play read and set as the original's picture number and wait.</summary>
/// <typeparam name="TSegment">The component's runs of pictures.</typeparam>
/// <typeparam name="TPicture">The component's pictures.</typeparam>
internal static class ChargeFlareNativePlay<TSegment, TPicture>
    where TSegment : struct
    where TPicture : struct
{
    /// <summary>The ordinary play's player.</summary>
    private static AnimationPlayer<TSegment, TPicture, NoAnimationCue> Player(ChargeFlarePlay<TSegment, TPicture> play) => play.Player;

    /// <summary>The ordinary play's animation.</summary>
    private static Animation<TSegment> Animation(ChargeFlarePlay<TSegment, TPicture> play) => play.Looping.Animation;

    /// <summary>The number of the picture the ordinary play shows.</summary>
    internal static int? Picture(ChargeFlarePlay<TSegment, TPicture> play) => PictureOf(Player(play));

    /// <summary>The number of the picture the Hyper Beam's count shows, or null once it has stepped back past the first.</summary>
    internal static int? Picture(HyperBeamFlareCount<TSegment, TPicture> count) =>
        PictureOf(count.Player);

    /// <summary>
    /// The ordinary play's wait as the original keeps it: the updates its picture still shows less one, so the update that
    /// takes it below zero shows the next picture.
    /// </summary>
    internal static WrappingCountdown Wait(ChargeFlarePlay<TSegment, TPicture> play) => WrappingCountdown.Start(Player(play).UpdatesLeft - 1);

    /// <summary>
    /// Shows the ordinary play's picture numbered <paramref name="number"/> with the original's wait <paramref name="wait"/>,
    /// which the ordinary flare never leaves run past zero.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The component has no picture of that number, or the wait has run past zero.</exception>
    internal static void ShowNative(ChargeFlarePlay<TSegment, TPicture> play, int number, WrappingCountdown wait)
    {
        Animation<TSegment> animation = Animation(play);
        Player(play).ShowAt(animation, PlaceOfPicture(animation, number),
            wait.IsNegative
                ? throw new ArgumentOutOfRangeException(nameof(wait), wait, "The ordinary flare never leaves a wait run past zero.")
                : wait.RemainingFrames + 1);
    }

    /// <summary>The updates each picture of <paramref name="animation"/> shows for, its phases in order.</summary>
    internal static IReadOnlyList<int> Durations(AnimationSegments<TSegment, TPicture, NoAnimationCue> segments, LoopingAnimation<TSegment> animation) =>
        [.. segments.PlayedBeats(animation.Animation).Select(beat => beat is AnimationBeat<TPicture, NoAnimationCue>.Frame frame
            ? frame.Duration
            : throw new InvalidOperationException($"Charge-flare beat {beat} shows no picture."))];

    /// <summary>The pictures <paramref name="animation"/>'s loop repeats, by number: from the first it shows to one past the last.</summary>
    internal static (int From, int End) LoopedPictures(Animation<TSegment> animation)
    {
        var loop = (AnimationEnd.Loop)animation.End;
        AnimationPhase<TSegment> repeated = animation.Phases[loop.Phase];
        int from = animation.Phases.Take(loop.Phase).Sum(phase => phase.BeatCount * phase.Plays) + repeated.FirstBeat;
        return (from, from + repeated.BeatCount * repeated.Plays);
    }

    /// <summary>The number of the picture <paramref name="player"/> shows, or null when it shows none.</summary>
    private static int? PictureOf(AnimationPlayer<TSegment, TPicture, NoAnimationCue> player)
    {
        if (player.Shown is null)
            return null;
        // The place stands just past the beat shown.
        AnimationPlace place = player.Playback.Place;
        IReadOnlyList<AnimationPhase<TSegment>> phases = player.Playback.Animation.Phases;
        AnimationPhase<TSegment> playing = phases[place.Phase];
        int playsBefore = playing.IsRepeated ? playing.Plays - place.PlaysLeft : 0;
        return phases.Take(place.Phase).Sum(phase => phase.BeatCount * phase.Plays) + playsBefore * playing.BeatCount + place.Beat - 1;
    }

    /// <summary>The place of <paramref name="animation"/> whose next beat shows its picture numbered <paramref name="number"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The animation has no picture of that number.</exception>
    private static AnimationPlace PlaceOfPicture(Animation<TSegment> animation, int number)
    {
        IReadOnlyList<AnimationPhase<TSegment>> phases = animation.Phases;
        int left = number;
        for (int phase = 0; phase < phases.Count; phase++)
        {
            // A repeated phase's pictures are numbered through each of its plays in turn.
            AnimationPhase<TSegment> numbered = phases[phase];
            if (left >= 0 && left < numbered.BeatCount * numbered.Plays)
                return new AnimationPlace(phase, left % numbered.BeatCount)
                    .WithPlaysLeft(numbered.IsRepeated ? numbered.Plays - left / numbered.BeatCount : 0);
            left -= numbered.BeatCount * numbered.Plays;
        }
        throw new ArgumentOutOfRangeException(nameof(number), number, $"The charge flare has no picture {number}.");
    }
}
