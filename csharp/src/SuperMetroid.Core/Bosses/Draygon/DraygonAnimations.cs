using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Bosses.Draygon.DraygonProgram;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.SpriteObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's typed programs (#641), in place of its bank-$A5 instruction lists.</summary>
internal static class DraygonAnimations
{
    /// <summary>Every body, eye, tail and arms program.</summary>
    internal static AnimationProgramSet<DraygonProgram, DraygonFrame, DraygonCue> Programs { get; } = new(
        new Dictionary<DraygonProgram, AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>[]>
    {
        [P.Sleep] =
        [
            Hold,
        ],
        [P.BodyFacingLeftReset] =
        [
            Cue(new DraygonCue.StartPartPrograms(P.BodyFacingLeftIdle, P.EyeFacingLeftIdle, P.TailFacingLeftIdle, P.ArmsFacingLeftIdle)),
            Cue(DraygonCue.BeginHudDraw.Instance),
            Cue(new DraygonCue.SetEyeFunction(DraygonEyeFunction.FacingLeft)),
            Show(22, 1),
            Hold,
        ],
        [P.BodyFacingRightReset] =
        [
            Cue(new DraygonCue.StartPartPrograms(P.BodyFacingRightIdle, P.EyeFacingRightIdle, P.TailFacingRightIdle, P.ArmsFacingRightIdle)),
            Cue(DraygonCue.BeginHudDraw.Instance),
            Cue(new DraygonCue.SetEyeFunction(DraygonEyeFunction.FacingRight)),
            Show(63, 1),
            Hold,
        ],
        [P.ArmsFacingLeftIdle] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(3, 5),
            Show(4, 5),
            Show(5, 5),
            GoTo(P.ArmsFacingLeftIdle),
        ],
        [P.ArmsFacingLeftNearSwoopApex] =
        [
            Show(23, 1),
            Show(24, 1),
            Show(25, 1),
            Show(26, 64),
            Hold,
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
            Hold,
        ],
        [P.ArmsFacingLeftDying] =
        [
            Show(6, 5),
            Show(7, 5),
            Show(8, 5),
            Show(9, 5),
            GoTo(P.BodyDying),
        ],
        [P.BodyFacingLeftIdle] =
        [
            Cue(DraygonCue.BeginHudDraw.Instance),
            Cue(new DraygonCue.SetEyeFunction(DraygonEyeFunction.FacingLeft)),
            Show(22, 1),
            Hold,
        ],
        [P.BodyDying] =
        [
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.DyingCry)),
            Cue(DraygonCue.SetBodyIntangible.Instance),
            SetCounter(8),
            FallInto(P.BodyDyingExplosions),
        ],
        [P.BodyDyingExplosions] =
        [
            Wait(12),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.BotwoonLargeExplosion)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.SporeSpawnDyingExplosion)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.DustCloud)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.DraygonBreathBubble)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            RepeatWhileCounting(P.BodyDyingExplosions),
            Wait(1),
            Cue(DraygonCue.ParalyseTailAndArms.Instance),
            FallInto(P.BodyDyingExplosionsLoop),
        ],
        [P.BodyDyingExplosionsLoop] =
        [
            Wait(16),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.BotwoonLargeExplosion)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.SporeSpawnDyingExplosion)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.DustCloud)),
            Cue(new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.DraygonBreathBubble)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            GoTo(P.BodyDyingExplosionsLoop),
        ],
        [P.Delete] =
        [
            Stop,
        ],
        [P.BodyFacingLeftFireGoop] =
        [
            Show(10, 1),
            Show(11, 2),
            Show(12, 3),
            Cue(new DraygonCue.SpawnGoop(MovingRight: false)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.GoopSpit)),
            Show(13, 3),
            Show(12, 2),
            Show(11, 2),
            Show(10, 1),
            Hold,
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
            Hold,
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
            Cue(new DraygonCue.SetOwnEyeFunction(DraygonEyeFunction.FacingLeft)),
            Hold,
        ],
        [P.EyeFacingLeftDying] =
        [
            SetCounter(4),
            FallInto(P.EyeFacingLeftDyingLoop),
        ],
        [P.EyeFacingLeftDyingLoop] =
        [
            Show(18, 4),
            Show(20, 4),
            Show(19, 4),
            Show(21, 4),
            RepeatWhileCounting(P.EyeFacingLeftDyingLoop),
            Show(17, 32),
            Show(16, 16),
            Hold,
        ],
        [P.EyeFacingLeftDead] =
        [
            Show(17, 32),
            Show(16, 32),
            Show(15, 32),
            Show(14, 1),
            Hold,
        ],
        [P.EyeFacingLeftLookingLeft] =
        [
            Show(18, 1),
            Hold,
        ],
        [P.EyeFacingLeftLookingRight] =
        [
            Show(19, 1),
            Hold,
        ],
        [P.EyeFacingLeftLookingUp] =
        [
            Show(20, 1),
            Hold,
        ],
        [P.EyeFacingLeftLookingDown] =
        [
            Show(21, 1),
            Hold,
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
            GoTo(P.TailFacingLeftIdle),
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
            GoTo(P.TailFacingLeftIdle),
        ],
        [P.TailFacingLeftFinalWhips] =
        [
            SetCounter(4),
            FallInto(P.TailFacingLeftFinalWhipsLoop),
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
            RepeatWhileCounting(P.TailFacingLeftFinalWhipsLoop),
            Cue(new DraygonCue.SetBodyFunction(DraygonAiFunction.ReleaseSamus)),
            GoTo(P.TailFacingLeftIdle),
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
            GoTo(P.TailFacingLeftIdle),
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
            GoTo(P.TailFacingLeftIdle),
        ],
        [P.ArmsFacingRightIdle] =
        [
            Show(41, 5),
            Show(42, 5),
            Show(43, 5),
            Show(44, 5),
            Show(45, 5),
            Show(46, 5),
            GoTo(P.ArmsFacingRightIdle),
        ],
        [P.ArmsFacingRightNearSwoopApex] =
        [
            Show(64, 1),
            Show(65, 1),
            Show(66, 1),
            Show(67, 64),
            Hold,
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
            Hold,
        ],
        [P.ArmsFacingRightDying] =
        [
            Show(47, 5),
            Show(48, 5),
            Show(49, 5),
            Show(50, 5),
            GoTo(P.BodyDying),
        ],
        [P.BodyFacingRightIdle] =
        [
            Cue(DraygonCue.BeginHudDraw.Instance),
            Cue(new DraygonCue.SetEyeFunction(DraygonEyeFunction.FacingRight)),
            Show(63, 1),
            Hold,
        ],
        [P.BodyFacingRightFireGoop] =
        [
            Show(51, 1),
            Show(52, 2),
            Show(53, 3),
            Cue(new DraygonCue.SpawnGoop(MovingRight: true)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.GoopSpit)),
            Show(54, 3),
            Show(53, 2),
            Show(52, 2),
            Show(51, 1),
            Hold,
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
            Hold,
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
            Cue(new DraygonCue.SetOwnEyeFunction(DraygonEyeFunction.FacingLeft)),
            Hold,
        ],
        [P.EyeFacingRightDying] =
        [
            SetCounter(4),
            FallInto(P.EyeFacingRightDyingLoop),
        ],
        [P.EyeFacingRightDyingLoop] =
        [
            Show(59, 4),
            Show(61, 4),
            Show(60, 4),
            Show(62, 4),
            RepeatWhileCounting(P.EyeFacingRightDyingLoop),
            Show(58, 32),
            Show(57, 16),
            Hold,
        ],
        [P.EyeFacingRightDead] =
        [
            Show(58, 32),
            Show(57, 32),
            Show(56, 32),
            Show(55, 1),
            Hold,
        ],
        [P.EyeFacingRightLookingRight] =
        [
            Show(59, 1),
            Hold,
        ],
        [P.EyeFacingRightLookingLeft] =
        [
            Show(60, 1),
            Hold,
        ],
        [P.EyeFacingRightLookingUp] =
        [
            Show(61, 1),
            Hold,
        ],
        [P.EyeFacingRightLookingDown] =
        [
            Show(62, 1),
            Hold,
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
            GoTo(P.TailFacingRightIdle),
        ],
        [P.TailFacingRightFinalWhips] =
        [
            SetCounter(4),
            FallInto(P.TailFacingRightFinalWhipsLoop),
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
            RepeatWhileCounting(P.TailFacingRightFinalWhipsLoop),
            Cue(new DraygonCue.SetBodyFunction(DraygonAiFunction.ReleaseSamus)),
            GoTo(P.TailFacingRightIdle),
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
            GoTo(P.TailFacingRightIdle),
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
            GoTo(P.TailFacingRightIdle),
        ],    });

    /// <summary>Shows the catalog frame <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> Show(int frame, int duration) =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.Show(new(frame), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> Cue(DraygonCue cue) =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.Publish(cue);

    /// <summary>Continues at the start of <paramref name="program"/>.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> GoTo(DraygonProgram program) =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> FallInto(DraygonProgram program) =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.FallInto(program);

    /// <summary>Keeps the current frame for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> Wait(int duration) =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.Wait(duration);

    /// <summary>Loads the part's loop counter with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> SetCounter(int count) =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.SetCounter(count);

    /// <summary>Repeats <paramref name="program"/> while the loop counter counts down.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> RepeatWhileCounting(DraygonProgram program) =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.RepeatWhileCounting(program);

    /// <summary>Holds the last frame: the native sleep.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> Hold =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.Hold;

    /// <summary>Stops the part and deletes it.</summary>
    private static AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue> Stop =>
        AnimationProgramStep<DraygonProgram, DraygonFrame, DraygonCue>.Stop;
}
