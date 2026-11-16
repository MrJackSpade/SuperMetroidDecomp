using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;
using SuperMetroid.Core.Enemies.Artwork;
using P = SuperMetroid.Core.Bosses.Draygon.DraygonSegment;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's typed animations (#641, #1600): each body, eye, tail and arms segment's beats, and the animation starting at each.</summary>
internal static class DraygonAnimations
{
    /// <summary>Every body, eye, tail and arms segment's beats.</summary>
    internal static AnimationSegments<DraygonSegment, EnemyFrameId, DraygonCue> Segments { get; } = new(
        new Dictionary<DraygonSegment, AnimationBeat<EnemyFrameId, DraygonCue>[]>
    {
        [P.Rest] =
        [
        ],
        [P.BodyFacingLeftReset] =
        [
            Cue(new DraygonCue.StartPartAnimations(P.BodyFacingLeftIdle, P.EyeFacingLeftIdle, P.TailFacingLeftIdle, P.ArmsFacingLeftIdle)),
            Cue(new DraygonCue.TrackWithEye(HorizontalDirection.Left)),
            Show(F.DraygonBodyFacingLeftJawShutTwin, 1),
        ],
        [P.BodyFacingRightReset] =
        [
            Cue(new DraygonCue.StartPartAnimations(P.BodyFacingRightIdle, P.EyeFacingRightIdle, P.TailFacingRightIdle, P.ArmsFacingRightIdle)),
            Cue(new DraygonCue.TrackWithEye(HorizontalDirection.Right)),
            Show(F.DraygonBodyFacingRightJawShutTwin, 1),
        ],
        [P.ArmsFacingLeftIdle] =
        [
            Show(F.DraygonArmsFacingLeftForward, 5),
            Show(F.DraygonArmsFacingLeftLowering, 5),
            Show(F.DraygonArmsFacingLeftDown, 5),
            Show(F.DraygonArmsFacingLeftDownStretched, 5),
            Show(F.DraygonArmsFacingLeftRaising, 5),
            Show(F.DraygonArmsFacingLeftReaching, 5),
        ],
        [P.ArmsFacingLeftNearSwoopApex] =
        [
            Show(F.DraygonArmsFacingLeftSpreadWide, 1),
            Show(F.DraygonArmsFacingLeftClosing, 1),
            Show(F.DraygonArmsFacingLeftGathering, 1),
            Show(F.DraygonArmsFacingLeftTucked, 64),
        ],
        [P.ArmsFacingLeftGrab] =
        [
            Show(F.DraygonArmsFacingLeftSpreadWide, 1),
            Show(F.DraygonArmsFacingLeftClosing, 1),
            Show(F.DraygonArmsFacingLeftGathering, 1),
            Show(F.DraygonArmsFacingLeftTucked, 8),
            Show(F.DraygonArmsFacingLeftGathering, 1),
            Show(F.DraygonArmsFacingLeftClosing, 1),
            Show(F.DraygonArmsFacingLeftSpreadWide, 1),
            Show(F.DraygonArmsFacingLeftClosing, 1),
        ],
        [P.ArmsFacingLeftDying] =
        [
            Show(F.DraygonLimpArmFacingLeftClawRaised, 5),
            Show(F.DraygonLimpArmFacingLeftClawDropping, 5),
            Show(F.DraygonLimpArmFacingLeftClawDropped, 5),
            Show(F.DraygonLimpArmFacingLeftClawCurled, 5),
        ],
        [P.BodyFacingLeftIdle] =
        [
            Cue(new DraygonCue.TrackWithEye(HorizontalDirection.Left)),
            Show(F.DraygonBodyFacingLeftJawShutTwin, 1),
        ],
        [P.BodyDying] =
        [
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.DyingCry)),
            Cue(DraygonCue.SetBodyIntangible.Instance),
        ],
        [P.BodyDyingExplosions] =
        [
            Wait(12),
            Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.BigExplosion)),
            Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.SmallExplosion)),
            Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.BigDustCloud)),
            Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.DraygonBreathBubbles)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Wait(1),
            Cue(DraygonCue.ParalyseTailAndArms.Instance),
            Wait(16),
            Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.BigExplosion)),
            Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.SmallExplosion)),
            Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.BigDustCloud)),
            Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.DraygonBreathBubbles)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
        ],
        [P.Delete] =
        [
        ],
        [P.BodyFacingLeftFireGoop] =
        [
            Show(F.DraygonBodyFacingLeftJawShut, 1),
            Show(F.DraygonBodyFacingLeftJawParting, 2),
            Show(F.DraygonBodyFacingLeftJawOpen, 3),
            Cue(new DraygonCue.SpawnGoop(HorizontalDirection.Left)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.GoopSpit)),
            Show(F.DraygonBodyFacingLeftJawWide, 3),
            Show(F.DraygonBodyFacingLeftJawOpen, 2),
            Show(F.DraygonBodyFacingLeftJawParting, 2),
            Show(F.DraygonBodyFacingLeftJawShut, 1),
        ],
        [P.BodyFacingLeftRoar] =
        [
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Roar)),
            Show(F.DraygonBodyFacingLeftJawShut, 6),
            Show(F.DraygonBodyFacingLeftJawParting, 6),
            Show(F.DraygonBodyFacingLeftJawOpen, 6),
            Show(F.DraygonBodyFacingLeftJawWide, 6),
            Show(F.DraygonBodyFacingLeftJawOpen, 6),
            Show(F.DraygonBodyFacingLeftJawParting, 6),
            Show(F.DraygonBodyFacingLeftJawShut, 6),
        ],
        [P.EyeFacingLeftIdle] =
        [
            Show(F.DraygonEyeFacingLeftShut, 21),
            Show(F.DraygonEyeFacingLeftOpening, 5),
            Show(F.DraygonEyeFacingLeftHalfOpen, 5),
            Show(F.DraygonEyeFacingLeftOpening, 10),
            Show(F.DraygonEyeFacingLeftOpen, 10),
            Show(F.DraygonEyeFacingLeftOpen, 10),
            Show(F.DraygonEyeFacingLeftNarrowing, 10),
            Show(F.DraygonEyeFacingLeftNarrowing, 10),
            Show(F.DraygonEyeFacingLeftClosing, 10),
            Show(F.DraygonEyeFacingLeftHalfOpen, 5),
            Show(F.DraygonEyeFacingLeftOpening, 5),
            Show(F.DraygonEyeFacingLeftShut, 5),
            Cue(new DraygonCue.TrackWithOwnEye(HorizontalDirection.Left)),
        ],
        [P.EyeFacingLeftDying] =
        [
            Show(F.DraygonEyeFacingLeftOpen, 4),
            Show(F.DraygonEyeFacingLeftNarrowing, 4),
            Show(F.DraygonEyeFacingLeftBloodshot, 4),
            Show(F.DraygonEyeFacingLeftClosing, 4),
            Show(F.DraygonEyeFacingLeftWide, 32),
            Show(F.DraygonEyeFacingLeftHalfOpen, 16),
        ],
        [P.EyeFacingLeftDead] =
        [
            Show(F.DraygonEyeFacingLeftWide, 32),
            Show(F.DraygonEyeFacingLeftHalfOpen, 32),
            Show(F.DraygonEyeFacingLeftOpening, 32),
            Show(F.DraygonEyeFacingLeftShut, 1),
        ],
        [P.EyeFacingLeftLookingLeft] =
        [
            Show(F.DraygonEyeFacingLeftOpen, 1),
        ],
        [P.EyeFacingLeftLookingRight] =
        [
            Show(F.DraygonEyeFacingLeftBloodshot, 1),
        ],
        [P.EyeFacingLeftLookingUp] =
        [
            Show(F.DraygonEyeFacingLeftNarrowing, 1),
        ],
        [P.EyeFacingLeftLookingDown] =
        [
            Show(F.DraygonEyeFacingLeftClosing, 1),
        ],
        [P.TailFacingLeftIdle] =
        [
            Show(F.DraygonTailFacingLeftTipCurledIn, 8),
            Show(F.DraygonTailFacingLeftTipSwingingOut, 7),
            Show(F.DraygonTailFacingLeftTipOut, 6),
            Show(F.DraygonTailFacingLeftTipFurtherOut, 6),
            Show(F.DraygonTailFacingLeftTipFarthestOut, 6),
            Show(F.DraygonTailFacingLeftTipReturning, 6),
            Show(F.DraygonTailFacingLeftTipBack, 6),
            Show(F.DraygonTailFacingLeftTipReturning, 6),
            Show(F.DraygonTailFacingLeftTipFarthestOut, 6),
            Show(F.DraygonTailFacingLeftTipFurtherOut, 6),
            Show(F.DraygonTailFacingLeftTipOut, 6),
            Show(F.DraygonTailFacingLeftTipSwingingOut, 7),
        ],
        [P.TailFacingLeftInitialFakeWhip] =
        [
            Cue(new DraygonCue.ShiftBodyPicture(new(1, 1))),
            Show(F.DraygonTailFacingLeftTipOut, 16),
            Cue(new DraygonCue.ShiftBodyPicture(new(2, 2))),
            Show(F.DraygonTailFacingLeftWhipCocking, 6),
            Cue(new DraygonCue.ShiftBodyPicture(new(3, 3))),
            Show(F.DraygonTailFacingLeftWhipRaising, 5),
            Cue(new DraygonCue.ShiftBodyPicture(new(4, 4))),
            Show(F.DraygonTailFacingLeftWhipHangingLong, 4),
            Cue(new DraygonCue.ShiftBodyPicture(new(5, 5))),
            Show(F.DraygonTailFacingLeftWhipSwinging, 3),
            Cue(new DraygonCue.ShiftBodyPicture(new(6, 6))),
            Show(F.DraygonTailFacingLeftWhipLashedOut, 2),
            Cue(new DraygonCue.ShiftBodyPicture(new(8, 8))),
            Show(F.DraygonTailFacingLeftWhipCurling, 1),
            Cue(new DraygonCue.ShiftBodyPicture(new(0, 0))),
            Show(F.DraygonTailFacingLeftWhipRecoiling, 16),
            Show(F.DraygonTailFacingLeftWhipCurling, 1),
            Show(F.DraygonTailFacingLeftWhipLashedOut, 2),
            Show(F.DraygonTailFacingLeftWhipSwinging, 3),
            Show(F.DraygonTailFacingLeftWhipHangingLong, 4),
            Show(F.DraygonTailFacingLeftWhipRaising, 5),
            Show(F.DraygonTailFacingLeftWhipCocking, 6),
        ],
        [P.TailFacingLeftFinalWhips] =
        [
            Cue(new DraygonCue.ShiftBodyPicture(new(1, 1))),
            Show(F.DraygonTailFacingLeftTipOut, 2),
            Cue(new DraygonCue.ShiftBodyPicture(new(2, 2))),
            Show(F.DraygonTailFacingLeftWhipCocking, 6),
            Cue(new DraygonCue.ShiftBodyPicture(new(3, 3))),
            Show(F.DraygonTailFacingLeftWhipRaising, 5),
            Cue(new DraygonCue.ShiftBodyPicture(new(4, 4))),
            Show(F.DraygonTailFacingLeftWhipHangingLong, 4),
            Cue(new DraygonCue.ShiftBodyPicture(new(5, 5))),
            Show(F.DraygonTailFacingLeftWhipSwinging, 3),
            Cue(new DraygonCue.ShiftBodyPicture(new(6, 6))),
            Show(F.DraygonTailFacingLeftWhipLashedOut, 2),
            Cue(new DraygonCue.ShiftBodyPicture(new(8, 8))),
            Show(F.DraygonTailFacingLeftWhipCurling, 1),
            Cue(new DraygonCue.ShiftBodyPicture(new(0, 0))),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(F.DraygonTailFacingLeftWhipRecoiling, 3),
            Show(F.DraygonTailFacingLeftWhipCurling, 1),
            Show(F.DraygonTailFacingLeftWhipLashedOut, 2),
            Show(F.DraygonTailFacingLeftWhipSwinging, 3),
            Show(F.DraygonTailFacingLeftWhipHangingLong, 4),
            Show(F.DraygonTailFacingLeftWhipRaising, 5),
            Show(F.DraygonTailFacingLeftWhipCocking, 6),
            Cue(new DraygonCue.SetBodyPhase(DraygonPhase.ReleaseSamus)),
        ],
        [P.TailFacingLeftWhip] =
        [
            Cue(new DraygonCue.ShiftBodyPicture(new(1, 1))),
            Show(F.DraygonTailFacingLeftTipOut, 2),
            Cue(new DraygonCue.ShiftBodyPicture(new(2, 2))),
            Show(F.DraygonTailFacingLeftWhipCocking, 6),
            Cue(new DraygonCue.ShiftBodyPicture(new(3, 3))),
            Show(F.DraygonTailFacingLeftWhipRaising, 5),
            Cue(new DraygonCue.ShiftBodyPicture(new(4, 4))),
            Show(F.DraygonTailFacingLeftWhipHangingLong, 4),
            Cue(new DraygonCue.ShiftBodyPicture(new(5, 5))),
            Show(F.DraygonTailFacingLeftWhipSwinging, 3),
            Cue(new DraygonCue.ShiftBodyPicture(new(6, 6))),
            Show(F.DraygonTailFacingLeftWhipLashedOut, 2),
            Cue(new DraygonCue.ShiftBodyPicture(new(8, 8))),
            Show(F.DraygonTailFacingLeftWhipCurling, 1),
            Cue(new DraygonCue.ShiftBodyPicture(new(0, 0))),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(F.DraygonTailFacingLeftWhipRecoiling, 3),
            Show(F.DraygonTailFacingLeftWhipCurling, 1),
            Show(F.DraygonTailFacingLeftWhipLashedOut, 2),
            Show(F.DraygonTailFacingLeftWhipSwinging, 3),
            Show(F.DraygonTailFacingLeftWhipHangingLong, 4),
            Show(F.DraygonTailFacingLeftWhipRaising, 5),
            Show(F.DraygonTailFacingLeftWhipCocking, 6),
        ],
        [P.TailFacingLeftFlail] =
        [
            Show(F.DraygonTailFacingLeftTipOut, 2),
            Show(F.DraygonTailFacingLeftWhipCocking, 6),
            Show(F.DraygonTailFacingLeftWhipRaising, 5),
            Show(F.DraygonTailFacingLeftWhipHangingLong, 4),
            Show(F.DraygonTailFacingLeftWhipSwinging, 3),
            Show(F.DraygonTailFacingLeftWhipLashedOut, 2),
            Show(F.DraygonTailFacingLeftWhipCurling, 1),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(F.DraygonTailFacingLeftWhipRecoiling, 3),
            Show(F.DraygonTailFacingLeftWhipCurling, 1),
            Show(F.DraygonTailFacingLeftWhipLashedOut, 2),
            Show(F.DraygonTailFacingLeftWhipSwinging, 3),
            Show(F.DraygonTailFacingLeftWhipHangingLong, 4),
            Show(F.DraygonTailFacingLeftWhipRaising, 5),
            Show(F.DraygonTailFacingLeftWhipCocking, 6),
        ],
        [P.ArmsFacingRightIdle] =
        [
            Show(F.DraygonArmsFacingRightForward, 5),
            Show(F.DraygonArmsFacingRightLowering, 5),
            Show(F.DraygonArmsFacingRightDown, 5),
            Show(F.DraygonArmsFacingRightDownStretched, 5),
            Show(F.DraygonArmsFacingRightRaising, 5),
            Show(F.DraygonArmsFacingRightReaching, 5),
        ],
        [P.ArmsFacingRightNearSwoopApex] =
        [
            Show(F.DraygonArmsFacingRightSpreadWide, 1),
            Show(F.DraygonArmsFacingRightClosing, 1),
            Show(F.DraygonArmsFacingRightGathering, 1),
            Show(F.DraygonArmsFacingRightTucked, 64),
        ],
        [P.ArmsFacingRightGrab] =
        [
            Show(F.DraygonArmsFacingRightSpreadWide, 1),
            Show(F.DraygonArmsFacingRightClosing, 1),
            Show(F.DraygonArmsFacingRightGathering, 1),
            Show(F.DraygonArmsFacingRightTucked, 8),
            Show(F.DraygonArmsFacingRightGathering, 1),
            Show(F.DraygonArmsFacingRightClosing, 1),
            Show(F.DraygonArmsFacingRightSpreadWide, 1),
            Show(F.DraygonArmsFacingRightClosing, 1),
        ],
        [P.ArmsFacingRightDying] =
        [
            Show(F.DraygonLimpArmFacingRightClawRaised, 5),
            Show(F.DraygonLimpArmFacingRightClawDropping, 5),
            Show(F.DraygonLimpArmFacingRightClawDropped, 5),
            Show(F.DraygonLimpArmFacingRightClawCurled, 5),
        ],
        [P.BodyFacingRightIdle] =
        [
            Cue(new DraygonCue.TrackWithEye(HorizontalDirection.Right)),
            Show(F.DraygonBodyFacingRightJawShutTwin, 1),
        ],
        [P.BodyFacingRightFireGoop] =
        [
            Show(F.DraygonBodyFacingRightJawShut, 1),
            Show(F.DraygonBodyFacingRightJawParting, 2),
            Show(F.DraygonBodyFacingRightJawOpen, 3),
            Cue(new DraygonCue.SpawnGoop(HorizontalDirection.Right)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.GoopSpit)),
            Show(F.DraygonBodyFacingRightJawWide, 3),
            Show(F.DraygonBodyFacingRightJawOpen, 2),
            Show(F.DraygonBodyFacingRightJawParting, 2),
            Show(F.DraygonBodyFacingRightJawShut, 1),
        ],
        [P.BodyFacingRightRoar] =
        [
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Roar)),
            Show(F.DraygonBodyFacingRightJawShut, 6),
            Show(F.DraygonBodyFacingRightJawParting, 6),
            Show(F.DraygonBodyFacingRightJawOpen, 6),
            Show(F.DraygonBodyFacingRightJawWide, 6),
            Show(F.DraygonBodyFacingRightJawOpen, 6),
            Show(F.DraygonBodyFacingRightJawParting, 6),
            Show(F.DraygonBodyFacingRightJawShut, 6),
        ],
        [P.EyeFacingRightIdle] =
        [
            Show(F.DraygonEyeFacingRightShut, 21),
            Show(F.DraygonEyeFacingRightOpening, 5),
            Show(F.DraygonEyeFacingRightHalfOpen, 5),
            Show(F.DraygonEyeFacingRightOpening, 10),
            Show(F.DraygonEyeFacingRightOpen, 10),
            Show(F.DraygonEyeFacingRightOpen, 10),
            Show(F.DraygonEyeFacingRightNarrowing, 10),
            Show(F.DraygonEyeFacingRightNarrowing, 10),
            Show(F.DraygonEyeFacingRightClosing, 10),
            Show(F.DraygonEyeFacingRightHalfOpen, 5),
            Show(F.DraygonEyeFacingRightOpening, 5),
            Show(F.DraygonEyeFacingRightShut, 5),
            Cue(new DraygonCue.TrackWithOwnEye(HorizontalDirection.Left)),
        ],
        [P.EyeFacingRightDying] =
        [
            Show(F.DraygonEyeFacingRightOpen, 4),
            Show(F.DraygonEyeFacingRightNarrowing, 4),
            Show(F.DraygonEyeFacingRightBloodshot, 4),
            Show(F.DraygonEyeFacingRightClosing, 4),
            Show(F.DraygonEyeFacingRightWide, 32),
            Show(F.DraygonEyeFacingRightHalfOpen, 16),
        ],
        [P.EyeFacingRightDead] =
        [
            Show(F.DraygonEyeFacingRightWide, 32),
            Show(F.DraygonEyeFacingRightHalfOpen, 32),
            Show(F.DraygonEyeFacingRightOpening, 32),
            Show(F.DraygonEyeFacingRightShut, 1),
        ],
        [P.EyeFacingRightLookingRight] =
        [
            Show(F.DraygonEyeFacingRightOpen, 1),
        ],
        [P.EyeFacingRightLookingLeft] =
        [
            Show(F.DraygonEyeFacingRightBloodshot, 1),
        ],
        [P.EyeFacingRightLookingUp] =
        [
            Show(F.DraygonEyeFacingRightNarrowing, 1),
        ],
        [P.EyeFacingRightLookingDown] =
        [
            Show(F.DraygonEyeFacingRightClosing, 1),
        ],
        [P.TailFacingRightIdle] =
        [
            Show(F.DraygonTailFacingRightTipCurledIn, 8),
            Show(F.DraygonTailFacingRightTipSwingingOut, 7),
            Show(F.DraygonTailFacingRightTipOut, 6),
            Show(F.DraygonTailFacingRightTipFurtherOut, 6),
            Show(F.DraygonTailFacingRightTipFarthestOut, 6),
            Show(F.DraygonTailFacingRightTipReturning, 6),
            Show(F.DraygonTailFacingRightTipBack, 6),
            Show(F.DraygonTailFacingRightTipReturning, 6),
            Show(F.DraygonTailFacingRightTipFarthestOut, 6),
            Show(F.DraygonTailFacingRightTipFurtherOut, 6),
            Show(F.DraygonTailFacingRightTipOut, 6),
            Show(F.DraygonTailFacingRightTipSwingingOut, 7),
        ],
        [P.TailFacingRightFinalWhips] =
        [
            Cue(new DraygonCue.ShiftBodyPicture(new(-1, 1))),
            Show(F.DraygonTailFacingRightTipOut, 2),
            Cue(new DraygonCue.ShiftBodyPicture(new(-2, 2))),
            Show(F.DraygonTailFacingRightWhipCocking, 6),
            Cue(new DraygonCue.ShiftBodyPicture(new(-3, 3))),
            Show(F.DraygonTailFacingRightWhipRaising, 5),
            Cue(new DraygonCue.ShiftBodyPicture(new(-4, 4))),
            Show(F.DraygonTailFacingRightWhipHangingLong, 4),
            Cue(new DraygonCue.ShiftBodyPicture(new(-5, 5))),
            Show(F.DraygonTailFacingRightWhipSwinging, 3),
            Cue(new DraygonCue.ShiftBodyPicture(new(-6, 6))),
            Show(F.DraygonTailFacingRightWhipLashedOut, 2),
            Cue(new DraygonCue.ShiftBodyPicture(new(-8, 8))),
            Show(F.DraygonTailFacingRightWhipCurling, 1),
            Cue(new DraygonCue.ShiftBodyPicture(new(0, 0))),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(F.DraygonTailFacingRightWhipRecoiling, 3),
            Show(F.DraygonTailFacingRightWhipCurling, 1),
            Show(F.DraygonTailFacingRightWhipLashedOut, 2),
            Show(F.DraygonTailFacingRightWhipSwinging, 3),
            Show(F.DraygonTailFacingRightWhipHangingLong, 4),
            Show(F.DraygonTailFacingRightWhipRaising, 5),
            Show(F.DraygonTailFacingRightWhipCocking, 6),
            Cue(new DraygonCue.SetBodyPhase(DraygonPhase.ReleaseSamus)),
        ],
        [P.TailFacingRightWhip] =
        [
            Cue(new DraygonCue.ShiftBodyPicture(new(-1, 1))),
            Show(F.DraygonTailFacingRightTipOut, 2),
            Cue(new DraygonCue.ShiftBodyPicture(new(-2, 2))),
            Show(F.DraygonTailFacingRightWhipCocking, 6),
            Cue(new DraygonCue.ShiftBodyPicture(new(-3, 3))),
            Show(F.DraygonTailFacingRightWhipRaising, 5),
            Cue(new DraygonCue.ShiftBodyPicture(new(-4, 4))),
            Show(F.DraygonTailFacingRightWhipHangingLong, 4),
            Cue(new DraygonCue.ShiftBodyPicture(new(-5, 5))),
            Show(F.DraygonTailFacingRightWhipSwinging, 3),
            Cue(new DraygonCue.ShiftBodyPicture(new(-6, 6))),
            Show(F.DraygonTailFacingRightWhipLashedOut, 2),
            Cue(new DraygonCue.ShiftBodyPicture(new(-8, 8))),
            Show(F.DraygonTailFacingRightWhipCurling, 1),
            Cue(new DraygonCue.ShiftBodyPicture(new(0, 0))),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(F.DraygonTailFacingRightWhipRecoiling, 3),
            Show(F.DraygonTailFacingRightWhipCurling, 1),
            Show(F.DraygonTailFacingRightWhipLashedOut, 2),
            Show(F.DraygonTailFacingRightWhipSwinging, 3),
            Show(F.DraygonTailFacingRightWhipHangingLong, 4),
            Show(F.DraygonTailFacingRightWhipRaising, 5),
            Show(F.DraygonTailFacingRightWhipCocking, 6),
        ],
        [P.TailFacingRightFlail] =
        [
            Show(F.DraygonTailFacingRightTipOut, 2),
            Show(F.DraygonTailFacingRightWhipCocking, 6),
            Show(F.DraygonTailFacingRightWhipRaising, 5),
            Show(F.DraygonTailFacingRightWhipHangingLong, 4),
            Show(F.DraygonTailFacingRightWhipSwinging, 3),
            Show(F.DraygonTailFacingRightWhipLashedOut, 2),
            Show(F.DraygonTailFacingRightWhipCurling, 1),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(F.DraygonTailFacingRightWhipRecoiling, 3),
            Show(F.DraygonTailFacingRightWhipCurling, 1),
            Show(F.DraygonTailFacingRightWhipLashedOut, 2),
            Show(F.DraygonTailFacingRightWhipSwinging, 3),
            Show(F.DraygonTailFacingRightWhipHangingLong, 4),
            Show(F.DraygonTailFacingRightWhipRaising, 5),
            Show(F.DraygonTailFacingRightWhipCocking, 6),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DraygonSegment, Animation<DraygonSegment>> StartingAt { get; } = new Dictionary<DraygonSegment, Animation<DraygonSegment>>
    {
        [P.Rest] = new([], AnimationEnd.Holds),
        [P.BodyFacingLeftReset] = new([new(P.BodyFacingLeftReset, 0, 3, 1)], AnimationEnd.Holds),
        [P.BodyFacingRightReset] = new([new(P.BodyFacingRightReset, 0, 3, 1)], AnimationEnd.Holds),
        [P.ArmsFacingLeftIdle] = new([new(P.ArmsFacingLeftIdle, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmsFacingLeftNearSwoopApex] = new([new(P.ArmsFacingLeftNearSwoopApex, 0, 4, 1)], AnimationEnd.Holds),
        [P.ArmsFacingLeftGrab] = new([new(P.ArmsFacingLeftGrab, 0, 8, 1)], AnimationEnd.Holds),
        [P.ArmsFacingLeftDying] = new([new(P.ArmsFacingLeftDying, 0, 4, 1), new(P.BodyDying, 0, 2, 1), new(P.BodyDyingExplosions, 0, 6, 8), new(P.BodyDyingExplosions, 6, 2, 1), new(P.BodyDyingExplosions, 8, 6, 1)], AnimationEnd.LoopsFrom(4)),
        [P.BodyFacingLeftIdle] = new([new(P.BodyFacingLeftIdle, 0, 2, 1)], AnimationEnd.Holds),
        [P.BodyDying] = new([new(P.BodyDying, 0, 2, 1), new(P.BodyDyingExplosions, 0, 6, 8), new(P.BodyDyingExplosions, 6, 2, 1), new(P.BodyDyingExplosions, 8, 6, 1)], AnimationEnd.LoopsFrom(3)),
        [P.Delete] = new([], AnimationEnd.Stops),
        [P.BodyFacingLeftFireGoop] = new([new(P.BodyFacingLeftFireGoop, 0, 9, 1)], AnimationEnd.Holds),
        [P.BodyFacingLeftRoar] = new([new(P.BodyFacingLeftRoar, 0, 8, 1)], AnimationEnd.Holds),
        [P.EyeFacingLeftIdle] = new([new(P.EyeFacingLeftIdle, 0, 13, 1)], AnimationEnd.Holds),
        [P.EyeFacingLeftDying] = new([new(P.EyeFacingLeftDying, 0, 4, 4), new(P.EyeFacingLeftDying, 4, 2, 1)], AnimationEnd.Holds),
        [P.EyeFacingLeftDead] = new([new(P.EyeFacingLeftDead, 0, 4, 1)], AnimationEnd.Holds),
        [P.EyeFacingLeftLookingLeft] = new([new(P.EyeFacingLeftLookingLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.EyeFacingLeftLookingRight] = new([new(P.EyeFacingLeftLookingRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.EyeFacingLeftLookingUp] = new([new(P.EyeFacingLeftLookingUp, 0, 1, 1)], AnimationEnd.Holds),
        [P.EyeFacingLeftLookingDown] = new([new(P.EyeFacingLeftLookingDown, 0, 1, 1)], AnimationEnd.Holds),
        [P.TailFacingLeftIdle] = new([new(P.TailFacingLeftIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TailFacingLeftInitialFakeWhip] = new([new(P.TailFacingLeftInitialFakeWhip, 0, 22, 1), new(P.TailFacingLeftIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TailFacingLeftFinalWhips] = new([new(P.TailFacingLeftFinalWhips, 0, 24, 4), new(P.TailFacingLeftFinalWhips, 24, 1, 1), new(P.TailFacingLeftIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(2)),
        [P.TailFacingLeftWhip] = new([new(P.TailFacingLeftWhip, 0, 24, 1), new(P.TailFacingLeftIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TailFacingLeftFlail] = new([new(P.TailFacingLeftFlail, 0, 15, 1), new(P.TailFacingLeftIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ArmsFacingRightIdle] = new([new(P.ArmsFacingRightIdle, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmsFacingRightNearSwoopApex] = new([new(P.ArmsFacingRightNearSwoopApex, 0, 4, 1)], AnimationEnd.Holds),
        [P.ArmsFacingRightGrab] = new([new(P.ArmsFacingRightGrab, 0, 8, 1)], AnimationEnd.Holds),
        [P.ArmsFacingRightDying] = new([new(P.ArmsFacingRightDying, 0, 4, 1), new(P.BodyDying, 0, 2, 1), new(P.BodyDyingExplosions, 0, 6, 8), new(P.BodyDyingExplosions, 6, 2, 1), new(P.BodyDyingExplosions, 8, 6, 1)], AnimationEnd.LoopsFrom(4)),
        [P.BodyFacingRightIdle] = new([new(P.BodyFacingRightIdle, 0, 2, 1)], AnimationEnd.Holds),
        [P.BodyFacingRightFireGoop] = new([new(P.BodyFacingRightFireGoop, 0, 9, 1)], AnimationEnd.Holds),
        [P.BodyFacingRightRoar] = new([new(P.BodyFacingRightRoar, 0, 8, 1)], AnimationEnd.Holds),
        [P.EyeFacingRightIdle] = new([new(P.EyeFacingRightIdle, 0, 13, 1)], AnimationEnd.Holds),
        [P.EyeFacingRightDying] = new([new(P.EyeFacingRightDying, 0, 4, 4), new(P.EyeFacingRightDying, 4, 2, 1)], AnimationEnd.Holds),
        [P.EyeFacingRightDead] = new([new(P.EyeFacingRightDead, 0, 4, 1)], AnimationEnd.Holds),
        [P.EyeFacingRightLookingRight] = new([new(P.EyeFacingRightLookingRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.EyeFacingRightLookingLeft] = new([new(P.EyeFacingRightLookingLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.EyeFacingRightLookingUp] = new([new(P.EyeFacingRightLookingUp, 0, 1, 1)], AnimationEnd.Holds),
        [P.EyeFacingRightLookingDown] = new([new(P.EyeFacingRightLookingDown, 0, 1, 1)], AnimationEnd.Holds),
        [P.TailFacingRightIdle] = new([new(P.TailFacingRightIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TailFacingRightFinalWhips] = new([new(P.TailFacingRightFinalWhips, 0, 24, 4), new(P.TailFacingRightFinalWhips, 24, 1, 1), new(P.TailFacingRightIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(2)),
        [P.TailFacingRightWhip] = new([new(P.TailFacingRightWhip, 0, 24, 1), new(P.TailFacingRightIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TailFacingRightFlail] = new([new(P.TailFacingRightFlail, 0, 15, 1), new(P.TailFacingRightIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DraygonSegment> StartingWith(DraygonSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue> Show(EnemyFrameId frame, int duration) =>
        new AnimationBeat<EnemyFrameId, DraygonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue> Cue(DraygonCue cue) =>
        new AnimationBeat<EnemyFrameId, DraygonCue>.Event(cue);

    /// <summary>Keeps the current frame for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue> Wait(int duration) =>
        new AnimationBeat<EnemyFrameId, DraygonCue>.Pause(duration);
}
