using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Doors.EyeDoorObjectProgram, SuperMetroid.Core.Rooms.Doors.EyeDoorObjectFrame, SuperMetroid.Core.Rooms.Doors.EyeDoorObjectCue>;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The eye door's PLM programs (#1296). Both facings run the same
/// programs; the right-facing components pulse faster, and each facing mirrors its draws and projectile operands.
/// </summary>
internal static class EyeDoorObjectProgramDefinitions
{
    /// <summary>The library-2 sound each hit on the eye queues.</summary>
    internal static readonly SoundEffectId HitSound = Library2Sound.EnemyKilled;
    /// <summary>The hit sound's request priority.</summary>
    internal static readonly SoundRequestPriority HitSoundPriority = SoundRequestPriority.Normal;
    /// <summary>The hits the eye takes before the door opens.</summary>
    internal const int HitThreshold = 3;
    /// <summary>The hit counter a Super Missile seeds: the next count passes the threshold.</summary>
    internal const int SuperMissileHitCounter = 119;
    /// <summary>How many times the opening eye flashes to the blue cap.</summary>
    private const int OpeningFlashes = 10;
    /// <summary>How long a left-facing component shows each pulse pose.</summary>
    private const int LeftPulseDuration = 8;
    /// <summary>How long a right-facing component shows each pulse pose.</summary>
    private const int RightPulseDuration = 6;
    /// <summary>The blocks across that wake the eye or keep it watching.</summary>
    private const int EyeWakeColumns = 6;
    /// <summary>The blocks up or down that wake the eye, keep it watching, or close it.</summary>
    private const int EyeWakeRows = 4;
    /// <summary>The blocks across that count as right in front of the eye.</summary>
    private const int EyeInFrontColumns = 1;
    /// <summary>The blocks across that make a component pulse.</summary>
    private const int ComponentWakeColumns = 6;
    /// <summary>The blocks up or down that make a component pulse.</summary>
    private const int ComponentWakeRows = 16;

    /// <summary>Each facing's program set: the right-facing components pulse faster.</summary>
    internal static IReadOnlyDictionary<EyeDoorOrientation, AnimationProgramSet<EyeDoorObjectProgram, EyeDoorObjectFrame, EyeDoorObjectCue>> Sets { get; } =
        new Dictionary<EyeDoorOrientation, AnimationProgramSet<EyeDoorObjectProgram, EyeDoorObjectFrame, EyeDoorObjectCue>>
    {
        [EyeDoorOrientation.Left] = new(Programs(LeftPulseDuration)),
        [EyeDoorOrientation.Right] = new(Programs(RightPulseDuration)),
    };

    /// <summary>The operands of each proximity cue; no other cue is a proximity branch.</summary>
    internal static IReadOnlyDictionary<EyeDoorObjectCue, EyeDoorProximity> Proximities { get; } = new Dictionary<EyeDoorObjectCue, EyeDoorProximity>
    {
        [EyeDoorObjectCue.ActivateIfSamusNear] = new(new(EyeWakeColumns, EyeWakeRows), EyeDoorObjectProgram.EyeActivate),
        [EyeDoorObjectCue.CloseIfSamusInFront] = new(new(EyeInFrontColumns, EyeWakeRows), EyeDoorObjectProgram.EyeClose),
        [EyeDoorObjectCue.WatchIfSamusNear] = new(new(EyeWakeColumns, EyeWakeRows), EyeDoorObjectProgram.EyeWatch),
        [EyeDoorObjectCue.PulseIfSamusNear] = new(new(ComponentWakeColumns, ComponentWakeRows), EyeDoorObjectProgram.ComponentActivate),
        [EyeDoorObjectCue.KeepPulsingIfSamusNear] = new(new(ComponentWakeColumns, ComponentWakeRows), EyeDoorObjectProgram.ComponentCycle),
    };

    /// <summary>One facing's programs, its components pulsing every <paramref name="pulse"/> updates.</summary>
    private static Dictionary<EyeDoorObjectProgram, Step[]> Programs(int pulse) =>
        new()
        {
            [EyeDoorObjectProgram.EyeStart] =
            [
                Step.Publish(EyeDoorObjectCue.ConvertIfOpened),
                Step.FallInto(EyeDoorObjectProgram.EyeIdle),
            ],
            [EyeDoorObjectProgram.EyeIdle] =
            [
                Step.Show(EyeDoorObjectFrame.Eye0, 4),
                Step.Publish(EyeDoorObjectCue.ActivateIfSamusNear),
                Step.GoTo(EyeDoorObjectProgram.EyeIdle),
            ],
            [EyeDoorObjectProgram.EyeActivate] =
            [
                Step.Publish(EyeDoorObjectCue.LinkHit),
                Step.Publish(EyeDoorObjectCue.ArmMissileWake),
                Step.Show(EyeDoorObjectFrame.Eye1, 8),
                Step.FallInto(EyeDoorObjectProgram.EyeWatch),
            ],
            [EyeDoorObjectProgram.EyeWatch] =
            [
                Step.Publish(EyeDoorObjectCue.CloseIfSamusInFront),
                Step.Show(EyeDoorObjectFrame.Eye2, 64),
                Step.Publish(EyeDoorObjectCue.Shoot),
                Step.Show(EyeDoorObjectFrame.Eye2, 32),
                Step.Publish(EyeDoorObjectCue.Shoot),
                Step.Show(EyeDoorObjectFrame.Eye2, 32),
                Step.Publish(EyeDoorObjectCue.Shoot),
                Step.Show(EyeDoorObjectFrame.Eye2, 64),
                Step.Show(EyeDoorObjectFrame.Eye1, 6),
                Step.Show(EyeDoorObjectFrame.Eye0, 48),
                Step.Show(EyeDoorObjectFrame.Eye0, 48),
                Step.Show(EyeDoorObjectFrame.Eye1, 6),
                Step.Publish(EyeDoorObjectCue.WatchIfSamusNear),
                Step.GoTo(EyeDoorObjectProgram.EyeIdle),
            ],
            [EyeDoorObjectProgram.EyeClose] =
            [
                Step.Show(EyeDoorObjectFrame.Eye0, 4),
                Step.GoTo(EyeDoorObjectProgram.EyeWatch),
            ],
            [EyeDoorObjectProgram.EyeHit] =
            [
                Step.Publish(EyeDoorObjectCue.QueueHitSound),
                Step.Publish(EyeDoorObjectCue.SpawnTwoSmoke),
                Step.Publish(EyeDoorObjectCue.SpawnTwoSmoke),
                Step.Publish(EyeDoorObjectCue.CountHit),
                Step.Show(EyeDoorObjectFrame.Eye3, 2),
                Step.Show(EyeDoorObjectFrame.Eye4, 2),
                Step.Publish(EyeDoorObjectCue.SpawnTwoSmoke),
                Step.Show(EyeDoorObjectFrame.Eye3, 2),
                Step.Show(EyeDoorObjectFrame.Eye4, 2),
                Step.Show(EyeDoorObjectFrame.Eye3, 2),
                Step.Publish(EyeDoorObjectCue.SpawnTwoSmoke),
                Step.Show(EyeDoorObjectFrame.Eye4, 2),
                Step.Show(EyeDoorObjectFrame.Eye1, 4),
                Step.Show(EyeDoorObjectFrame.Eye0, 8),
                Step.Publish(EyeDoorObjectCue.SpawnSweat),
                Step.Show(EyeDoorObjectFrame.Eye0, 56),
                Step.Show(EyeDoorObjectFrame.Eye1, 4),
                Step.Show(EyeDoorObjectFrame.Eye4, 4),
                Step.GoTo(EyeDoorObjectProgram.EyeWatch),
            ],
            [EyeDoorObjectProgram.EyeDestroyed] =
            [
                Step.Publish(EyeDoorObjectCue.ClearFrameBehavior),
                Step.Publish(EyeDoorObjectCue.SpawnSmoke),
                Step.Publish(EyeDoorObjectCue.SpawnSmoke),
                Step.Publish(EyeDoorObjectCue.SpawnTwoSmoke),
                Step.Publish(EyeDoorObjectCue.SpawnTwoSmoke),
                Step.Publish(EyeDoorObjectCue.MakeBlue),
                Step.SetCounter(OpeningFlashes),
                Step.FallInto(EyeDoorObjectProgram.EyeOpening),
            ],
            [EyeDoorObjectProgram.EyeOpening] =
            [
                Step.Show(EyeDoorObjectFrame.OpeningClear, 3),
                Step.Show(EyeDoorObjectFrame.BlueCap, 4),
                Step.RepeatWhileCounting(EyeDoorObjectProgram.EyeOpening),
                Step.Publish(EyeDoorObjectCue.BecomeBlueDoor),
                Step.Hold,
            ],
            [EyeDoorObjectProgram.EyeOpened] =
            [
                Step.Publish(EyeDoorObjectCue.MakeBlue),
                Step.Publish(EyeDoorObjectCue.BecomeBlueDoor),
                Step.Hold,
            ],
            [EyeDoorObjectProgram.ComponentStart] =
            [
                Step.Publish(EyeDoorObjectCue.DeleteIfOpened),
                Step.FallInto(EyeDoorObjectProgram.ComponentIdle),
            ],
            [EyeDoorObjectProgram.ComponentIdle] =
            [
                Step.Publish(EyeDoorObjectCue.PulseIfSamusNear),
                Step.Show(EyeDoorObjectFrame.Pose0, 8),
                Step.GoTo(EyeDoorObjectProgram.ComponentIdle),
            ],
            [EyeDoorObjectProgram.ComponentActivate] =
            [
                Step.Publish(EyeDoorObjectCue.LinkDelete),
                Step.Publish(EyeDoorObjectCue.ArmDoorBitWake),
                Step.FallInto(EyeDoorObjectProgram.ComponentCycle),
            ],
            [EyeDoorObjectProgram.ComponentCycle] =
            [
                Step.Show(EyeDoorObjectFrame.Pose0, pulse),
                Step.Show(EyeDoorObjectFrame.Pose1, pulse),
                Step.Show(EyeDoorObjectFrame.Pose2, pulse),
                Step.Show(EyeDoorObjectFrame.Pose1, pulse),
                Step.Publish(EyeDoorObjectCue.KeepPulsingIfSamusNear),
                Step.GoTo(EyeDoorObjectProgram.ComponentIdle),
            ],
            [EyeDoorObjectProgram.ComponentDelete] =
            [
                Step.Stop,
            ],
        };
}
