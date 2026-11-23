using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Shaktool.ShaktoolCueKind;
using P = SuperMetroid.Core.Enemies.Shaktool.ShaktoolSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// The Shaktool's saw-hand, arm and head animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="ShaktoolCue"/> naming what it does.
/// </summary>
internal static partial class ShaktoolAnimations
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
        [P.HeadAimingDown] =
        [
            Hold(EnemyFrameId.ShaktoolHeadDown),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at; the final piece's spin only runs on from
    /// its head bob (#2166).
    /// </summary>
    internal static Dictionary<ShaktoolSegment, Animation<ShaktoolSegment>> StartingAt { get; } = new Dictionary<ShaktoolSegment, Animation<ShaktoolSegment>>
    {
        [P.SawHandHeadBobPrimaryPiece] = Animation<ShaktoolSegment>.LeadingInto([Segments.Whole(P.SawHandHeadBobPrimaryPiece)], [Segments.Whole(P.SawHandPrimaryPiece)]),
        [P.SawHandHeadBobFinalPiece] = Animation<ShaktoolSegment>.LeadingInto([Segments.Whole(P.SawHandHeadBobFinalPiece)], [Segments.Whole(P.SawHandFinalPiece)]),
        [P.SawHandPrimaryPiece] = Animation<ShaktoolSegment>.Looping([Segments.Whole(P.SawHandPrimaryPiece)]),
        [P.ArmPieceHeadBobBack] = new([Segments.Whole(P.ArmPieceHeadBobBack), Segments.Whole(P.ArmPieceNormal)], AnimationEnd.Holds),
        [P.ArmPieceHeadBobFront] = new([Segments.Whole(P.ArmPieceHeadBobFront), Segments.Whole(P.ArmPieceNormal)], AnimationEnd.Holds),
        [P.ArmPieceNormal] = new([Segments.Whole(P.ArmPieceNormal)], AnimationEnd.Holds),
        [P.HeadHeadBob] = new([Segments.Whole(P.HeadHeadBob), Segments.Whole(P.HeadAimingLeft)], AnimationEnd.Holds),
        [P.HeadAimingLeft] = new([Segments.Whole(P.HeadAimingLeft)], AnimationEnd.Holds),
        [P.HeadAimingDown] = new([Segments.Whole(P.HeadAimingDown)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ShaktoolSegment> StartingWith(ShaktoolSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, ShaktoolCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, ShaktoolCue>.Frame(frame, duration);

    /// <summary>
    /// Shows <paramref name="frame"/> for good (#2068): the arm and the head's first pose and head bob rest on their pose.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, ShaktoolCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, ShaktoolCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, ShaktoolCue> Cue(ShaktoolCue cue) => new AnimationBeat<EnemyFrameId, ShaktoolCue>.Event(cue);

    /// <summary>Waits <paramref name="updates"/> updates on the current frame.</summary>
    private static AnimationBeat<EnemyFrameId, ShaktoolCue> Wait(int updates) => new AnimationBeat<EnemyFrameId, ShaktoolCue>.Pause(updates);
}
