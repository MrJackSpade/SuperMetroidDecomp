using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheCueKind;
using P = SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheProgram;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The Fune's and Namihe's idle and spitting animations as typed programs (#641). Each
/// frame shows a <see cref="FuneNamiheFrame"/> pose and each callback becomes a <see cref="FuneNamiheCue"/> with its typed operands.
/// </summary>
internal static class FuneNamiheAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FuneNamiheProgram, FuneNamiheFrame, FuneNamiheCue> Segments { get; } = new(
        new Dictionary<FuneNamiheProgram, AnimationBeat<FuneNamiheFrame, FuneNamiheCue>[]>
    {
        [P.FuneIdleLeft] =
        [
            Show(0, 1),
        ],
        [P.FuneActiveLeft] =
        [
            Show(0, 16),
            Show(1, 8),
            Show(2, 8),
            Show(3, 8),
            Cue(new(K.FuneFireLeft)),
            Cue(new(K.SpitSound)),
            Show(4, 16),
            Show(3, 8),
            Show(2, 8),
            Show(1, 8),
            Cue(new(K.FinishActivity)),
        ],
        [P.FuneIdleRight] =
        [
            Show(5, 1),
        ],
        [P.FuneActiveRight] =
        [
            Show(5, 16),
            Show(6, 8),
            Show(7, 8),
            Show(8, 8),
            Cue(new(K.FuneFireRight)),
            Cue(new(K.SpitSound)),
            Show(9, 16),
            Show(8, 8),
            Show(7, 8),
            Show(6, 8),
            Cue(new(K.FinishActivity)),
        ],
        [P.NamiheIdleLeft] =
        [
            Show(10, 1),
        ],
        [P.NamiheActiveLeft] =
        [
            Show(10, 8),
            Show(11, 8),
            Show(12, 8),
            Show(13, 8),
            Show(14, 8),
            Cue(new(K.NamiheFireLeft)),
            Cue(new(K.SpitSound)),
            Show(15, 8),
            Show(14, 8),
            Show(13, 8),
            Show(12, 8),
            Cue(new(K.FinishActivity)),
        ],
        [P.NamiheIdleRight] =
        [
            Show(16, 1),
        ],
        [P.NamiheActiveRight] =
        [
            Show(16, 8),
            Show(17, 8),
            Show(18, 8),
            Show(19, 8),
            Show(20, 8),
            Cue(new(K.NamiheFireRight)),
            Cue(new(K.SpitSound)),
            Show(21, 8),
            Show(20, 8),
            Show(19, 8),
            Show(18, 8),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<FuneNamiheFrame, FuneNamiheCue> Show(int pose, int duration) => new AnimationBeat<FuneNamiheFrame, FuneNamiheCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<FuneNamiheFrame, FuneNamiheCue> Cue(FuneNamiheCue cue) => new AnimationBeat<FuneNamiheFrame, FuneNamiheCue>.Event(cue);
}
