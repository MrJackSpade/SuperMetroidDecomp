using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheCueKind;
using P = SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The Fune's and Namihe's idle and spitting animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="FuneNamiheCue"/> with its typed operands.
/// </summary>
internal static class FuneNamiheAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FuneNamiheProgram, EnemyFrameId, FuneNamiheCue> Segments { get; } = new(
        new Dictionary<FuneNamiheProgram, AnimationBeat<EnemyFrameId, FuneNamiheCue>[]>
    {
        [P.FuneIdleLeft] =
        [
            Show(EnemyFrameId.FuneLeftIdle, 1),
        ],
        [P.FuneActiveLeft] =
        [
            Show(EnemyFrameId.FuneLeftIdle, 16),
            Show(EnemyFrameId.FuneLeftActive0, 8),
            Show(EnemyFrameId.FuneLeftActive1, 8),
            Show(EnemyFrameId.FuneLeftActive2, 8),
            Cue(new(K.FuneFireLeft)),
            Cue(new(K.SpitSound)),
            Show(EnemyFrameId.FuneLeftActive3, 16),
            Show(EnemyFrameId.FuneLeftActive2, 8),
            Show(EnemyFrameId.FuneLeftActive1, 8),
            Show(EnemyFrameId.FuneLeftActive0, 8),
            Cue(new(K.FinishActivity)),
        ],
        [P.FuneIdleRight] =
        [
            Show(EnemyFrameId.FuneRightIdle, 1),
        ],
        [P.FuneActiveRight] =
        [
            Show(EnemyFrameId.FuneRightIdle, 16),
            Show(EnemyFrameId.FuneRightActive0, 8),
            Show(EnemyFrameId.FuneRightActive1, 8),
            Show(EnemyFrameId.FuneRightActive2, 8),
            Cue(new(K.FuneFireRight)),
            Cue(new(K.SpitSound)),
            Show(EnemyFrameId.FuneRightActive3, 16),
            Show(EnemyFrameId.FuneRightActive2, 8),
            Show(EnemyFrameId.FuneRightActive1, 8),
            Show(EnemyFrameId.FuneRightActive0, 8),
            Cue(new(K.FinishActivity)),
        ],
        [P.NamiheIdleLeft] =
        [
            Show(EnemyFrameId.NamiheLeftIdle, 1),
        ],
        [P.NamiheActiveLeft] =
        [
            Show(EnemyFrameId.NamiheLeftIdle, 8),
            Show(EnemyFrameId.NamiheLeftActive0, 8),
            Show(EnemyFrameId.NamiheLeftActive1, 8),
            Show(EnemyFrameId.NamiheLeftActive2, 8),
            Show(EnemyFrameId.NamiheLeftActive3, 8),
            Cue(new(K.NamiheFireLeft)),
            Cue(new(K.SpitSound)),
            Show(EnemyFrameId.NamiheLeftActive4, 8),
            Show(EnemyFrameId.NamiheLeftActive3, 8),
            Show(EnemyFrameId.NamiheLeftActive2, 8),
            Show(EnemyFrameId.NamiheLeftActive1, 8),
            Cue(new(K.FinishActivity)),
        ],
        [P.NamiheIdleRight] =
        [
            Show(EnemyFrameId.NamiheRightIdle, 1),
        ],
        [P.NamiheActiveRight] =
        [
            Show(EnemyFrameId.NamiheRightIdle, 8),
            Show(EnemyFrameId.NamiheRightActive0, 8),
            Show(EnemyFrameId.NamiheRightActive1, 8),
            Show(EnemyFrameId.NamiheRightActive2, 8),
            Show(EnemyFrameId.NamiheRightActive3, 8),
            Cue(new(K.NamiheFireRight)),
            Cue(new(K.SpitSound)),
            Show(EnemyFrameId.NamiheRightActive4, 8),
            Show(EnemyFrameId.NamiheRightActive3, 8),
            Show(EnemyFrameId.NamiheRightActive2, 8),
            Show(EnemyFrameId.NamiheRightActive1, 8),
            Cue(new(K.FinishActivity)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<FuneNamiheProgram, Animation<FuneNamiheProgram>> StartingAt { get; } = new Dictionary<FuneNamiheProgram, Animation<FuneNamiheProgram>>
    {
        [P.FuneIdleLeft] = new([new(P.FuneIdleLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.FuneActiveLeft] = new([new(P.FuneActiveLeft, 0, 11, 1), new(P.FuneIdleLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.FuneIdleRight] = new([new(P.FuneIdleRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.FuneActiveRight] = new([new(P.FuneActiveRight, 0, 11, 1), new(P.FuneIdleRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.NamiheIdleLeft] = new([new(P.NamiheIdleLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.NamiheActiveLeft] = new([new(P.NamiheActiveLeft, 0, 12, 1), new(P.NamiheIdleLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.NamiheIdleRight] = new([new(P.NamiheIdleRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.NamiheActiveRight] = new([new(P.NamiheActiveRight, 0, 12, 1), new(P.NamiheIdleRight, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FuneNamiheProgram> StartingWith(FuneNamiheProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, FuneNamiheCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, FuneNamiheCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, FuneNamiheCue> Cue(FuneNamiheCue cue) => new AnimationBeat<EnemyFrameId, FuneNamiheCue>.Event(cue);
}
