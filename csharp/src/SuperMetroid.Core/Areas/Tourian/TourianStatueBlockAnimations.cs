using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Areas.Tourian.TourianStatueBlockCueKind;
using P = SuperMetroid.Core.Areas.Tourian.TourianStatueBlockSegment;
using F = SuperMetroid.Core.Fx.Artwork.TourianStatueBlockPicture;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The typed animations a Tourian boss statue's animated pictures play (#1600).
/// All four statues share the layout; only the first idle frame's duration differs, and every other value a cue acts on is read
/// from the statue's <see cref="TourianStatueAnimationDefinition"/> when an event is reached.
/// </summary>
internal static class TourianStatueBlockAnimations
{
    /// <summary>Builds one statue's segments, whose first idle frame lasts <paramref name="firstFrameDuration"/> updates.</summary>
    internal static AnimationSegments<P, F, TourianStatueBlockCue> SegmentsFor(int firstFrameDuration) => new(
        new Dictionary<P, AnimationBeat<F, TourianStatueBlockCue>[]>
        {
            [P.Entry] =
            [
                Cue(new(K.MarkAnimating)),
                Cue(new(K.MayAlreadyBeGrey)),
                Show(F.EyesBright, firstFrameDuration),
            ],
            [P.Oscillate] =
            [
                Show(F.EyesDim, 12),
                Show(F.EyesDark, 12),
                Show(F.EyesDim, 12),
                Show(F.EyesBright, 16),
                Cue(new(K.MayBeReleased)),
                Cue(new(K.ClearAnimating)),
            ],
            [P.Release] =
            [
                Cue(new(K.MayWaitItsTurn)),
                Cue(new(K.ClaimRelease)),
                Cue(new(K.BlackOutEyes)),
                Show(F.EyesDim, 16),
                Show(F.EyesDark, 16),
                Cue(new(K.SpawnEyeGlow)),
                Show(F.Released, 192),
                Cue(new(K.SpawnSoul)),
                Cue(new(K.SpawnGreyColourEffect)),
                Show(F.Released, 128),
                Cue(new(K.SetGreyEvent)),
                Cue(new(K.FinishRelease)),
            ],
            [P.AlreadyGrey] =
            [
                Cue(new(K.FinishRelease)),
                Cue(new(K.ApplyGreyColors)),
            ],
        });

    /// <summary>
    /// The animation that starts at each segment: the entry runs on into the oscillation, which loops; a release and the
    /// already-grey finish end the statue's animation, ending its picture animation.
    /// </summary>
    internal static IReadOnlyDictionary<P, Animation<P>> StartingAt { get; } = new Dictionary<P, Animation<P>>
    {
        [P.Entry] = Animation<P>.LeadingInto([new(P.Entry, 0, 3, 1)], [new(P.Oscillate, 0, 6, 1)]),
        [P.Oscillate] = Animation<P>.Looping([new(P.Oscillate, 0, 6, 1)]),
        [P.Release] = new([new(P.Release, 0, 12, 1)], AnimationEnd.Stops),
        [P.AlreadyGrey] = new([new(P.AlreadyGrey, 0, 2, 1)], AnimationEnd.Stops),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<P> StartingWith(P segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="picture"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<F, TourianStatueBlockCue> Show(F picture, int duration) =>
        new AnimationBeat<F, TourianStatueBlockCue>.Frame(picture, duration);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<F, TourianStatueBlockCue> Cue(TourianStatueBlockCue cue) =>
        new AnimationBeat<F, TourianStatueBlockCue>.Event(cue);
}
