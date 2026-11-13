using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Areas.Tourian.TourianStatueTileCueKind;
using P = SuperMetroid.Core.Areas.Tourian.TourianStatueTileSegment;
using F = SuperMetroid.Core.Fx.Artwork.TourianStatueTilePicture;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The typed animations a Tourian boss statue's animated tiles play (#1600).
/// All four statues share the layout; only the first idle frame's duration differs, and every other operand is read
/// from the statue's <see cref="TourianStatueAnimationDefinition"/> when an event is reached. The Tourian animation
/// lookup check matches each statue's segments beat by beat, and its animations' control flow, with the game's.
/// </summary>
internal static class TourianStatueTileAnimations
{
    /// <summary>Builds one statue's segments, whose first idle frame lasts <paramref name="firstFrameDuration"/> updates.</summary>
    internal static AnimationSegments<P, F, TourianStatueTileCue> SegmentsFor(int firstFrameDuration) => new(
        new Dictionary<P, AnimationBeat<F, TourianStatueTileCue>[]>
        {
            [P.Entry] =
            [
                Cue(new(K.MarkAnimating)),
                Cue(new(K.GoToIfGrey, Target: P.AlreadyGrey)),
                Show(F.OscillationFirst, firstFrameDuration),
            ],
            [P.Oscillate] =
            [
                Show(F.OscillationSecond, 12),
                Show(F.OscillationThird, 12),
                Show(F.OscillationSecond, 12),
                Show(F.OscillationFirst, 16),
                Cue(new(K.GoToIfBossDefeated, Target: P.Release)),
                Cue(new(K.ClearAnimating)),
            ],
            [P.Release] =
            [
                Cue(new(K.GoToIfReleaseInProgress, Target: P.Oscillate)),
                Cue(new(K.ClaimRelease)),
                Cue(new(K.ClearThreeColors)),
                Show(F.OscillationSecond, 16),
                Show(F.OscillationThird, 16),
                Cue(new(K.SpawnEyeGlow)),
                Show(F.Released, 192),
                Cue(new(K.SpawnSoul)),
                Cue(new(K.SpawnGreyPaletteFx)),
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
    /// already-grey finish end the statue's animation, deleting its animated-tile object.
    /// </summary>
    internal static IReadOnlyDictionary<P, Animation<P>> StartingAt { get; } = new Dictionary<P, Animation<P>>
    {
        [P.Entry] = new([new(P.Entry, 0, 3, 1), new(P.Oscillate, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.Oscillate] = new([new(P.Oscillate, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Release] = new([new(P.Release, 0, 12, 1)], AnimationEnd.Stops),
        [P.AlreadyGrey] = new([new(P.AlreadyGrey, 0, 2, 1)], AnimationEnd.Stops),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<P> StartingWith(P segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="picture"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<F, TourianStatueTileCue> Show(F picture, int duration) =>
        new AnimationBeat<F, TourianStatueTileCue>.Frame(picture, duration);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<F, TourianStatueTileCue> Cue(TourianStatueTileCue cue) =>
        new AnimationBeat<F, TourianStatueTileCue>.Event(cue);
}
