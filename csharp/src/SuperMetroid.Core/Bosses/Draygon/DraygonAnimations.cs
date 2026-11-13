using SuperMetroid.Core.Programs;
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
            Show(22, 1),
        ],
        [P.BodyFacingRightReset] =
        [
            Cue(new DraygonCue.StartPartAnimations(P.BodyFacingRightIdle, P.EyeFacingRightIdle, P.TailFacingRightIdle, P.ArmsFacingRightIdle)),
            Cue(new DraygonCue.SetEyeAction(DraygonEyeAction.FacingRight)),
            Show(63, 1),
        ],
        [P.ArmsFacingLeftIdle] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(3, 5),
            Show(4, 5),
            Show(5, 5),
        ],
        [P.ArmsFacingLeftNearSwoopApex] =
        [
            Show(23, 1),
            Show(24, 1),
            Show(25, 1),
            Show(26, 64),
        ],
        [P.ArmsFacingLeftGrab] =
        [
            Show(23, 1),
            Show(24, 1),
            Show(25, 1),
            Show(26, 8),
            Show(25, 1),
            Show(24, 1),
            Show(23, 1),
            Show(24, 1),
        ],
        [P.ArmsFacingLeftDying] =
        [
            Show(6, 5),
            Show(7, 5),
            Show(8, 5),
            Show(9, 5),
        ],
        [P.BodyFacingLeftIdle] =
        [
            Cue(new DraygonCue.SetEyeAction(DraygonEyeAction.FacingLeft)),
            Show(22, 1),
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
            Show(10, 1),
            Show(11, 2),
            Show(12, 3),
            Cue(new DraygonCue.SpawnGoop(HorizontalDirection.Left)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.GoopSpit)),
            Show(13, 3),
            Show(12, 2),
            Show(11, 2),
            Show(10, 1),
        ],
        [P.BodyFacingLeftRoar] =
        [
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Roar)),
            Show(10, 6),
            Show(11, 6),
            Show(12, 6),
            Show(13, 6),
            Show(12, 6),
            Show(11, 6),
            Show(10, 6),
        ],
        [P.EyeFacingLeftIdle] =
        [
            Show(14, 21),
            Show(15, 5),
            Show(16, 5),
            Show(15, 10),
            Show(18, 10),
            Show(18, 10),
            Show(20, 10),
            Show(20, 10),
            Show(21, 10),
            Show(16, 5),
            Show(15, 5),
            Show(14, 5),
            Cue(new DraygonCue.SetOwnEyeAction(DraygonEyeAction.FacingLeft)),
        ],
        [P.EyeFacingLeftDying] =
        [
        ],
        [P.EyeFacingLeftDyingLoop] =
        [
            Show(18, 4),
            Show(20, 4),
            Show(19, 4),
            Show(21, 4),
            Show(17, 32),
            Show(16, 16),
        ],
        [P.EyeFacingLeftDead] =
        [
            Show(17, 32),
            Show(16, 32),
            Show(15, 32),
            Show(14, 1),
        ],
        [P.EyeFacingLeftLookingLeft] =
        [
            Show(18, 1),
        ],
        [P.EyeFacingLeftLookingRight] =
        [
            Show(19, 1),
        ],
        [P.EyeFacingLeftLookingUp] =
        [
            Show(20, 1),
        ],
        [P.EyeFacingLeftLookingDown] =
        [
            Show(21, 1),
        ],
        [P.TailFacingLeftIdle] =
        [
            Show(27, 8),
            Show(28, 7),
            Show(29, 6),
            Show(30, 6),
            Show(31, 6),
            Show(32, 6),
            Show(33, 6),
            Show(32, 6),
            Show(31, 6),
            Show(30, 6),
            Show(29, 6),
            Show(28, 7),
        ],
        [P.TailFacingLeftInitialFakeWhip] =
        [
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-1, -1))),
            Show(29, 16),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-2, -2))),
            Show(34, 6),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-3, -3))),
            Show(35, 5),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-4, -4))),
            Show(36, 4),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-5, -5))),
            Show(37, 3),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-6, -6))),
            Show(38, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-8, -8))),
            Show(39, 1),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(0, 0))),
            Show(40, 16),
            Show(39, 1),
            Show(38, 2),
            Show(37, 3),
            Show(36, 4),
            Show(35, 5),
            Show(34, 6),
        ],
        [P.TailFacingLeftFinalWhips] =
        [
        ],
        [P.TailFacingLeftFinalWhipsLoop] =
        [
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-1, -1))),
            Show(29, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-2, -2))),
            Show(34, 6),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-3, -3))),
            Show(35, 5),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-4, -4))),
            Show(36, 4),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-5, -5))),
            Show(37, 3),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-6, -6))),
            Show(38, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-8, -8))),
            Show(39, 1),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(0, 0))),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(40, 3),
            Show(39, 1),
            Show(38, 2),
            Show(37, 3),
            Show(36, 4),
            Show(35, 5),
            Show(34, 6),
            Cue(new DraygonCue.SetBodyPhase(DraygonPhase.ReleaseSamus)),
        ],
        [P.TailFacingLeftWhip] =
        [
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-1, -1))),
            Show(29, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-2, -2))),
            Show(34, 6),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-3, -3))),
            Show(35, 5),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-4, -4))),
            Show(36, 4),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-5, -5))),
            Show(37, 3),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-6, -6))),
            Show(38, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(-8, -8))),
            Show(39, 1),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(0, 0))),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(40, 3),
            Show(39, 1),
            Show(38, 2),
            Show(37, 3),
            Show(36, 4),
            Show(35, 5),
            Show(34, 6),
        ],
        [P.TailFacingLeftFlail] =
        [
            Show(29, 2),
            Show(34, 6),
            Show(35, 5),
            Show(36, 4),
            Show(37, 3),
            Show(38, 2),
            Show(39, 1),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(40, 3),
            Show(39, 1),
            Show(38, 2),
            Show(37, 3),
            Show(36, 4),
            Show(35, 5),
            Show(34, 6),
        ],
        [P.ArmsFacingRightIdle] =
        [
            Show(41, 5),
            Show(42, 5),
            Show(43, 5),
            Show(44, 5),
            Show(45, 5),
            Show(46, 5),
        ],
        [P.ArmsFacingRightNearSwoopApex] =
        [
            Show(64, 1),
            Show(65, 1),
            Show(66, 1),
            Show(67, 64),
        ],
        [P.ArmsFacingRightGrab] =
        [
            Show(64, 1),
            Show(65, 1),
            Show(66, 1),
            Show(67, 8),
            Show(66, 1),
            Show(65, 1),
            Show(64, 1),
            Show(65, 1),
        ],
        [P.ArmsFacingRightDying] =
        [
            Show(47, 5),
            Show(48, 5),
            Show(49, 5),
            Show(50, 5),
        ],
        [P.BodyFacingRightIdle] =
        [
            Cue(new DraygonCue.SetEyeAction(DraygonEyeAction.FacingRight)),
            Show(63, 1),
        ],
        [P.BodyFacingRightFireGoop] =
        [
            Show(51, 1),
            Show(52, 2),
            Show(53, 3),
            Cue(new DraygonCue.SpawnGoop(HorizontalDirection.Right)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.GoopSpit)),
            Show(54, 3),
            Show(53, 2),
            Show(52, 2),
            Show(51, 1),
        ],
        [P.BodyFacingRightRoar] =
        [
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Roar)),
            Show(51, 6),
            Show(52, 6),
            Show(53, 6),
            Show(54, 6),
            Show(53, 6),
            Show(52, 6),
            Show(51, 6),
        ],
        [P.EyeFacingRightIdle] =
        [
            Show(55, 21),
            Show(56, 5),
            Show(57, 5),
            Show(56, 10),
            Show(59, 10),
            Show(59, 10),
            Show(61, 10),
            Show(61, 10),
            Show(62, 10),
            Show(57, 5),
            Show(56, 5),
            Show(55, 5),
            Cue(new DraygonCue.SetOwnEyeAction(DraygonEyeAction.FacingLeft)),
        ],
        [P.EyeFacingRightDying] =
        [
        ],
        [P.EyeFacingRightDyingLoop] =
        [
            Show(59, 4),
            Show(61, 4),
            Show(60, 4),
            Show(62, 4),
            Show(58, 32),
            Show(57, 16),
        ],
        [P.EyeFacingRightDead] =
        [
            Show(58, 32),
            Show(57, 32),
            Show(56, 32),
            Show(55, 1),
        ],
        [P.EyeFacingRightLookingRight] =
        [
            Show(59, 1),
        ],
        [P.EyeFacingRightLookingLeft] =
        [
            Show(60, 1),
        ],
        [P.EyeFacingRightLookingUp] =
        [
            Show(61, 1),
        ],
        [P.EyeFacingRightLookingDown] =
        [
            Show(62, 1),
        ],
        [P.TailFacingRightIdle] =
        [
            Show(68, 8),
            Show(69, 7),
            Show(70, 6),
            Show(71, 6),
            Show(72, 6),
            Show(73, 6),
            Show(74, 6),
            Show(73, 6),
            Show(72, 6),
            Show(71, 6),
            Show(70, 6),
            Show(69, 7),
        ],
        [P.TailFacingRightFinalWhips] =
        [
        ],
        [P.TailFacingRightFinalWhipsLoop] =
        [
            Cue(new DraygonCue.DisplaceBodyGraphics(new(1, -1))),
            Show(70, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(2, -2))),
            Show(75, 6),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(3, -3))),
            Show(76, 5),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(4, -4))),
            Show(77, 4),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(5, -5))),
            Show(78, 3),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(6, -6))),
            Show(79, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(8, -8))),
            Show(80, 1),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(0, 0))),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(81, 3),
            Show(80, 1),
            Show(79, 2),
            Show(78, 3),
            Show(77, 4),
            Show(76, 5),
            Show(75, 6),
            Cue(new DraygonCue.SetBodyPhase(DraygonPhase.ReleaseSamus)),
        ],
        [P.TailFacingRightWhip] =
        [
            Cue(new DraygonCue.DisplaceBodyGraphics(new(1, -1))),
            Show(70, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(2, -2))),
            Show(75, 6),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(3, -3))),
            Show(76, 5),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(4, -4))),
            Show(77, 4),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(5, -5))),
            Show(78, 3),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(6, -6))),
            Show(79, 2),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(8, -8))),
            Show(80, 1),
            Cue(new DraygonCue.DisplaceBodyGraphics(new(0, 0))),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(81, 3),
            Show(80, 1),
            Show(79, 2),
            Show(78, 3),
            Show(77, 4),
            Show(76, 5),
            Show(75, 6),
        ],
        [P.TailFacingRightFlail] =
        [
            Show(70, 2),
            Show(75, 6),
            Show(76, 5),
            Show(77, 4),
            Show(78, 3),
            Show(79, 2),
            Show(80, 1),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            Show(81, 3),
            Show(80, 1),
            Show(79, 2),
            Show(78, 3),
            Show(77, 4),
            Show(76, 5),
            Show(75, 6),
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

    /// <summary>Shows the catalog frame <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<DraygonFrame, DraygonCue> Show(int frame, int duration) =>
        new AnimationBeat<DraygonFrame, DraygonCue>.Frame(new(frame), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<DraygonFrame, DraygonCue> Cue(DraygonCue cue) =>
        new AnimationBeat<DraygonFrame, DraygonCue>.Event(cue);

    /// <summary>Keeps the current frame for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<DraygonFrame, DraygonCue> Wait(int duration) =>
        new AnimationBeat<DraygonFrame, DraygonCue>.Pause(duration);
}
