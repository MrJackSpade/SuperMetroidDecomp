using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheCueKind;
using P = SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The Fune's and Namihe's idle and spitting animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="FuneNamiheCue"/> naming what it does.
/// </summary>
internal static partial class FuneNamiheAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FuneNamiheSegment, EnemyFrameId, FuneNamiheCue> Segments { get; } = new(
        new Dictionary<FuneNamiheSegment, AnimationBeat<EnemyFrameId, FuneNamiheCue>[]>
    {
        [P.FuneIdleLeft] = [Hold(EnemyFrameId.FuneFacingLeftMouthClosed)],
        [P.FuneActiveLeft] = FuneSpit(EnemyFrameId.FuneFacingLeftMouthClosed, EnemyFrameId.FuneFacingLeftMouthOpening, EnemyFrameId.FuneFacingLeftMouthHalfOpen, EnemyFrameId.FuneFacingLeftMouthOpen, HorizontalDirection.Left, EnemyFrameId.FuneFacingLeftMouthWide),
        [P.FuneIdleRight] = [Hold(EnemyFrameId.FuneFacingRightMouthClosed)],
        [P.FuneActiveRight] = FuneSpit(EnemyFrameId.FuneFacingRightMouthClosed, EnemyFrameId.FuneFacingRightMouthOpening, EnemyFrameId.FuneFacingRightMouthHalfOpen, EnemyFrameId.FuneFacingRightMouthOpen, HorizontalDirection.Right, EnemyFrameId.FuneFacingRightMouthWide),
        [P.NamiheIdleLeft] = [Hold(EnemyFrameId.NamiheFacingLeftMouthClosed)],
        [P.NamiheActiveLeft] = NamiheSpit(EnemyFrameId.NamiheFacingLeftMouthClosed, EnemyFrameId.NamiheFacingLeftMouthOpening, EnemyFrameId.NamiheFacingLeftMouthHalfOpen, EnemyFrameId.NamiheFacingLeftMouthOpen, EnemyFrameId.NamiheFacingLeftMouthWide, HorizontalDirection.Left, EnemyFrameId.NamiheFacingLeftMouthWidest),
        [P.NamiheIdleRight] = [Hold(EnemyFrameId.NamiheFacingRightMouthClosed)],
        [P.NamiheActiveRight] = NamiheSpit(EnemyFrameId.NamiheFacingRightMouthClosed, EnemyFrameId.NamiheFacingRightMouthOpening, EnemyFrameId.NamiheFacingRightMouthHalfOpen, EnemyFrameId.NamiheFacingRightMouthOpen, EnemyFrameId.NamiheFacingRightMouthWide, HorizontalDirection.Right, EnemyFrameId.NamiheFacingRightMouthWidest),
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static Dictionary<FuneNamiheSegment, Animation<FuneNamiheSegment>> StartingAt { get; } = new Dictionary<FuneNamiheSegment, Animation<FuneNamiheSegment>>
    {
        [P.FuneIdleLeft] = new([Segments.Whole(P.FuneIdleLeft)], AnimationEnd.Holds),
        [P.FuneActiveLeft] = new([Segments.Whole(P.FuneActiveLeft), Segments.Whole(P.FuneIdleLeft)], AnimationEnd.Holds),
        [P.FuneIdleRight] = new([Segments.Whole(P.FuneIdleRight)], AnimationEnd.Holds),
        [P.FuneActiveRight] = new([Segments.Whole(P.FuneActiveRight), Segments.Whole(P.FuneIdleRight)], AnimationEnd.Holds),
        [P.NamiheIdleLeft] = new([Segments.Whole(P.NamiheIdleLeft)], AnimationEnd.Holds),
        [P.NamiheActiveLeft] = new([Segments.Whole(P.NamiheActiveLeft), Segments.Whole(P.NamiheIdleLeft)], AnimationEnd.Holds),
        [P.NamiheIdleRight] = new([Segments.Whole(P.NamiheIdleRight)], AnimationEnd.Holds),
        [P.NamiheActiveRight] = new([Segments.Whole(P.NamiheActiveRight), Segments.Whole(P.NamiheIdleRight)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FuneNamiheSegment> StartingWith(FuneNamiheSegment segment) => StartingAt[segment];

    /// <summary>A Fune's spit over a facing's mouth poses, <paramref name="mouthClosed"/>, <paramref name="mouthOpening"/>, <paramref name="mouthHalfOpen"/>, <paramref name="mouthOpen"/>, <paramref name="facing"/>, <paramref name="mouthWide"/>: its mouth opens, it fires and spits, and its mouth closes again.</summary>
    private static AnimationBeat<EnemyFrameId, FuneNamiheCue>[] FuneSpit(EnemyFrameId mouthClosed, EnemyFrameId mouthOpening, EnemyFrameId mouthHalfOpen, EnemyFrameId mouthOpen, HorizontalDirection facing, EnemyFrameId mouthWide) =>
    [
        Show(mouthClosed, 16),
        Show(mouthOpening, 8),
        Show(mouthHalfOpen, 8),
        Show(mouthOpen, 8),
        Cue(new FuneNamiheCue.Fire(facing)),
        Cue(new FuneNamiheCue.Signal(K.SpitSound)),
        Show(mouthWide, 16),
        Show(mouthOpen, 8),
        Show(mouthHalfOpen, 8),
        Show(mouthOpening, 8),
        Cue(new FuneNamiheCue.Signal(K.FinishActivity)),
    ];

    /// <summary>A Namihe's spit over a facing's mouth poses, <paramref name="mouthClosed"/>, <paramref name="mouthOpening"/>, <paramref name="mouthHalfOpen"/>, <paramref name="mouthOpen"/>, <paramref name="mouthWide"/>, <paramref name="facing"/>, <paramref name="mouthWidest"/>: its mouth opens wide, it fires and spits, and its mouth closes again.</summary>
    private static AnimationBeat<EnemyFrameId, FuneNamiheCue>[] NamiheSpit(EnemyFrameId mouthClosed, EnemyFrameId mouthOpening, EnemyFrameId mouthHalfOpen, EnemyFrameId mouthOpen, EnemyFrameId mouthWide, HorizontalDirection facing, EnemyFrameId mouthWidest) =>
    [
        Show(mouthClosed, 8),
        Show(mouthOpening, 8),
        Show(mouthHalfOpen, 8),
        Show(mouthOpen, 8),
        Show(mouthWide, 8),
        Cue(new FuneNamiheCue.Fire(facing)),
        Cue(new FuneNamiheCue.Signal(K.SpitSound)),
        Show(mouthWidest, 8),
        Show(mouthWide, 8),
        Show(mouthOpen, 8),
        Show(mouthHalfOpen, 8),
        Cue(new FuneNamiheCue.Signal(K.FinishActivity)),
    ];

    /// <summary>A still picture (#2275): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyFrameId, FuneNamiheCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, FuneNamiheCue>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, FuneNamiheCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, FuneNamiheCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, FuneNamiheCue> Cue(FuneNamiheCue cue) => new AnimationBeat<EnemyFrameId, FuneNamiheCue>.Event(cue);
}
