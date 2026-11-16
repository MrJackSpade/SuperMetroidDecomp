using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Shaktool.ShaktoolCueKind;
using P = SuperMetroid.Core.Enemies.Shaktool.ShaktoolSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// The Shaktool's saw-hand, arm and head animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="ShaktoolCue"/> naming what it does.
/// </summary>
internal static class ShaktoolAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ShaktoolSegment, EnemyFrameId, ShaktoolCue> Segments { get; } = new(
        new Dictionary<ShaktoolSegment, AnimationBeat<EnemyFrameId, ShaktoolCue>[]>
    {
        [P.SawHandHeadBobPrimaryPiece] =
        [
            Wait(20),
        ],
        [P.SawHandHeadBobFinalPiece] =
        [
            Wait(20),
            Cue(new(K.ResumeMotion)),
        ],
        [P.SawHandPrimaryPiece] =
        [
            Show(EnemyFrameId.ShaktoolSawHandPrimaryPieceTurned0, 10),
            Show(EnemyFrameId.ShaktoolSawHandPrimaryPieceTurned30, 10),
            Show(EnemyFrameId.ShaktoolSawHandPrimaryPieceTurned60, 10),
        ],
        [P.SawHandFinalPiece] =
        [
            Show(EnemyFrameId.ShaktoolSawHandFinalPieceTurned0, 3),
            Show(EnemyFrameId.ShaktoolSawHandFinalPieceTurned30, 3),
            Show(EnemyFrameId.ShaktoolSawHandFinalPieceTurned60, 3),
        ],
        [P.ArmPieceHeadBobBack] =
        [
            Cue(new(K.Lower)),
            Wait(20),
            Cue(new(K.Raise)),
        ],
        [P.ArmPieceHeadBobFront] =
        [
            Wait(4),
            Cue(new(K.Lower)),
            Wait(12),
            Cue(new(K.Raise)),
            Wait(4),
        ],
        [P.ArmPieceNormal] =
        [
            Hold(EnemyFrameId.ShaktoolArm),
        ],
        [P.HeadHeadBob] =
        [
            Wait(8),
            Cue(new(K.Lower)),
            Wait(4),
            Cue(new(K.Raise)),
            Wait(9),
        ],
        [P.HeadAimingLeft] =
        [
            Hold(EnemyFrameId.ShaktoolHeadLeft),
        ],
        [P.HeadAimingUpLeft] =
        [
            Hold(EnemyFrameId.ShaktoolHeadUpLeft),
        ],
        [P.HeadAimingUp] =
        [
            Hold(EnemyFrameId.ShaktoolHeadUp),
        ],
        [P.HeadAimingUpRight] =
        [
            Hold(EnemyFrameId.ShaktoolHeadUpRight),
        ],
        [P.HeadAimingRight] =
        [
            Hold(EnemyFrameId.ShaktoolHeadRight),
        ],
        [P.HeadAimingDownRight] =
        [
            Hold(EnemyFrameId.ShaktoolHeadDownRight),
        ],
        [P.HeadAimingDown] =
        [
            Hold(EnemyFrameId.ShaktoolHeadDown),
        ],
        [P.HeadAimingDownLeft] =
        [
            Hold(EnemyFrameId.ShaktoolHeadDownLeft),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at; the final piece's spin only runs on from
    /// its head bob (#2166).
    /// </summary>
    internal static IReadOnlyDictionary<ShaktoolSegment, Animation<ShaktoolSegment>> StartingAt { get; } = new Dictionary<ShaktoolSegment, Animation<ShaktoolSegment>>
    {
        [P.SawHandHeadBobPrimaryPiece] = new([Whole(P.SawHandHeadBobPrimaryPiece), Whole(P.SawHandPrimaryPiece)], AnimationEnd.LoopsFrom(1)),
        [P.SawHandHeadBobFinalPiece] = new([Whole(P.SawHandHeadBobFinalPiece), Whole(P.SawHandFinalPiece)], AnimationEnd.LoopsFrom(1)),
        [P.SawHandPrimaryPiece] = new([Whole(P.SawHandPrimaryPiece)], AnimationEnd.LoopsFrom(0)),
        [P.ArmPieceHeadBobBack] = new([Whole(P.ArmPieceHeadBobBack), Whole(P.ArmPieceNormal)], AnimationEnd.LoopsFrom(1)),
        [P.ArmPieceHeadBobFront] = new([Whole(P.ArmPieceHeadBobFront), Whole(P.ArmPieceNormal)], AnimationEnd.LoopsFrom(1)),
        [P.ArmPieceNormal] = new([Whole(P.ArmPieceNormal)], AnimationEnd.LoopsFrom(0)),
        [P.HeadHeadBob] = new([Whole(P.HeadHeadBob), Whole(P.HeadAimingLeft)], AnimationEnd.LoopsFrom(1)),
        [P.HeadAimingLeft] = new([Whole(P.HeadAimingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.HeadAimingUpLeft] = new([Whole(P.HeadAimingUpLeft)], AnimationEnd.LoopsFrom(0)),
        [P.HeadAimingUp] = new([Whole(P.HeadAimingUp)], AnimationEnd.LoopsFrom(0)),
        [P.HeadAimingUpRight] = new([Whole(P.HeadAimingUpRight)], AnimationEnd.LoopsFrom(0)),
        [P.HeadAimingRight] = new([Whole(P.HeadAimingRight)], AnimationEnd.LoopsFrom(0)),
        [P.HeadAimingDownRight] = new([Whole(P.HeadAimingDownRight)], AnimationEnd.LoopsFrom(0)),
        [P.HeadAimingDown] = new([Whole(P.HeadAimingDown)], AnimationEnd.LoopsFrom(0)),
        [P.HeadAimingDownLeft] = new([Whole(P.HeadAimingDownLeft)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ShaktoolSegment> StartingWith(ShaktoolSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<ShaktoolSegment> Whole(ShaktoolSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, ShaktoolCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, ShaktoolCue>.Frame(frame, duration);

    /// <summary>
    /// Shows <paramref name="frame"/> for good (#2068): the arm and the aimed head rest on their pose, the head until its
    /// orientation restarts it on the next update.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, ShaktoolCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, ShaktoolCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, ShaktoolCue> Cue(ShaktoolCue cue) => new AnimationBeat<EnemyFrameId, ShaktoolCue>.Event(cue);

    /// <summary>Waits <paramref name="updates"/> updates on the current frame.</summary>
    private static AnimationBeat<EnemyFrameId, ShaktoolCue> Wait(int updates) => new AnimationBeat<EnemyFrameId, ShaktoolCue>.Pause(updates);
}
