using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<WorkRobotProgram>[] Entries =
    [
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.NoPowerNeutral, 0xc6d3),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.NoPowerLeaningLeft, 0xc6d9),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.NoPowerLeaningRight, 0xc6df),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.Initial, 0xc6e5),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingLeftWalkingForwards, 0xc6e9),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingLeftWalkingForwardsLoop, 0xc6ed),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingLeftHitWallMovingForwards, 0xc73f),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingLeftShotSamusAhead, 0xc7bb),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingLeftShotSamusBehind, 0xc833),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingLeftShotLaserDownLeft, 0xc8b1),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingLeftShotLaserLeft, 0xc8bd),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingLeftShotLaserUpLeft, 0xc8d1),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingLeftLaserShotRecoil, 0xc8e9),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.ApproachingFallRight, 0xc91b),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingRightWalkingForwards, 0xc92d),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingRightWalkingForwardsLoop, 0xc931),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingRightHitWallMovingForwards, 0xc985),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingRightShotSamusAhead, 0xca01),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingRightShotSamusBehind, 0xca7d),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingRightShotLaserDownRight, 0xcafd),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingRightShotLaserRight, 0xcb09),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingRightShotLaserUpRight, 0xcb1d),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.FacingRightLaserShotRecoil, 0xcb35),
        new NativeProgramEntry<WorkRobotProgram>(WorkRobotProgram.ApproachingFallLeft, 0xcb65),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, WorkRobotNativeListAddressesInstruction> Instructions = new()
    {
        [0xcd09] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.LeftForwardWatchingLedges, ""),
        [0xcda4] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.LeftForward, ""),
        [0xcdea] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.LeftBackwardWatchingLedges, ""),
        [0xce85] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.LeftBackward, ""),
        [0xcecb] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.WalkRight, ""),
        [0xcecf] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.RightForwardWatchingLedges, ""),
        [0xcf6a] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.RightForward, ""),
        [0xcfb0] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.RightBackwardWatchingLedges, ""),
        [0xd04b] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.RightBackward, ""),
        [0xd091] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.Footstep, ""),
        [0xd0c2] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.WalkLeft, ""),
        [0xd0c6] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.TryLaserUpRight, ""),
        [0xd0d2] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.TryLaserUpLeft, ""),
        [0xd100] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.TryLaserRight, ""),
        [0xd107] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.TryLaserLeft, ""),
        [0xd131] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.TryLaserDownRight, ""),
        [0xd13d] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.TryLaserDownLeft, ""),
        [0xd16b] = new WorkRobotNativeListAddressesInstruction(WorkRobotCueKind.CountDownLaserCooldown, ""),
    };
}
