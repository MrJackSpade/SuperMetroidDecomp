using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Areas.Tourian.TourianStatueTileCueKind;
using P = SuperMetroid.Core.Areas.Tourian.TourianStatueTileProgram;
using F = SuperMetroid.Core.Fx.Artwork.TourianStatueTilePicture;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The typed programs a Tourian boss statue's animated tiles run, in place of the bank-$87 statue instruction lists.
/// All four statues share the layout; only the first idle frame's duration differs, and every other operand is read
/// from the statue's <see cref="TourianStatueAnimatedTileProgramDefinition"/> when a cue runs. The ROM oracle in
/// <c>LookupTourianProgramVerification</c> matches each statue's programs step by step with its native list.
/// </summary>
internal static class TourianStatueTileAnimations
{
    /// <summary>Builds one statue's programs, whose first idle frame lasts <paramref name="firstFrameDuration"/> updates.</summary>
    internal static AnimationProgramSet<P, F, TourianStatueTileCue> Create(int firstFrameDuration) => new(
        new Dictionary<P, AnimationProgramStep<P, F, TourianStatueTileCue>[]>
        {
            [P.Entry] =
            [
                Cue(new(K.MarkAnimating)),
                Cue(new(K.GoToIfGrey, Target: P.AlreadyGrey)),
                Show(F.OscillationFirst, firstFrameDuration),
                FallInto(P.Oscillate),
            ],
            [P.Oscillate] =
            [
                Show(F.OscillationSecond, 12),
                Show(F.OscillationThird, 12),
                Show(F.OscillationSecond, 12),
                Show(F.OscillationFirst, 16),
                Cue(new(K.GoToIfBossDefeated, Target: P.Release)),
                Cue(new(K.ClearAnimating)),
                GoTo(P.Oscillate),
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
                Stop,
            ],
            [P.AlreadyGrey] =
            [
                Cue(new(K.FinishRelease)),
                Cue(new(K.ApplyGreyColors)),
                Stop,
            ],
        });

    /// <summary>Ends the program and deletes the statue's animated-tile object.</summary>
    private static AnimationProgramStep<P, F, TourianStatueTileCue> Stop => AnimationProgramStep<P, F, TourianStatueTileCue>.Stop;

    /// <summary>Shows <paramref name="picture"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<P, F, TourianStatueTileCue> Show(F picture, int duration) =>
        AnimationProgramStep<P, F, TourianStatueTileCue>.Show(picture, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<P, F, TourianStatueTileCue> Cue(TourianStatueTileCue cue) =>
        AnimationProgramStep<P, F, TourianStatueTileCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<P, F, TourianStatueTileCue> GoTo(P program) =>
        AnimationProgramStep<P, F, TourianStatueTileCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<P, F, TourianStatueTileCue> FallInto(P program) =>
        AnimationProgramStep<P, F, TourianStatueTileCue>.FallInto(program);
}
