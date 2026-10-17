using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Doors.EyeDoorPlmProgram, SuperMetroid.Core.Rooms.Doors.EyeDoorPlmFrame, SuperMetroid.Core.Rooms.Doors.EyeDoorPlmCue>;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The eye door's PLM programs (#1296), in place of its bank-$84 lists at $84:D81E..DA8B. Both facings run the same
/// programs; the right-facing components pulse faster, and each facing mirrors its draws and projectile operands.
/// </summary>
internal static class EyeDoorPlmProgramDefinitions
{
    /// <summary>The library-2 sound each hit on the eye queues.</summary>
    internal const byte HitSound = 9;
    /// <summary>The hit sound's queue cap.</summary>
    internal const byte HitSoundMaximumQueued = 6;
    /// <summary>The hits the eye takes before the door opens.</summary>
    internal const byte HitThreshold = 3;
    /// <summary>The hit counter a Super Missile seeds: the next count passes the threshold.</summary>
    internal const byte SuperMissileHitCounter = 119;
    /// <summary>How many times the opening eye flashes to the blue cap.</summary>
    private const int OpeningFlashes = 10;
    /// <summary>How long a left-facing component shows each pulse pose.</summary>
    private const int LeftPulseDuration = 8;
    /// <summary>How long a right-facing component shows each pulse pose.</summary>
    private const int RightPulseDuration = 6;
    /// <summary>The projectile operand a right-facing eye's shots and sweat add.</summary>
    private const ushort RightShotParameter = 20;
    /// <summary>The sweat operand of a right-facing eye.</summary>
    private const ushort RightSweatParameter = 4;
    /// <summary>The blocks across that wake the eye or keep it watching.</summary>
    private const byte EyeWakeColumns = 6;
    /// <summary>The blocks up or down that wake the eye, keep it watching, or close it.</summary>
    private const byte EyeWakeRows = 4;
    /// <summary>The blocks across that count as right in front of the eye.</summary>
    private const byte EyeInFrontColumns = 1;
    /// <summary>The blocks across that make a component pulse.</summary>
    private const byte ComponentWakeColumns = 6;
    /// <summary>The blocks up or down that make a component pulse.</summary>
    private const byte ComponentWakeRows = 16;

    /// <summary>Each facing's program set.</summary>
    private static readonly Dictionary<EyeDoorOrientation, AnimationProgramSet<EyeDoorPlmProgram, EyeDoorPlmFrame, EyeDoorPlmCue>> Sets =
        Enum.GetValues<EyeDoorOrientation>().ToDictionary(orientation => orientation,
            orientation => new AnimationProgramSet<EyeDoorPlmProgram, EyeDoorPlmFrame, EyeDoorPlmCue>(Build(orientation)));

    /// <summary>The programs of the door facing <paramref name="orientation"/>.</summary>
    internal static AnimationProgramSet<EyeDoorPlmProgram, EyeDoorPlmFrame, EyeDoorPlmCue> ProgramsFor(EyeDoorOrientation orientation) =>
        Sets[orientation];

    /// <summary>The projectile operand the eye's shots carry.</summary>
    internal static ushort ShotParameterOf(EyeDoorOrientation orientation) => orientation switch
    {
        EyeDoorOrientation.Left => 0,
        EyeDoorOrientation.Right => RightShotParameter,
        _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {orientation}."),
    };

    /// <summary>The projectile operand the eye's sweat carries.</summary>
    internal static ushort SweatParameterOf(EyeDoorOrientation orientation) => orientation switch
    {
        EyeDoorOrientation.Left => 0,
        EyeDoorOrientation.Right => RightSweatParameter,
        _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {orientation}."),
    };

    /// <summary>The operands of a proximity cue.</summary>
    internal static EyeDoorProximity ProximityOf(EyeDoorPlmCue cue) => cue switch
    {
        EyeDoorPlmCue.ActivateIfSamusNear => new(new(EyeWakeColumns, EyeWakeRows), EyeDoorPlmProgram.EyeActivate),
        EyeDoorPlmCue.CloseIfSamusInFront => new(new(EyeInFrontColumns, EyeWakeRows), EyeDoorPlmProgram.EyeClose),
        EyeDoorPlmCue.WatchIfSamusNear => new(new(EyeWakeColumns, EyeWakeRows), EyeDoorPlmProgram.EyeWatch),
        EyeDoorPlmCue.PulseIfSamusNear => new(new(ComponentWakeColumns, ComponentWakeRows), EyeDoorPlmProgram.ComponentActivate),
        EyeDoorPlmCue.KeepPulsingIfSamusNear => new(new(ComponentWakeColumns, ComponentWakeRows), EyeDoorPlmProgram.ComponentCycle),
        _ => throw new InvalidOperationException($"{nameof(EyeDoorPlmCue)} {cue} is not a proximity branch."),
    };

    /// <summary>One facing's programs.</summary>
    private static Dictionary<EyeDoorPlmProgram, Step[]> Build(EyeDoorOrientation orientation)
    {
        int pulse = orientation == EyeDoorOrientation.Right ? RightPulseDuration : LeftPulseDuration;
        return new()
        {
            [EyeDoorPlmProgram.EyeStart] =
            [
                Step.Publish(EyeDoorPlmCue.ConvertIfOpened),
                Step.FallInto(EyeDoorPlmProgram.EyeIdle),
            ],
            [EyeDoorPlmProgram.EyeIdle] =
            [
                Step.Show(EyeDoorPlmFrame.Eye0, 4),
                Step.Publish(EyeDoorPlmCue.ActivateIfSamusNear),
                Step.GoTo(EyeDoorPlmProgram.EyeIdle),
            ],
            [EyeDoorPlmProgram.EyeActivate] =
            [
                Step.Publish(EyeDoorPlmCue.LinkHit),
                Step.Publish(EyeDoorPlmCue.ArmMissileWake),
                Step.Show(EyeDoorPlmFrame.Eye1, 8),
                Step.FallInto(EyeDoorPlmProgram.EyeWatch),
            ],
            [EyeDoorPlmProgram.EyeWatch] =
            [
                Step.Publish(EyeDoorPlmCue.CloseIfSamusInFront),
                Step.Show(EyeDoorPlmFrame.Eye2, 64),
                Step.Publish(EyeDoorPlmCue.Shoot),
                Step.Show(EyeDoorPlmFrame.Eye2, 32),
                Step.Publish(EyeDoorPlmCue.Shoot),
                Step.Show(EyeDoorPlmFrame.Eye2, 32),
                Step.Publish(EyeDoorPlmCue.Shoot),
                Step.Show(EyeDoorPlmFrame.Eye2, 64),
                Step.Show(EyeDoorPlmFrame.Eye1, 6),
                Step.Show(EyeDoorPlmFrame.Eye0, 48),
                Step.Show(EyeDoorPlmFrame.Eye0, 48),
                Step.Show(EyeDoorPlmFrame.Eye1, 6),
                Step.Publish(EyeDoorPlmCue.WatchIfSamusNear),
                Step.GoTo(EyeDoorPlmProgram.EyeIdle),
            ],
            [EyeDoorPlmProgram.EyeClose] =
            [
                Step.Show(EyeDoorPlmFrame.Eye0, 4),
                Step.GoTo(EyeDoorPlmProgram.EyeWatch),
            ],
            [EyeDoorPlmProgram.EyeHit] =
            [
                Step.Publish(EyeDoorPlmCue.QueueHitSound),
                Step.Publish(EyeDoorPlmCue.SpawnTwoSmoke),
                Step.Publish(EyeDoorPlmCue.SpawnTwoSmoke),
                Step.Publish(EyeDoorPlmCue.CountHit),
                Step.Show(EyeDoorPlmFrame.Eye3, 2),
                Step.Show(EyeDoorPlmFrame.Eye4, 2),
                Step.Publish(EyeDoorPlmCue.SpawnTwoSmoke),
                Step.Show(EyeDoorPlmFrame.Eye3, 2),
                Step.Show(EyeDoorPlmFrame.Eye4, 2),
                Step.Show(EyeDoorPlmFrame.Eye3, 2),
                Step.Publish(EyeDoorPlmCue.SpawnTwoSmoke),
                Step.Show(EyeDoorPlmFrame.Eye4, 2),
                Step.Show(EyeDoorPlmFrame.Eye1, 4),
                Step.Show(EyeDoorPlmFrame.Eye0, 8),
                Step.Publish(EyeDoorPlmCue.SpawnSweat),
                Step.Show(EyeDoorPlmFrame.Eye0, 56),
                Step.Show(EyeDoorPlmFrame.Eye1, 4),
                Step.Show(EyeDoorPlmFrame.Eye4, 4),
                Step.GoTo(EyeDoorPlmProgram.EyeWatch),
            ],
            [EyeDoorPlmProgram.EyeDestroyed] =
            [
                Step.Publish(EyeDoorPlmCue.ClearPreInstruction),
                Step.Publish(EyeDoorPlmCue.SpawnSmoke),
                Step.Publish(EyeDoorPlmCue.SpawnSmoke),
                Step.Publish(EyeDoorPlmCue.SpawnTwoSmoke),
                Step.Publish(EyeDoorPlmCue.SpawnTwoSmoke),
                Step.Publish(EyeDoorPlmCue.MakeBlue),
                Step.SetCounter(OpeningFlashes),
                Step.FallInto(EyeDoorPlmProgram.EyeOpening),
            ],
            [EyeDoorPlmProgram.EyeOpening] =
            [
                Step.Show(EyeDoorPlmFrame.OpeningClear, 3),
                Step.Show(EyeDoorPlmFrame.BlueCap, 4),
                Step.RepeatWhileCounting(EyeDoorPlmProgram.EyeOpening),
                Step.Publish(EyeDoorPlmCue.BecomeBlueDoor),
                Step.Hold,
            ],
            [EyeDoorPlmProgram.EyeOpened] =
            [
                Step.Publish(EyeDoorPlmCue.MakeBlue),
                Step.Publish(EyeDoorPlmCue.BecomeBlueDoor),
                Step.Hold,
            ],
            [EyeDoorPlmProgram.ComponentStart] =
            [
                Step.Publish(EyeDoorPlmCue.DeleteIfOpened),
                Step.FallInto(EyeDoorPlmProgram.ComponentIdle),
            ],
            [EyeDoorPlmProgram.ComponentIdle] =
            [
                Step.Publish(EyeDoorPlmCue.PulseIfSamusNear),
                Step.Show(EyeDoorPlmFrame.Pose0, 8),
                Step.GoTo(EyeDoorPlmProgram.ComponentIdle),
            ],
            [EyeDoorPlmProgram.ComponentActivate] =
            [
                Step.Publish(EyeDoorPlmCue.LinkDelete),
                Step.Publish(EyeDoorPlmCue.ArmDoorBitWake),
                Step.FallInto(EyeDoorPlmProgram.ComponentCycle),
            ],
            [EyeDoorPlmProgram.ComponentCycle] =
            [
                Step.Show(EyeDoorPlmFrame.Pose0, pulse),
                Step.Show(EyeDoorPlmFrame.Pose1, pulse),
                Step.Show(EyeDoorPlmFrame.Pose2, pulse),
                Step.Show(EyeDoorPlmFrame.Pose1, pulse),
                Step.Publish(EyeDoorPlmCue.KeepPulsingIfSamusNear),
                Step.GoTo(EyeDoorPlmProgram.ComponentIdle),
            ],
            [EyeDoorPlmProgram.ComponentDelete] =
            [
                Step.Stop,
            ],
        };
    }
}
