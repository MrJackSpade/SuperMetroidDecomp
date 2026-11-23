using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Areas.Tourian.TourianStatueBlockSegment;
using F = SuperMetroid.Core.Fx.Artwork.TourianStatueBlockPicture;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The typed animations one Tourian boss statue's animated pictures play (#1600, #2424).
/// All four statues share the layout; only the first idle frame's duration differs, and every other value a cue acts on is read
/// from the statue's <see cref="TourianStatueAnimationDefinition"/> when an event is reached. Each statue builds its own, and
/// each of its animations plays every beat of the segments it names.
/// </summary>
internal sealed partial class TourianStatueBlockAnimations
{
    /// <summary>Builds one statue's animations, whose first idle frame lasts <paramref name="firstFrameDuration"/> updates.</summary>
    internal TourianStatueBlockAnimations(int firstFrameDuration)
    {
        Segments = Layout(firstFrameDuration);
        StartingAt = new Dictionary<P, Animation<P>>
        {
            [P.Entry] = Animation<P>.LeadingInto([Segments.Whole(P.Entry)], [Segments.Whole(P.Oscillate)]),
            [P.Oscillate] = Animation<P>.Looping([Segments.Whole(P.Oscillate)]),
            [P.Release] = new([Segments.Whole(P.Release)], AnimationEnd.Stops),
            [P.AlreadyGrey] = new([Segments.Whole(P.AlreadyGrey)], AnimationEnd.Stops),
        };
    }

    /// <summary>The statue's segments.</summary>
    internal AnimationSegments<P, F, TourianStatueBlockCue> Segments { get; }

    /// <summary>
    /// The animation that starts at each segment: the entry runs on into the oscillation, which loops; a release and the
    /// already-grey finish end the statue's animation, ending its picture animation.
    /// </summary>
    internal IReadOnlyDictionary<P, Animation<P>> StartingAt { get; }

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal Animation<P> StartingWith(P segment) => StartingAt[segment];

    /// <summary>The statue's segments, whose first idle frame lasts <paramref name="firstFrameDuration"/> updates.</summary>
    private static AnimationSegments<P, F, TourianStatueBlockCue> Layout(int firstFrameDuration) => new(
        new Dictionary<P, AnimationBeat<F, TourianStatueBlockCue>[]>
        {
            [P.Entry] =
            [
                Cue(TourianStatueBlockCue.MarkAnimating),
                Cue(TourianStatueBlockCue.MayAlreadyBeGrey),
                Show(F.EyesBright, firstFrameDuration),
            ],
            [P.Oscillate] =
            [
                Show(F.EyesDim, 12),
                Show(F.EyesDark, 12),
                Show(F.EyesDim, 12),
                Show(F.EyesBright, 16),
                Cue(TourianStatueBlockCue.MayBeReleased),
                Cue(TourianStatueBlockCue.ClearAnimating),
            ],
            [P.Release] =
            [
                Cue(TourianStatueBlockCue.MayWaitItsTurn),
                Cue(TourianStatueBlockCue.ClaimRelease),
                Cue(TourianStatueBlockCue.BlackOutEyes),
                Show(F.EyesDim, 16),
                Show(F.EyesDark, 16),
                Cue(TourianStatueBlockCue.SpawnEyeGlow),
                Show(F.Released, 192),
                Cue(TourianStatueBlockCue.SpawnSoul),
                Cue(TourianStatueBlockCue.SpawnGreyColourEffect),
                Show(F.Released, 128),
                Cue(TourianStatueBlockCue.SetGreyEvent),
                Cue(TourianStatueBlockCue.FinishRelease),
            ],
            [P.AlreadyGrey] =
            [
                Cue(TourianStatueBlockCue.FinishRelease),
                Cue(TourianStatueBlockCue.TurnGrey),
            ],
        });

    /// <summary>Shows <paramref name="picture"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<F, TourianStatueBlockCue> Show(F picture, int duration) =>
        new AnimationBeat<F, TourianStatueBlockCue>.Frame(picture, duration);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<F, TourianStatueBlockCue> Cue(TourianStatueBlockCue cue) =>
        new AnimationBeat<F, TourianStatueBlockCue>.Event(cue);
}
