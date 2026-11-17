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
        [P.FuneIdleLeft] = [Hold(EnemyFrameId.FuneFacingLeftMouthClosed)],
        [P.FuneActiveLeft] =
        [
            Show(EnemyFrameId.FuneFacingLeftMouthClosed, 16),
            Show(EnemyFrameId.FuneFacingLeftMouthOpening, 8),
            Show(EnemyFrameId.FuneFacingLeftMouthHalfOpen, 8),
            Show(EnemyFrameId.FuneFacingLeftMouthOpen, 8),
            Cue(new FuneNamiheCue.Fire(HorizontalDirection.Left)),
            Cue(new FuneNamiheCue.Signal(K.SpitSound)),
            Show(EnemyFrameId.FuneFacingLeftMouthWide, 16),
            Show(EnemyFrameId.FuneFacingLeftMouthOpen, 8),
            Show(EnemyFrameId.FuneFacingLeftMouthHalfOpen, 8),
            Show(EnemyFrameId.FuneFacingLeftMouthOpening, 8),
            Cue(new FuneNamiheCue.Signal(K.FinishActivity)),
        ],
        [P.FuneIdleRight] = [Hold(EnemyFrameId.FuneFacingRightMouthClosed)],
        [P.FuneActiveRight] =
        [
            Show(EnemyFrameId.FuneFacingRightMouthClosed, 16),
            Show(EnemyFrameId.FuneFacingRightMouthOpening, 8),
            Show(EnemyFrameId.FuneFacingRightMouthHalfOpen, 8),
            Show(EnemyFrameId.FuneFacingRightMouthOpen, 8),
            Cue(new FuneNamiheCue.Fire(HorizontalDirection.Right)),
            Cue(new FuneNamiheCue.Signal(K.SpitSound)),
            Show(EnemyFrameId.FuneFacingRightMouthWide, 16),
            Show(EnemyFrameId.FuneFacingRightMouthOpen, 8),
            Show(EnemyFrameId.FuneFacingRightMouthHalfOpen, 8),
            Show(EnemyFrameId.FuneFacingRightMouthOpening, 8),
            Cue(new FuneNamiheCue.Signal(K.FinishActivity)),
        ],
        [P.NamiheIdleLeft] = [Hold(EnemyFrameId.NamiheFacingLeftMouthClosed)],
        [P.NamiheActiveLeft] =
        [
            Show(EnemyFrameId.NamiheFacingLeftMouthClosed, 8),
            Show(EnemyFrameId.NamiheFacingLeftMouthOpening, 8),
            Show(EnemyFrameId.NamiheFacingLeftMouthHalfOpen, 8),
            Show(EnemyFrameId.NamiheFacingLeftMouthOpen, 8),
            Show(EnemyFrameId.NamiheFacingLeftMouthWide, 8),
            Cue(new FuneNamiheCue.Fire(HorizontalDirection.Left)),
            Cue(new FuneNamiheCue.Signal(K.SpitSound)),
            Show(EnemyFrameId.NamiheFacingLeftMouthWidest, 8),
            Show(EnemyFrameId.NamiheFacingLeftMouthWide, 8),
            Show(EnemyFrameId.NamiheFacingLeftMouthOpen, 8),
            Show(EnemyFrameId.NamiheFacingLeftMouthHalfOpen, 8),
            Cue(new FuneNamiheCue.Signal(K.FinishActivity)),
        ],
        [P.NamiheIdleRight] = [Hold(EnemyFrameId.NamiheFacingRightMouthClosed)],
        [P.NamiheActiveRight] =
        [
            Show(EnemyFrameId.NamiheFacingRightMouthClosed, 8),
            Show(EnemyFrameId.NamiheFacingRightMouthOpening, 8),
            Show(EnemyFrameId.NamiheFacingRightMouthHalfOpen, 8),
            Show(EnemyFrameId.NamiheFacingRightMouthOpen, 8),
            Show(EnemyFrameId.NamiheFacingRightMouthWide, 8),
            Cue(new FuneNamiheCue.Fire(HorizontalDirection.Right)),
            Cue(new FuneNamiheCue.Signal(K.SpitSound)),
            Show(EnemyFrameId.NamiheFacingRightMouthWidest, 8),
            Show(EnemyFrameId.NamiheFacingRightMouthWide, 8),
            Show(EnemyFrameId.NamiheFacingRightMouthOpen, 8),
            Show(EnemyFrameId.NamiheFacingRightMouthHalfOpen, 8),
            Cue(new FuneNamiheCue.Signal(K.FinishActivity)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<FuneNamiheSegment, Animation<FuneNamiheSegment>> StartingAt { get; } = new Dictionary<FuneNamiheSegment, Animation<FuneNamiheSegment>>
    {
        [P.FuneIdleLeft] = new([Whole(P.FuneIdleLeft)], AnimationEnd.Holds),
        [P.FuneActiveLeft] = new([Whole(P.FuneActiveLeft), Whole(P.FuneIdleLeft)], AnimationEnd.Holds),
        [P.FuneIdleRight] = new([Whole(P.FuneIdleRight)], AnimationEnd.Holds),
        [P.FuneActiveRight] = new([Whole(P.FuneActiveRight), Whole(P.FuneIdleRight)], AnimationEnd.Holds),
        [P.NamiheIdleLeft] = new([Whole(P.NamiheIdleLeft)], AnimationEnd.Holds),
        [P.NamiheActiveLeft] = new([Whole(P.NamiheActiveLeft), Whole(P.NamiheIdleLeft)], AnimationEnd.Holds),
        [P.NamiheIdleRight] = new([Whole(P.NamiheIdleRight)], AnimationEnd.Holds),
        [P.NamiheActiveRight] = new([Whole(P.NamiheActiveRight), Whole(P.NamiheIdleRight)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FuneNamiheSegment> StartingWith(FuneNamiheSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<FuneNamiheSegment> Whole(FuneNamiheSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>A still picture (#2275): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyFrameId, FuneNamiheCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, FuneNamiheCue>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, FuneNamiheCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, FuneNamiheCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, FuneNamiheCue> Cue(FuneNamiheCue cue) => new AnimationBeat<EnemyFrameId, FuneNamiheCue>.Event(cue);
}
