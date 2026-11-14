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
        [P.SawHandAttackPrimaryPiece] =
        [
            Wait(576),
        ],
        [P.SawHandAttackFinalPiece] =
        [
            Wait(576),
            Cue(new(K.ResetFrameBehaviors)),
        ],
        [P.SawHandHeadBobPrimaryPiece] =
        [
            Wait(20),
        ],
        [P.SawHandHeadBobFinalPiece] =
        [
            Wait(20),
            Cue(new(K.ResetFrameBehaviors)),
        ],
        [P.SawHandPrimaryPiece] =
        [
            Show(EnemyFrameId.ShaktoolSawHandPrimaryPieceFirst, 10),
            Show(EnemyFrameId.ShaktoolSawHandPrimaryPieceSecond, 10),
            Show(EnemyFrameId.ShaktoolSawHandPrimaryPieceThird, 10),
        ],
        [P.SawHandFinalPiece] =
        [
            Show(EnemyFrameId.ShaktoolSawHandFinalPieceFirst, 3),
            Show(EnemyFrameId.ShaktoolSawHandFinalPieceSecond, 3),
            Show(EnemyFrameId.ShaktoolSawHandFinalPieceThird, 3),
        ],
        [P.ArmPieceAttackBack] =
        [
            Wait(192),
            Cue(new(K.LowerAwayFromCenter)),
            Wait(128),
            Cue(new(K.RaiseTowardCenter)),
            Wait(256),
        ],
        [P.ArmPieceAttackFront] =
        [
            Wait(256),
            Cue(new(K.LowerAwayFromCenter)),
            Wait(128),
            Cue(new(K.RaiseTowardCenter)),
            Wait(192),
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
            Show(EnemyFrameId.ShaktoolArm, 119),
        ],
        [P.HeadAttack] =
        [
            Wait(128),
            Cue(new(K.LowerAwayFromCenter)),
            Wait(128),
            Cue(new(K.RaiseTowardCenter)),
            Wait(320),
            Wait(1),
        ],
        [P.HeadHeadBob] =
        [
            Wait(8),
            Cue(new(K.Lower)),
            Wait(4),
            Cue(new(K.Raise)),
            Wait(8),
            Wait(1),
        ],
        [P.HeadAimingLeft] =
        [
            Show(EnemyFrameId.ShaktoolHeadLeft, 1908),
        ],
        [P.HeadAimingUpLeft] =
        [
            Show(EnemyFrameId.ShaktoolHeadUpLeft, 1909),
        ],
        [P.HeadAimingUp] =
        [
            Show(EnemyFrameId.ShaktoolHeadUp, 1910),
        ],
        [P.HeadAimingUpRight] =
        [
            Show(EnemyFrameId.ShaktoolHeadUpRight, 1911),
        ],
        [P.HeadAimingRight] =
        [
            Show(EnemyFrameId.ShaktoolHeadRight, 1912),
        ],
        [P.HeadAimingDownRight] =
        [
            Show(EnemyFrameId.ShaktoolHeadDownRight, 1913),
        ],
        [P.HeadAimingDown] =
        [
            Show(EnemyFrameId.ShaktoolHeadDown, 1914),
        ],
        [P.HeadAimingDownLeft] =
        [
            Show(EnemyFrameId.ShaktoolHeadDownLeft, 1915),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ShaktoolSegment, Animation<ShaktoolSegment>> StartingAt { get; } = new Dictionary<ShaktoolSegment, Animation<ShaktoolSegment>>
    {
        [P.SawHandAttackPrimaryPiece] = new([new(P.SawHandAttackPrimaryPiece, 0, 1, 1), new(P.SawHandPrimaryPiece, 0, 3, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SawHandAttackFinalPiece] = new([new(P.SawHandAttackFinalPiece, 0, 2, 1), new(P.SawHandFinalPiece, 0, 3, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SawHandHeadBobPrimaryPiece] = new([new(P.SawHandHeadBobPrimaryPiece, 0, 1, 1), new(P.SawHandPrimaryPiece, 0, 3, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SawHandHeadBobFinalPiece] = new([new(P.SawHandHeadBobFinalPiece, 0, 2, 1), new(P.SawHandFinalPiece, 0, 3, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SawHandPrimaryPiece] = new([new(P.SawHandPrimaryPiece, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SawHandFinalPiece] = new([new(P.SawHandFinalPiece, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmPieceAttackBack] = new([new(P.ArmPieceAttackBack, 0, 5, 1), new(P.ArmPieceNormal, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ArmPieceAttackFront] = new([new(P.ArmPieceAttackFront, 0, 5, 1), new(P.ArmPieceNormal, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ArmPieceHeadBobBack] = new([new(P.ArmPieceHeadBobBack, 0, 3, 1), new(P.ArmPieceNormal, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ArmPieceHeadBobFront] = new([new(P.ArmPieceHeadBobFront, 0, 5, 1), new(P.ArmPieceNormal, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ArmPieceNormal] = new([new(P.ArmPieceNormal, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HeadAttack] = new([new(P.HeadAttack, 0, 6, 1), new(P.HeadHeadBob, 0, 6, 1), new(P.HeadAimingLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(2)),
        [P.HeadHeadBob] = new([new(P.HeadHeadBob, 0, 6, 1), new(P.HeadAimingLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.HeadAimingLeft] = new([new(P.HeadAimingLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HeadAimingUpLeft] = new([new(P.HeadAimingUpLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HeadAimingUp] = new([new(P.HeadAimingUp, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HeadAimingUpRight] = new([new(P.HeadAimingUpRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HeadAimingRight] = new([new(P.HeadAimingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HeadAimingDownRight] = new([new(P.HeadAimingDownRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HeadAimingDown] = new([new(P.HeadAimingDown, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HeadAimingDownLeft] = new([new(P.HeadAimingDownLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ShaktoolSegment> StartingWith(ShaktoolSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, ShaktoolCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, ShaktoolCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, ShaktoolCue> Cue(ShaktoolCue cue) => new AnimationBeat<EnemyFrameId, ShaktoolCue>.Event(cue);

    /// <summary>Waits <paramref name="updates"/> updates on the current frame.</summary>
    private static AnimationBeat<EnemyFrameId, ShaktoolCue> Wait(int updates) => new AnimationBeat<EnemyFrameId, ShaktoolCue>.Pause(updates);
}
