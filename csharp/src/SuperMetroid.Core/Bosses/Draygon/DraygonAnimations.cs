using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Bosses.Draygon.DraygonFrame;
using P = SuperMetroid.Core.Bosses.Draygon.DraygonSegment;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's typed animations (#641, #1600): each body, eye, tail and arms segment's beats, and the animation starting at each.</summary>
internal static class DraygonAnimations
{
    /// <summary>Every body, eye, tail and arms segment's beats.</summary>
    internal static AnimationSegments<DraygonSegment, DraygonFrame, DraygonCue> Segments { get; } = new(
        new Dictionary<DraygonSegment, AnimationBeat<DraygonFrame, DraygonCue>[]>
    {
        [P.Sleep] =
        [
        ],
        [P.BodyFacingLeftReset] =
        [
            Cue(new DraygonCue.StartPartAnimations(P.BodyFacingLeftIdle, P.EyeFacingLeftIdle, P.TailFacingLeftIdle, P.ArmsFacingLeftIdle)),
            Cue(new DraygonCue.SetEyeAction(DraygonEyeAction.FacingLeft)),
            Show(F.BodyFacingLeftReset, 1),
        ],
        [P.BodyFacingRightReset] =
        [
            Cue(new DraygonCue.StartPartAnimations(P.BodyFacingRightIdle, P.EyeFacingRightIdle, P.TailFacingRightIdle, P.ArmsFacingRightIdle)),
            Cue(new DraygonCue.SetEyeAction(DraygonEyeAction.FacingRight)),
            Show(F.BodyFacingRightReset, 1),
        ],
        [P.ArmsFacingLeftIdle] =
        [
            Show(F.ArmsFacingLeftIdleFirst, 5),
            Show(F.ArmsFacingLeftIdleSecond, 5),
            Show(F.ArmsFacingLeftIdleThird, 5),
            Show(F.ArmsFacingLeftIdleFourth, 5),
            Show(F.ArmsFacingLeftIdleFifth, 5),
            Show(F.ArmsFacingLeftIdleSixth, 5),
        ],
        [P.ArmsFacingLeftNearSwoopApex] =
        [
            Show(F.ArmsFacingLeftNearSwoopApexFirst, 1),
            Show(F.ArmsFacingLeftNearSwoopApexSecond, 1),
            Show(F.ArmsFacingLeftNearSwoopApexThird, 1),
            Show(F.ArmsFacingLeftNearSwoopApexFourth, 64),
        ],
        [P.ArmsFacingLeftGrab] =
        [
            Show(F.ArmsFacingLeftNearSwoopApexFirst, 1),
            Show(F.ArmsFacingLeftNearSwoopApexSecond, 1),
            Show(F.ArmsFacingLeftNearSwoopApexThird, 1),
            Show(F.ArmsFacingLeftNearSwoopApexFourth, 8),
            Show(F.ArmsFacingLeftNearSwoopApexThird, 1),
            Show(F.ArmsFacingLeftNearSwoopApexSecond, 1),
            Show(F.ArmsFacingLeftNearSwoopApexFirst, 1),
            Show(F.ArmsFacingLeftNearSwoopApexSecond, 1),
        ],
        [P.ArmsFacingLeftDying] =
        [
            Show(F.ArmsFacingLeftDyingFirst, 5),
            Show(F.ArmsFacingLeftDyingSecond, 5),
            Show(F.ArmsFacingLeftDyingThird, 5),
            Show(F.ArmsFacingLeftDyingFourth, 5),
        ],
        [P.BodyFacingLeftIdle] =
        [
            Cue(new DraygonCue.SetEyeAction(DraygonEyeAction.FacingLeft)),
            Show(F.BodyFacingLeftReset, 1),
        ],
        [P.BodyDying] =
        [
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.DyingCry)),
            Cue(DraygonCue.SetBodyIntangible.Instance),
        ],
        [P.BodyDyingExplosions] =
        [
            Wait(12),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.BotwoonLargeExplosion)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.SporeSpawnDyingExplosion)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.DustCloud)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.DraygonBreathBubble)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Wait(1),
            Cue(DraygonCue.ParalyseTailAndArms.Instance),
        ],
        [P.BodyDyingExplosionsLoop] =
        [
            Wait(16),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.BotwoonLargeExplosion)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.SporeSpawnDyingExplosion)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.DustCloud)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.DraygonBreathBubble)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
        ],
        [P.Delete] =
        [
        ],
        [P.BodyFacingLeftFireGoop] =
        [
            Show(F.BodyFacingLeftFireGoopFirst, 1),
            Show(F.BodyFacingLeftFireGoopSecond, 2),
            Show(F.BodyFacingLeftFireGoopThird, 3),
            Cue(new DraygonCue.SpawnGoop(HorizontalDirection.Left)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.GoopSpit)),
            Show(F.BodyFacingLeftFireGoopFourth, 3),
            Show(F.BodyFacingLeftFireGoopThird, 2),
            Show(F.BodyFacingLeftFireGoopSecond, 2),
            Show(F.BodyFacingLeftFireGoopFirst, 1),
        ],
        [P.BodyFacingLeftRoar] =
        [
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Roar)),
            Show(F.BodyFacingLeftFireGoopFirst, 6),
            Show(F.BodyFacingLeftFireGoopSecond, 6),
            Show(F.BodyFacingLeftFireGoopThird, 6),
            Show(F.BodyFacingLeftFireGoopFourth, 6),
            Show(F.BodyFacingLeftFireGoopThird, 6),
            Show(F.BodyFacingLeftFireGoopSecond, 6),
            Show(F.BodyFacingLeftFireGoopFirst, 6),
        ],
        [P.EyeFacingLeftIdle] =
        [
            Show(F.EyeFacingLeftIdleFirst, 21),
            Show(F.EyeFacingLeftIdleSecond, 5),
            Show(F.EyeFacingLeftIdleThird, 5),
            Show(F.EyeFacingLeftIdleSecond, 10),
            Show(F.EyeFacingLeftIdleFourth, 10),
            Show(F.EyeFacingLeftIdleFourth, 10),
            Show(F.EyeFacingLeftIdleFifth, 10),
            Show(F.EyeFacingLeftIdleFifth, 10),
            Show(F.EyeFacingLeftIdleSixth, 10),
            Show(F.EyeFacingLeftIdleThird, 5),
            Show(F.EyeFacingLeftIdleSecond, 5),
            Show(F.EyeFacingLeftIdleFirst, 5),
            Cue(new DraygonCue.SetOwnEyeAction(DraygonEyeAction.FacingLeft)),
        ],
        [P.EyeFacingLeftDying] =
        [
        ],
        [P.EyeFacingLeftDyingLoop] =
        [
            Show(F.EyeFacingLeftIdleFourth, 4),
            Show(F.EyeFacingLeftIdleFifth, 4),
            Show(F.EyeFacingLeftDyingLoopThird, 4),
            Show(F.EyeFacingLeftIdleSixth, 4),
            Show(F.EyeFacingLeftDyingLoopFifth, 32),
            Show(F.EyeFacingLeftIdleThird, 16),
        ],
        [P.EyeFacingLeftDead] =
        [
            Show(F.EyeFacingLeftDyingLoopFifth, 32),
            Show(F.EyeFacingLeftIdleThird, 32),
            Show(F.EyeFacingLeftIdleSecond, 32),
            Show(F.EyeFacingLeftIdleFirst, 1),
        ],
        [P.EyeFacingLeftLookingLeft] =
        [
            Show(F.EyeFacingLeftIdleFourth, 1),
        ],
        [P.EyeFacingLeftLookingRight] =
        [
            Show(F.EyeFacingLeftDyingLoopThird, 1),
        ],
        [P.EyeFacingLeftLookingUp] =
        [
            Show(F.EyeFacingLeftIdleFifth, 1),
        ],
        [P.EyeFacingLeftLookingDown] =
        [
            Show(F.EyeFacingLeftIdleSixth, 1),
        ],
        [P.TailFacingLeftIdle] =
        [
            Show(F.TailFacingLeftIdleFirst, 8),
            Show(F.TailFacingLeftIdleSecond, 7),
            Show(F.TailFacingLeftIdleThird, 6),
            Show(F.TailFacingLeftIdleFourth, 6),
            Show(F.TailFacingLeftIdleFifth, 6),
            Show(F.TailFacingLeftIdleSixth, 6),
            Show(F.TailFacingLeftIdleSeventh, 6),
            Show(F.TailFacingLeftIdleSixth, 6),
            Show(F.TailFacingLeftIdleFifth, 6),
            Show(F.TailFacingLeftIdleFourth, 6),
            Show(F.TailFacingLeftIdleThird, 6),
            Show(F.TailFacingLeftIdleSecond, 7),
        ],
        [P.TailFacingLeftInitialFakeWhip] =
        [
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-1, -1))),
            Show(F.TailFacingLeftIdleThird, 16),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-2, -2))),
            Show(F.TailFacingLeftInitialFakeWhipSecond, 6),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-3, -3))),
            Show(F.TailFacingLeftInitialFakeWhipThird, 5),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-4, -4))),
            Show(F.TailFacingLeftInitialFakeWhipFourth, 4),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-5, -5))),
            Show(F.TailFacingLeftInitialFakeWhipFifth, 3),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-6, -6))),
            Show(F.TailFacingLeftInitialFakeWhipSixth, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-8, -8))),
            Show(F.TailFacingLeftInitialFakeWhipSeventh, 1),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(0, 0))),
            Show(F.TailFacingLeftInitialFakeWhipEighth, 16),
            Show(F.TailFacingLeftInitialFakeWhipSeventh, 1),
            Show(F.TailFacingLeftInitialFakeWhipSixth, 2),
            Show(F.TailFacingLeftInitialFakeWhipFifth, 3),
            Show(F.TailFacingLeftInitialFakeWhipFourth, 4),
            Show(F.TailFacingLeftInitialFakeWhipThird, 5),
            Show(F.TailFacingLeftInitialFakeWhipSecond, 6),
        ],
        [P.TailFacingLeftFinalWhips] =
        [
        ],
        [P.TailFacingLeftFinalWhipsLoop] =
        [
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-1, -1))),
            Show(F.TailFacingLeftIdleThird, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-2, -2))),
            Show(F.TailFacingLeftInitialFakeWhipSecond, 6),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-3, -3))),
            Show(F.TailFacingLeftInitialFakeWhipThird, 5),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-4, -4))),
            Show(F.TailFacingLeftInitialFakeWhipFourth, 4),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-5, -5))),
            Show(F.TailFacingLeftInitialFakeWhipFifth, 3),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-6, -6))),
            Show(F.TailFacingLeftInitialFakeWhipSixth, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-8, -8))),
            Show(F.TailFacingLeftInitialFakeWhipSeventh, 1),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(0, 0))),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(F.TailFacingLeftInitialFakeWhipEighth, 3),
            Show(F.TailFacingLeftInitialFakeWhipSeventh, 1),
            Show(F.TailFacingLeftInitialFakeWhipSixth, 2),
            Show(F.TailFacingLeftInitialFakeWhipFifth, 3),
            Show(F.TailFacingLeftInitialFakeWhipFourth, 4),
            Show(F.TailFacingLeftInitialFakeWhipThird, 5),
            Show(F.TailFacingLeftInitialFakeWhipSecond, 6),
            Cue(new DraygonCue.SetBodyPhase(DraygonPhase.ReleaseSamus)),
        ],
        [P.TailFacingLeftWhip] =
        [
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-1, -1))),
            Show(F.TailFacingLeftIdleThird, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-2, -2))),
            Show(F.TailFacingLeftInitialFakeWhipSecond, 6),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-3, -3))),
            Show(F.TailFacingLeftInitialFakeWhipThird, 5),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-4, -4))),
            Show(F.TailFacingLeftInitialFakeWhipFourth, 4),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-5, -5))),
            Show(F.TailFacingLeftInitialFakeWhipFifth, 3),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-6, -6))),
            Show(F.TailFacingLeftInitialFakeWhipSixth, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-8, -8))),
            Show(F.TailFacingLeftInitialFakeWhipSeventh, 1),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(0, 0))),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(F.TailFacingLeftInitialFakeWhipEighth, 3),
            Show(F.TailFacingLeftInitialFakeWhipSeventh, 1),
            Show(F.TailFacingLeftInitialFakeWhipSixth, 2),
            Show(F.TailFacingLeftInitialFakeWhipFifth, 3),
            Show(F.TailFacingLeftInitialFakeWhipFourth, 4),
            Show(F.TailFacingLeftInitialFakeWhipThird, 5),
            Show(F.TailFacingLeftInitialFakeWhipSecond, 6),
        ],
        [P.TailFacingLeftFlail] =
        [
            Show(F.TailFacingLeftIdleThird, 2),
            Show(F.TailFacingLeftInitialFakeWhipSecond, 6),
            Show(F.TailFacingLeftInitialFakeWhipThird, 5),
            Show(F.TailFacingLeftInitialFakeWhipFourth, 4),
            Show(F.TailFacingLeftInitialFakeWhipFifth, 3),
            Show(F.TailFacingLeftInitialFakeWhipSixth, 2),
            Show(F.TailFacingLeftInitialFakeWhipSeventh, 1),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(F.TailFacingLeftInitialFakeWhipEighth, 3),
            Show(F.TailFacingLeftInitialFakeWhipSeventh, 1),
            Show(F.TailFacingLeftInitialFakeWhipSixth, 2),
            Show(F.TailFacingLeftInitialFakeWhipFifth, 3),
            Show(F.TailFacingLeftInitialFakeWhipFourth, 4),
            Show(F.TailFacingLeftInitialFakeWhipThird, 5),
            Show(F.TailFacingLeftInitialFakeWhipSecond, 6),
        ],
        [P.ArmsFacingRightIdle] =
        [
            Show(F.ArmsFacingRightIdleFirst, 5),
            Show(F.ArmsFacingRightIdleSecond, 5),
            Show(F.ArmsFacingRightIdleThird, 5),
            Show(F.ArmsFacingRightIdleFourth, 5),
            Show(F.ArmsFacingRightIdleFifth, 5),
            Show(F.ArmsFacingRightIdleSixth, 5),
        ],
        [P.ArmsFacingRightNearSwoopApex] =
        [
            Show(F.ArmsFacingRightNearSwoopApexFirst, 1),
            Show(F.ArmsFacingRightNearSwoopApexSecond, 1),
            Show(F.ArmsFacingRightNearSwoopApexThird, 1),
            Show(F.ArmsFacingRightNearSwoopApexFourth, 64),
        ],
        [P.ArmsFacingRightGrab] =
        [
            Show(F.ArmsFacingRightNearSwoopApexFirst, 1),
            Show(F.ArmsFacingRightNearSwoopApexSecond, 1),
            Show(F.ArmsFacingRightNearSwoopApexThird, 1),
            Show(F.ArmsFacingRightNearSwoopApexFourth, 8),
            Show(F.ArmsFacingRightNearSwoopApexThird, 1),
            Show(F.ArmsFacingRightNearSwoopApexSecond, 1),
            Show(F.ArmsFacingRightNearSwoopApexFirst, 1),
            Show(F.ArmsFacingRightNearSwoopApexSecond, 1),
        ],
        [P.ArmsFacingRightDying] =
        [
            Show(F.ArmsFacingRightDyingFirst, 5),
            Show(F.ArmsFacingRightDyingSecond, 5),
            Show(F.ArmsFacingRightDyingThird, 5),
            Show(F.ArmsFacingRightDyingFourth, 5),
        ],
        [P.BodyFacingRightIdle] =
        [
            Cue(new DraygonCue.SetEyeAction(DraygonEyeAction.FacingRight)),
            Show(F.BodyFacingRightReset, 1),
        ],
        [P.BodyFacingRightFireGoop] =
        [
            Show(F.BodyFacingRightFireGoopFirst, 1),
            Show(F.BodyFacingRightFireGoopSecond, 2),
            Show(F.BodyFacingRightFireGoopThird, 3),
            Cue(new DraygonCue.SpawnGoop(HorizontalDirection.Right)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.GoopSpit)),
            Show(F.BodyFacingRightFireGoopFourth, 3),
            Show(F.BodyFacingRightFireGoopThird, 2),
            Show(F.BodyFacingRightFireGoopSecond, 2),
            Show(F.BodyFacingRightFireGoopFirst, 1),
        ],
        [P.BodyFacingRightRoar] =
        [
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Roar)),
            Show(F.BodyFacingRightFireGoopFirst, 6),
            Show(F.BodyFacingRightFireGoopSecond, 6),
            Show(F.BodyFacingRightFireGoopThird, 6),
            Show(F.BodyFacingRightFireGoopFourth, 6),
            Show(F.BodyFacingRightFireGoopThird, 6),
            Show(F.BodyFacingRightFireGoopSecond, 6),
            Show(F.BodyFacingRightFireGoopFirst, 6),
        ],
        [P.EyeFacingRightIdle] =
        [
            Show(F.EyeFacingRightIdleFirst, 21),
            Show(F.EyeFacingRightIdleSecond, 5),
            Show(F.EyeFacingRightIdleThird, 5),
            Show(F.EyeFacingRightIdleSecond, 10),
            Show(F.EyeFacingRightIdleFourth, 10),
            Show(F.EyeFacingRightIdleFourth, 10),
            Show(F.EyeFacingRightIdleFifth, 10),
            Show(F.EyeFacingRightIdleFifth, 10),
            Show(F.EyeFacingRightIdleSixth, 10),
            Show(F.EyeFacingRightIdleThird, 5),
            Show(F.EyeFacingRightIdleSecond, 5),
            Show(F.EyeFacingRightIdleFirst, 5),
            Cue(new DraygonCue.SetOwnEyeAction(DraygonEyeAction.FacingLeft)),
        ],
        [P.EyeFacingRightDying] =
        [
        ],
        [P.EyeFacingRightDyingLoop] =
        [
            Show(F.EyeFacingRightIdleFourth, 4),
            Show(F.EyeFacingRightIdleFifth, 4),
            Show(F.EyeFacingRightDyingLoopThird, 4),
            Show(F.EyeFacingRightIdleSixth, 4),
            Show(F.EyeFacingRightDyingLoopFifth, 32),
            Show(F.EyeFacingRightIdleThird, 16),
        ],
        [P.EyeFacingRightDead] =
        [
            Show(F.EyeFacingRightDyingLoopFifth, 32),
            Show(F.EyeFacingRightIdleThird, 32),
            Show(F.EyeFacingRightIdleSecond, 32),
            Show(F.EyeFacingRightIdleFirst, 1),
        ],
        [P.EyeFacingRightLookingRight] =
        [
            Show(F.EyeFacingRightIdleFourth, 1),
        ],
        [P.EyeFacingRightLookingLeft] =
        [
            Show(F.EyeFacingRightDyingLoopThird, 1),
        ],
        [P.EyeFacingRightLookingUp] =
        [
            Show(F.EyeFacingRightIdleFifth, 1),
        ],
        [P.EyeFacingRightLookingDown] =
        [
            Show(F.EyeFacingRightIdleSixth, 1),
        ],
        [P.TailFacingRightIdle] =
        [
            Show(F.TailFacingRightIdleFirst, 8),
            Show(F.TailFacingRightIdleSecond, 7),
            Show(F.TailFacingRightIdleThird, 6),
            Show(F.TailFacingRightIdleFourth, 6),
            Show(F.TailFacingRightIdleFifth, 6),
            Show(F.TailFacingRightIdleSixth, 6),
            Show(F.TailFacingRightIdleSeventh, 6),
            Show(F.TailFacingRightIdleSixth, 6),
            Show(F.TailFacingRightIdleFifth, 6),
            Show(F.TailFacingRightIdleFourth, 6),
            Show(F.TailFacingRightIdleThird, 6),
            Show(F.TailFacingRightIdleSecond, 7),
        ],
        [P.TailFacingRightFinalWhips] =
        [
        ],
        [P.TailFacingRightFinalWhipsLoop] =
        [
            Cue(new DraygonCue.DisplaceBodyGraphics(new(1, -1))),
            Show(F.TailFacingRightIdleThird, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(2, -2))),
            Show(F.TailFacingRightFinalWhipsLoopSecond, 6),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(3, -3))),
            Show(F.TailFacingRightFinalWhipsLoopThird, 5),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(4, -4))),
            Show(F.TailFacingRightFinalWhipsLoopFourth, 4),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(5, -5))),
            Show(F.TailFacingRightFinalWhipsLoopFifth, 3),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(6, -6))),
            Show(F.TailFacingRightFinalWhipsLoopSixth, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(8, -8))),
            Show(F.TailFacingRightFinalWhipsLoopSeventh, 1),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(0, 0))),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(F.TailFacingRightFinalWhipsLoopEighth, 3),
            Show(F.TailFacingRightFinalWhipsLoopSeventh, 1),
            Show(F.TailFacingRightFinalWhipsLoopSixth, 2),
            Show(F.TailFacingRightFinalWhipsLoopFifth, 3),
            Show(F.TailFacingRightFinalWhipsLoopFourth, 4),
            Show(F.TailFacingRightFinalWhipsLoopThird, 5),
            Show(F.TailFacingRightFinalWhipsLoopSecond, 6),
            Cue(new DraygonCue.SetBodyPhase(DraygonPhase.ReleaseSamus)),
        ],
        [P.TailFacingRightWhip] =
        [
            Cue(new DraygonCue.DisplaceBodyGraphics(new(1, -1))),
            Show(F.TailFacingRightIdleThird, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(2, -2))),
            Show(F.TailFacingRightFinalWhipsLoopSecond, 6),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(3, -3))),
            Show(F.TailFacingRightFinalWhipsLoopThird, 5),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(4, -4))),
            Show(F.TailFacingRightFinalWhipsLoopFourth, 4),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(5, -5))),
            Show(F.TailFacingRightFinalWhipsLoopFifth, 3),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(6, -6))),
            Show(F.TailFacingRightFinalWhipsLoopSixth, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(8, -8))),
            Show(F.TailFacingRightFinalWhipsLoopSeventh, 1),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(0, 0))),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(F.TailFacingRightFinalWhipsLoopEighth, 3),
            Show(F.TailFacingRightFinalWhipsLoopSeventh, 1),
            Show(F.TailFacingRightFinalWhipsLoopSixth, 2),
            Show(F.TailFacingRightFinalWhipsLoopFifth, 3),
            Show(F.TailFacingRightFinalWhipsLoopFourth, 4),
            Show(F.TailFacingRightFinalWhipsLoopThird, 5),
            Show(F.TailFacingRightFinalWhipsLoopSecond, 6),
        ],
        [P.TailFacingRightFlail] =
        [
            Show(F.TailFacingRightIdleThird, 2),
            Show(F.TailFacingRightFinalWhipsLoopSecond, 6),
            Show(F.TailFacingRightFinalWhipsLoopThird, 5),
            Show(F.TailFacingRightFinalWhipsLoopFourth, 4),
            Show(F.TailFacingRightFinalWhipsLoopFifth, 3),
            Show(F.TailFacingRightFinalWhipsLoopSixth, 2),
            Show(F.TailFacingRightFinalWhipsLoopSeventh, 1),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(F.TailFacingRightFinalWhipsLoopEighth, 3),
            Show(F.TailFacingRightFinalWhipsLoopSeventh, 1),
            Show(F.TailFacingRightFinalWhipsLoopSixth, 2),
            Show(F.TailFacingRightFinalWhipsLoopFifth, 3),
            Show(F.TailFacingRightFinalWhipsLoopFourth, 4),
            Show(F.TailFacingRightFinalWhipsLoopThird, 5),
            Show(F.TailFacingRightFinalWhipsLoopSecond, 6),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DraygonSegment, Animation<DraygonSegment>> StartingAt { get; } = new Dictionary<DraygonSegment, Animation<DraygonSegment>>
    {
        [P.Sleep] = new([], AnimationEnd.Holds),
        [P.BodyFacingLeftReset] = new([new(P.BodyFacingLeftReset, 0, 3, 1)], AnimationEnd.Holds),
        [P.BodyFacingRightReset] = new([new(P.BodyFacingRightReset, 0, 3, 1)], AnimationEnd.Holds),
        [P.ArmsFacingLeftIdle] = new([new(P.ArmsFacingLeftIdle, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmsFacingLeftNearSwoopApex] = new([new(P.ArmsFacingLeftNearSwoopApex, 0, 4, 1)], AnimationEnd.Holds),
        [P.ArmsFacingLeftGrab] = new([new(P.ArmsFacingLeftGrab, 0, 8, 1)], AnimationEnd.Holds),
        [P.ArmsFacingLeftDying] = new([new(P.ArmsFacingLeftDying, 0, 4, 1), new(P.BodyDying, 0, 2, 1), new(P.BodyDyingExplosions, 0, 6, 8), new(P.BodyDyingExplosions, 6, 2, 1), new(P.BodyDyingExplosionsLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(4)),
        [P.BodyFacingLeftIdle] = new([new(P.BodyFacingLeftIdle, 0, 2, 1)], AnimationEnd.Holds),
        [P.BodyDying] = new([new(P.BodyDying, 0, 2, 1), new(P.BodyDyingExplosions, 0, 6, 8), new(P.BodyDyingExplosions, 6, 2, 1), new(P.BodyDyingExplosionsLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(3)),
        [P.BodyDyingExplosionsLoop] = new([new(P.BodyDyingExplosionsLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Delete] = new([], AnimationEnd.Stops),
        [P.BodyFacingLeftFireGoop] = new([new(P.BodyFacingLeftFireGoop, 0, 9, 1)], AnimationEnd.Holds),
        [P.BodyFacingLeftRoar] = new([new(P.BodyFacingLeftRoar, 0, 8, 1)], AnimationEnd.Holds),
        [P.EyeFacingLeftIdle] = new([new(P.EyeFacingLeftIdle, 0, 13, 1)], AnimationEnd.Holds),
        [P.EyeFacingLeftDying] = new([new(P.EyeFacingLeftDyingLoop, 0, 4, 4), new(P.EyeFacingLeftDyingLoop, 4, 2, 1)], AnimationEnd.Holds),
        [P.EyeFacingLeftDead] = new([new(P.EyeFacingLeftDead, 0, 4, 1)], AnimationEnd.Holds),
        [P.EyeFacingLeftLookingLeft] = new([new(P.EyeFacingLeftLookingLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.EyeFacingLeftLookingRight] = new([new(P.EyeFacingLeftLookingRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.EyeFacingLeftLookingUp] = new([new(P.EyeFacingLeftLookingUp, 0, 1, 1)], AnimationEnd.Holds),
        [P.EyeFacingLeftLookingDown] = new([new(P.EyeFacingLeftLookingDown, 0, 1, 1)], AnimationEnd.Holds),
        [P.TailFacingLeftIdle] = new([new(P.TailFacingLeftIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TailFacingLeftInitialFakeWhip] = new([new(P.TailFacingLeftInitialFakeWhip, 0, 22, 1), new(P.TailFacingLeftIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TailFacingLeftFinalWhips] = new([new(P.TailFacingLeftFinalWhipsLoop, 0, 24, 4), new(P.TailFacingLeftFinalWhipsLoop, 24, 1, 1), new(P.TailFacingLeftIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(2)),
        [P.TailFacingLeftWhip] = new([new(P.TailFacingLeftWhip, 0, 24, 1), new(P.TailFacingLeftIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TailFacingLeftFlail] = new([new(P.TailFacingLeftFlail, 0, 15, 1), new(P.TailFacingLeftIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ArmsFacingRightIdle] = new([new(P.ArmsFacingRightIdle, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmsFacingRightNearSwoopApex] = new([new(P.ArmsFacingRightNearSwoopApex, 0, 4, 1)], AnimationEnd.Holds),
        [P.ArmsFacingRightGrab] = new([new(P.ArmsFacingRightGrab, 0, 8, 1)], AnimationEnd.Holds),
        [P.ArmsFacingRightDying] = new([new(P.ArmsFacingRightDying, 0, 4, 1), new(P.BodyDying, 0, 2, 1), new(P.BodyDyingExplosions, 0, 6, 8), new(P.BodyDyingExplosions, 6, 2, 1), new(P.BodyDyingExplosionsLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(4)),
        [P.BodyFacingRightIdle] = new([new(P.BodyFacingRightIdle, 0, 2, 1)], AnimationEnd.Holds),
        [P.BodyFacingRightFireGoop] = new([new(P.BodyFacingRightFireGoop, 0, 9, 1)], AnimationEnd.Holds),
        [P.BodyFacingRightRoar] = new([new(P.BodyFacingRightRoar, 0, 8, 1)], AnimationEnd.Holds),
        [P.EyeFacingRightIdle] = new([new(P.EyeFacingRightIdle, 0, 13, 1)], AnimationEnd.Holds),
        [P.EyeFacingRightDying] = new([new(P.EyeFacingRightDyingLoop, 0, 4, 4), new(P.EyeFacingRightDyingLoop, 4, 2, 1)], AnimationEnd.Holds),
        [P.EyeFacingRightDead] = new([new(P.EyeFacingRightDead, 0, 4, 1)], AnimationEnd.Holds),
        [P.EyeFacingRightLookingRight] = new([new(P.EyeFacingRightLookingRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.EyeFacingRightLookingLeft] = new([new(P.EyeFacingRightLookingLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.EyeFacingRightLookingUp] = new([new(P.EyeFacingRightLookingUp, 0, 1, 1)], AnimationEnd.Holds),
        [P.EyeFacingRightLookingDown] = new([new(P.EyeFacingRightLookingDown, 0, 1, 1)], AnimationEnd.Holds),
        [P.TailFacingRightIdle] = new([new(P.TailFacingRightIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TailFacingRightFinalWhips] = new([new(P.TailFacingRightFinalWhipsLoop, 0, 24, 4), new(P.TailFacingRightFinalWhipsLoop, 24, 1, 1), new(P.TailFacingRightIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(2)),
        [P.TailFacingRightWhip] = new([new(P.TailFacingRightWhip, 0, 24, 1), new(P.TailFacingRightIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TailFacingRightFlail] = new([new(P.TailFacingRightFlail, 0, 15, 1), new(P.TailFacingRightIdle, 0, 12, 1)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DraygonSegment> StartingWith(DraygonSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<DraygonFrame, DraygonCue> Show(DraygonFrame frame, int duration) =>
        new AnimationBeat<DraygonFrame, DraygonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<DraygonFrame, DraygonCue> Cue(DraygonCue cue) =>
        new AnimationBeat<DraygonFrame, DraygonCue>.Event(cue);

    /// <summary>Keeps the current frame for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<DraygonFrame, DraygonCue> Wait(int duration) =>
        new AnimationBeat<DraygonFrame, DraygonCue>.Pause(duration);
}
