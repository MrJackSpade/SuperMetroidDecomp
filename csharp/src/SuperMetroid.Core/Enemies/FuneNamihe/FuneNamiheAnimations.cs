using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheCueKind;
using P = SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The Fune's and Namihe's idle and spitting animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="FuneNamiheCue"/> naming what it does.
/// </summary>
internal static class FuneNamiheAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FuneNamiheSegment, EnemyFrameId, FuneNamiheCue> Segments { get; } = new(
        new Dictionary<FuneNamiheSegment, AnimationBeat<EnemyFrameId, FuneNamiheCue>[]>
    {
        [P.FuneIdleLeft] =
        [
            Show(EnemyFrameId.FuneLeftIdle, 1),
        ],
        [P.FuneActiveLeft] =
        [
            Show(EnemyFrameId.FuneLeftIdle, 16),
            Show(EnemyFrameId.FuneActiveLeftSecond, 8),
            Show(EnemyFrameId.FuneActiveLeftThird, 8),
            Show(EnemyFrameId.FuneActiveLeftFourth, 8),
            Cue(new(K.FuneFireLeft)),
            Cue(new(K.SpitSound)),
            Show(EnemyFrameId.FuneActiveLeftFifth, 16),
            Show(EnemyFrameId.FuneActiveLeftFourth, 8),
            Show(EnemyFrameId.FuneActiveLeftThird, 8),
            Show(EnemyFrameId.FuneActiveLeftSecond, 8),
            Cue(new(K.FinishActivity)),
        ],
        [P.FuneIdleRight] =
        [
            Show(EnemyFrameId.FuneRightIdle, 1),
        ],
        [P.FuneActiveRight] =
        [
            Show(EnemyFrameId.FuneRightIdle, 16),
            Show(EnemyFrameId.FuneActiveRightSecond, 8),
            Show(EnemyFrameId.FuneActiveRightThird, 8),
            Show(EnemyFrameId.FuneActiveRightFourth, 8),
            Cue(new(K.FuneFireRight)),
            Cue(new(K.SpitSound)),
            Show(EnemyFrameId.FuneActiveRightFifth, 16),
            Show(EnemyFrameId.FuneActiveRightFourth, 8),
            Show(EnemyFrameId.FuneActiveRightThird, 8),
            Show(EnemyFrameId.FuneActiveRightSecond, 8),
            Cue(new(K.FinishActivity)),
        ],
        [P.NamiheIdleLeft] =
        [
            Show(EnemyFrameId.NamiheLeftIdle, 1),
        ],
        [P.NamiheActiveLeft] =
        [
            Show(EnemyFrameId.NamiheLeftIdle, 8),
            Show(EnemyFrameId.NamiheActiveLeftSecond, 8),
            Show(EnemyFrameId.NamiheActiveLeftThird, 8),
            Show(EnemyFrameId.NamiheActiveLeftFourth, 8),
            Show(EnemyFrameId.NamiheActiveLeftFifth, 8),
            Cue(new(K.NamiheFireLeft)),
            Cue(new(K.SpitSound)),
            Show(EnemyFrameId.NamiheActiveLeftSixth, 8),
            Show(EnemyFrameId.NamiheActiveLeftFifth, 8),
            Show(EnemyFrameId.NamiheActiveLeftFourth, 8),
            Show(EnemyFrameId.NamiheActiveLeftThird, 8),
            Cue(new(K.FinishActivity)),
        ],
        [P.NamiheIdleRight] =
        [
            Show(EnemyFrameId.NamiheRightIdle, 1),
        ],
        [P.NamiheActiveRight] =
        [
            Show(EnemyFrameId.NamiheRightIdle, 8),
            Show(EnemyFrameId.NamiheActiveRightSecond, 8),
            Show(EnemyFrameId.NamiheActiveRightThird, 8),
            Show(EnemyFrameId.NamiheActiveRightFourth, 8),
            Show(EnemyFrameId.NamiheActiveRightFifth, 8),
            Cue(new(K.NamiheFireRight)),
            Cue(new(K.SpitSound)),
            Show(EnemyFrameId.NamiheActiveRightSixth, 8),
            Show(EnemyFrameId.NamiheActiveRightFifth, 8),
            Show(EnemyFrameId.NamiheActiveRightFourth, 8),
            Show(EnemyFrameId.NamiheActiveRightThird, 8),
            Cue(new(K.FinishActivity)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<FuneNamiheSegment, Animation<FuneNamiheSegment>> StartingAt { get; } = new Dictionary<FuneNamiheSegment, Animation<FuneNamiheSegment>>
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
    internal static Animation<FuneNamiheSegment> StartingWith(FuneNamiheSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, FuneNamiheCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, FuneNamiheCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, FuneNamiheCue> Cue(FuneNamiheCue cue) => new AnimationBeat<EnemyFrameId, FuneNamiheCue>.Event(cue);
}
