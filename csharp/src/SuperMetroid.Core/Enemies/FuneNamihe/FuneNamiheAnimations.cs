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
            Show(EnemyFrameId.FuneFacingLeftMouthClosed, 1),
        ],
        [P.FuneActiveLeft] =
        [
            Show(EnemyFrameId.FuneFacingLeftMouthClosed, 16),
            Show(EnemyFrameId.FuneFacingLeftMouthOpening, 8),
            Show(EnemyFrameId.FuneFacingLeftMouthHalfOpen, 8),
            Show(EnemyFrameId.FuneFacingLeftMouthOpen, 8),
            Cue(new(K.FuneFireLeft)),
            Cue(new(K.SpitSound)),
            Show(EnemyFrameId.FuneFacingLeftMouthWide, 16),
            Show(EnemyFrameId.FuneFacingLeftMouthOpen, 8),
            Show(EnemyFrameId.FuneFacingLeftMouthHalfOpen, 8),
            Show(EnemyFrameId.FuneFacingLeftMouthOpening, 8),
            Cue(new(K.FinishActivity)),
        ],
        [P.FuneIdleRight] =
        [
            Show(EnemyFrameId.FuneFacingRightMouthClosed, 1),
        ],
        [P.FuneActiveRight] =
        [
            Show(EnemyFrameId.FuneFacingRightMouthClosed, 16),
            Show(EnemyFrameId.FuneFacingRightMouthOpening, 8),
            Show(EnemyFrameId.FuneFacingRightMouthHalfOpen, 8),
            Show(EnemyFrameId.FuneFacingRightMouthOpen, 8),
            Cue(new(K.FuneFireRight)),
            Cue(new(K.SpitSound)),
            Show(EnemyFrameId.FuneFacingRightMouthWide, 16),
            Show(EnemyFrameId.FuneFacingRightMouthOpen, 8),
            Show(EnemyFrameId.FuneFacingRightMouthHalfOpen, 8),
            Show(EnemyFrameId.FuneFacingRightMouthOpening, 8),
            Cue(new(K.FinishActivity)),
        ],
        [P.NamiheIdleLeft] =
        [
            Show(EnemyFrameId.NamiheFacingLeftMouthClosed, 1),
        ],
        [P.NamiheActiveLeft] =
        [
            Show(EnemyFrameId.NamiheFacingLeftMouthClosed, 8),
            Show(EnemyFrameId.NamiheFacingLeftMouthOpening, 8),
            Show(EnemyFrameId.NamiheFacingLeftMouthHalfOpen, 8),
            Show(EnemyFrameId.NamiheFacingLeftMouthOpen, 8),
            Show(EnemyFrameId.NamiheFacingLeftMouthWide, 8),
            Cue(new(K.NamiheFireLeft)),
            Cue(new(K.SpitSound)),
            Show(EnemyFrameId.NamiheFacingLeftMouthWidest, 8),
            Show(EnemyFrameId.NamiheFacingLeftMouthWide, 8),
            Show(EnemyFrameId.NamiheFacingLeftMouthOpen, 8),
            Show(EnemyFrameId.NamiheFacingLeftMouthHalfOpen, 8),
            Cue(new(K.FinishActivity)),
        ],
        [P.NamiheIdleRight] =
        [
            Show(EnemyFrameId.NamiheFacingRightMouthClosed, 1),
        ],
        [P.NamiheActiveRight] =
        [
            Show(EnemyFrameId.NamiheFacingRightMouthClosed, 8),
            Show(EnemyFrameId.NamiheFacingRightMouthOpening, 8),
            Show(EnemyFrameId.NamiheFacingRightMouthHalfOpen, 8),
            Show(EnemyFrameId.NamiheFacingRightMouthOpen, 8),
            Show(EnemyFrameId.NamiheFacingRightMouthWide, 8),
            Cue(new(K.NamiheFireRight)),
            Cue(new(K.SpitSound)),
            Show(EnemyFrameId.NamiheFacingRightMouthWidest, 8),
            Show(EnemyFrameId.NamiheFacingRightMouthWide, 8),
            Show(EnemyFrameId.NamiheFacingRightMouthOpen, 8),
            Show(EnemyFrameId.NamiheFacingRightMouthHalfOpen, 8),
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
