using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.WorkRobots;

namespace SuperMetroid.Verification.Enemies.WorkRobots;

/// <summary>
/// The native bank-$A8 lists the Work Robot ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class WorkRobotNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<WorkRobotSegment>[] Entries =
    [
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.NoPowerNeutral, 0xc6d3),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.NoPowerLeaningLeft, 0xc6d9),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.NoPowerLeaningRight, 0xc6df),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.Initial, 0xc6e5),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingLeftWalkingForwards, 0xc6e9),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingLeftWalkingForwardsLoop, 0xc6ed),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingLeftHitWallMovingForwards, 0xc73f),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingLeftShotSamusAhead, 0xc7bb),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingLeftShotSamusBehind, 0xc833),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingLeftShotLaserDownLeft, 0xc8b1),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingLeftShotLaserLeft, 0xc8bd),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingLeftShotLaserUpLeft, 0xc8d1),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingLeftLaserShotRecoil, 0xc8e9),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.ApproachingFallRight, 0xc91b),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingRightWalkingForwards, 0xc92d),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingRightWalkingForwardsLoop, 0xc931),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingRightHitWallMovingForwards, 0xc985),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingRightShotSamusAhead, 0xca01),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingRightShotSamusBehind, 0xca7d),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingRightShotLaserDownRight, 0xcafd),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingRightShotLaserRight, 0xcb09),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingRightShotLaserUpRight, 0xcb1d),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.FacingRightLaserShotRecoil, 0xcb35),
        new NativeProgramEntry<WorkRobotSegment>(WorkRobotSegment.ApproachingFallLeft, 0xcb65),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, WorkRobotNativeListAddressesInstruction> Instructions = new()
    {
        [0xcd09] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftForwardWatchingLedges), ""),
        [0xcda4] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftForward), ""),
        [0xcdea] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges), ""),
        [0xce85] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackward), ""),
        [0xcecb] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.Walk(WorkRobotSegment.FacingRightWalkingForwards), ""),
        [0xcecf] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightForwardWatchingLedges), ""),
        [0xcf6a] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightForward), ""),
        [0xcfb0] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges), ""),
        [0xd04b] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackward), ""),
        [0xd091] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.Signal(WorkRobotCueKind.Footstep), ""),
        [0xd0c2] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.Walk(WorkRobotSegment.FacingLeftWalkingForwards), ""),
        [0xd0c6] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.TryLaser(WorkRobotLaser.UpRight, WorkRobotSegment.FacingRightShotLaserUpRight), ""),
        [0xd0d2] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.TryLaser(WorkRobotLaser.UpLeft, WorkRobotSegment.FacingLeftShotLaserUpLeft), ""),
        [0xd100] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.TryLaser(WorkRobotLaser.Horizontal, WorkRobotSegment.FacingRightShotLaserRight), ""),
        [0xd107] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.TryLaser(WorkRobotLaser.Horizontal, WorkRobotSegment.FacingLeftShotLaserLeft), ""),
        [0xd131] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.TryLaser(WorkRobotLaser.DownRight, WorkRobotSegment.FacingRightShotLaserDownRight), ""),
        [0xd13d] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.TryLaser(WorkRobotLaser.DownLeft, WorkRobotSegment.FacingLeftShotLaserDownLeft), ""),
        [0xd16b] = new WorkRobotNativeListAddressesInstruction(new WorkRobotCue.Signal(WorkRobotCueKind.CountDownLaserCooldown), ""),
    };
}
