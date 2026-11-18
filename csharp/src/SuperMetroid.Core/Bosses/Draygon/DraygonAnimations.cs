using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;
using SuperMetroid.Core.Enemies.Artwork;
using P = SuperMetroid.Core.Bosses.Draygon.DraygonSegment;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's typed animations (#641, #1600): each body, eye, tail and arms segment's beats, the animation starting at
/// each, which plays each of its segments whole (#2296), and his tail's final whips, which play its whip four times and
/// which his grab starts by name (#2399). The idle eye shows its open and narrowing pictures twice in a row
/// each, and the two showings stay two beats: each raises the eye's new-frame flag, which his body reads to show the eye's
/// picture on an update the draw pass reaches the eye, so the second showing shows it again where the first was missed.
/// </summary>
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
            Show(F.DraygonBodyFacingLeftJawShut, 1),
        ],
        [P.BodyFacingRightReset] =
        [
            Cue(new DraygonCue.StartPartAnimations(P.BodyFacingRightIdle, P.EyeFacingRightIdle, P.TailFacingRightIdle, P.ArmsFacingRightIdle)),
            Cue(new DraygonCue.TrackWithEye(HorizontalDirection.Right)),
            Show(F.DraygonBodyFacingRightJawShut, 1),
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
            Show(F.DraygonBodyFacingLeftJawShut, 1),
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
        ],
        [P.BodyDyingParalysis] =
        [
            Wait(1),
            Cue(DraygonCue.ParalyseTailAndArms.Instance),
        ],
        [P.BodyDyingExplosionLoop] =
        [
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
            Cue(new DraygonCue.TrackWithEye(HorizontalDirection.Left)),
        ],
        [P.EyeFacingLeftDying] =
        [
            Show(F.DraygonEyeFacingLeftOpen, 4),
            Show(F.DraygonEyeFacingLeftNarrowing, 4),
            Show(F.DraygonEyeFacingLeftBloodshot, 4),
            Show(F.DraygonEyeFacingLeftClosing, 4),
        ],
        [P.EyeFacingLeftDyingStare] =
        [
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
        [P.TailFacingLeftReleaseSamus] =
        [
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
            Show(F.DraygonBodyFacingRightJawShut, 1),
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
            Cue(new DraygonCue.TrackWithEye(HorizontalDirection.Left)),
        ],
        [P.EyeFacingRightDying] =
        [
            Show(F.DraygonEyeFacingRightOpen, 4),
            Show(F.DraygonEyeFacingRightNarrowing, 4),
            Show(F.DraygonEyeFacingRightBloodshot, 4),
            Show(F.DraygonEyeFacingRightClosing, 4),
        ],
        [P.EyeFacingRightDyingStare] =
        [
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
        [P.TailFacingRightReleaseSamus] =
        [
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
        [P.BodyFacingLeftReset] = new([Segments.Whole(P.BodyFacingLeftReset)], AnimationEnd.Holds),
        [P.BodyFacingRightReset] = new([Segments.Whole(P.BodyFacingRightReset)], AnimationEnd.Holds),
        [P.ArmsFacingLeftIdle] = Animation<DraygonSegment>.Looping([Segments.Whole(P.ArmsFacingLeftIdle)]),
        [P.ArmsFacingLeftNearSwoopApex] = new([Segments.Whole(P.ArmsFacingLeftNearSwoopApex)], AnimationEnd.Holds),
        [P.ArmsFacingLeftGrab] = new([Segments.Whole(P.ArmsFacingLeftGrab)], AnimationEnd.Holds),
        [P.ArmsFacingLeftDying] = Animation<DraygonSegment>.LeadingInto([Segments.Whole(P.ArmsFacingLeftDying), Segments.Whole(P.BodyDying), Segments.Repeated(P.BodyDyingExplosions, 8), Segments.Whole(P.BodyDyingParalysis)], [Segments.Whole(P.BodyDyingExplosionLoop)]),
        [P.BodyFacingLeftIdle] = new([Segments.Whole(P.BodyFacingLeftIdle)], AnimationEnd.Holds),
        [P.Delete] = new([], AnimationEnd.Stops),
        [P.BodyFacingLeftFireGoop] = new([Segments.Whole(P.BodyFacingLeftFireGoop)], AnimationEnd.Holds),
        [P.BodyFacingLeftRoar] = new([Segments.Whole(P.BodyFacingLeftRoar)], AnimationEnd.Holds),
        [P.EyeFacingLeftIdle] = new([Segments.Whole(P.EyeFacingLeftIdle)], AnimationEnd.Holds),
        [P.EyeFacingLeftDying] = new([Segments.Repeated(P.EyeFacingLeftDying, 4), Segments.Whole(P.EyeFacingLeftDyingStare)], AnimationEnd.Holds),
        [P.EyeFacingLeftDead] = new([Segments.Whole(P.EyeFacingLeftDead)], AnimationEnd.Holds),
        [P.EyeFacingLeftLookingLeft] = new([Segments.Whole(P.EyeFacingLeftLookingLeft)], AnimationEnd.Holds),
        [P.EyeFacingLeftLookingRight] = new([Segments.Whole(P.EyeFacingLeftLookingRight)], AnimationEnd.Holds),
        [P.EyeFacingLeftLookingUp] = new([Segments.Whole(P.EyeFacingLeftLookingUp)], AnimationEnd.Holds),
        [P.EyeFacingLeftLookingDown] = new([Segments.Whole(P.EyeFacingLeftLookingDown)], AnimationEnd.Holds),
        [P.TailFacingLeftIdle] = Animation<DraygonSegment>.Looping([Segments.Whole(P.TailFacingLeftIdle)]),
        [P.TailFacingLeftInitialFakeWhip] = Animation<DraygonSegment>.LeadingInto([Segments.Whole(P.TailFacingLeftInitialFakeWhip)], [Segments.Whole(P.TailFacingLeftIdle)]),
        [P.TailFacingLeftWhip] = Animation<DraygonSegment>.LeadingInto([Segments.Whole(P.TailFacingLeftWhip)], [Segments.Whole(P.TailFacingLeftIdle)]),
        [P.TailFacingLeftFlail] = Animation<DraygonSegment>.LeadingInto([Segments.Whole(P.TailFacingLeftFlail)], [Segments.Whole(P.TailFacingLeftIdle)]),
        [P.ArmsFacingRightIdle] = Animation<DraygonSegment>.Looping([Segments.Whole(P.ArmsFacingRightIdle)]),
        [P.ArmsFacingRightNearSwoopApex] = new([Segments.Whole(P.ArmsFacingRightNearSwoopApex)], AnimationEnd.Holds),
        [P.ArmsFacingRightGrab] = new([Segments.Whole(P.ArmsFacingRightGrab)], AnimationEnd.Holds),
        [P.ArmsFacingRightDying] = Animation<DraygonSegment>.LeadingInto([Segments.Whole(P.ArmsFacingRightDying), Segments.Whole(P.BodyDying), Segments.Repeated(P.BodyDyingExplosions, 8), Segments.Whole(P.BodyDyingParalysis)], [Segments.Whole(P.BodyDyingExplosionLoop)]),
        [P.BodyFacingRightIdle] = new([Segments.Whole(P.BodyFacingRightIdle)], AnimationEnd.Holds),
        [P.BodyFacingRightFireGoop] = new([Segments.Whole(P.BodyFacingRightFireGoop)], AnimationEnd.Holds),
        [P.BodyFacingRightRoar] = new([Segments.Whole(P.BodyFacingRightRoar)], AnimationEnd.Holds),
        [P.EyeFacingRightIdle] = new([Segments.Whole(P.EyeFacingRightIdle)], AnimationEnd.Holds),
        [P.EyeFacingRightDying] = new([Segments.Repeated(P.EyeFacingRightDying, 4), Segments.Whole(P.EyeFacingRightDyingStare)], AnimationEnd.Holds),
        [P.EyeFacingRightDead] = new([Segments.Whole(P.EyeFacingRightDead)], AnimationEnd.Holds),
        [P.EyeFacingRightLookingRight] = new([Segments.Whole(P.EyeFacingRightLookingRight)], AnimationEnd.Holds),
        [P.EyeFacingRightLookingLeft] = new([Segments.Whole(P.EyeFacingRightLookingLeft)], AnimationEnd.Holds),
        [P.EyeFacingRightLookingUp] = new([Segments.Whole(P.EyeFacingRightLookingUp)], AnimationEnd.Holds),
        [P.EyeFacingRightLookingDown] = new([Segments.Whole(P.EyeFacingRightLookingDown)], AnimationEnd.Holds),
        [P.TailFacingRightIdle] = Animation<DraygonSegment>.Looping([Segments.Whole(P.TailFacingRightIdle)]),
        [P.TailFacingRightWhip] = Animation<DraygonSegment>.LeadingInto([Segments.Whole(P.TailFacingRightWhip)], [Segments.Whole(P.TailFacingRightIdle)]),
        [P.TailFacingRightFlail] = Animation<DraygonSegment>.LeadingInto([Segments.Whole(P.TailFacingRightFlail)], [Segments.Whole(P.TailFacingRightIdle)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DraygonSegment> StartingWith(DraygonSegment segment) => StartingAt[segment];

    /// <summary>How many times his tail whips in its final whips before it releases Samus.</summary>
    private const int FinalWhips = 4;

    /// <summary>
    /// His tail's final whips facing left (#2399): his whip played <see cref="FinalWhips"/> times, the release of Samus, then
    /// his idle tail for good. His grab starts it by this name.
    /// </summary>
    internal static Animation<DraygonSegment> TailFacingLeftFinalWhips { get; } = Animation<DraygonSegment>.LeadingInto(
        [Segments.Repeated(P.TailFacingLeftWhip, FinalWhips), Segments.Whole(P.TailFacingLeftReleaseSamus)], [Segments.Whole(P.TailFacingLeftIdle)]);

    /// <summary>His tail's final whips facing right (#2399), as <see cref="TailFacingLeftFinalWhips"/> plays them facing left.</summary>
    internal static Animation<DraygonSegment> TailFacingRightFinalWhips { get; } = Animation<DraygonSegment>.LeadingInto(
        [Segments.Repeated(P.TailFacingRightWhip, FinalWhips), Segments.Whole(P.TailFacingRightReleaseSamus)], [Segments.Whole(P.TailFacingRightIdle)]);

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
