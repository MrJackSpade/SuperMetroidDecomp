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
    internal static readonly (WorkRobotProgram Program, ushort Entry)[] Entries =
    [
        (WorkRobotProgram.NoPowerNeutral, 0xc6d3),
        (WorkRobotProgram.NoPowerLeaningLeft, 0xc6d9),
        (WorkRobotProgram.NoPowerLeaningRight, 0xc6df),
        (WorkRobotProgram.Initial, 0xc6e5),
        (WorkRobotProgram.FacingLeftWalkingForwards, 0xc6e9),
        (WorkRobotProgram.FacingLeftWalkingForwardsLoop, 0xc6ed),
        (WorkRobotProgram.FacingLeftHitWallMovingForwards, 0xc73f),
        (WorkRobotProgram.FacingLeftShotSamusAhead, 0xc7bb),
        (WorkRobotProgram.FacingLeftShotSamusBehind, 0xc833),
        (WorkRobotProgram.FacingLeftShotLaserDownLeft, 0xc8b1),
        (WorkRobotProgram.FacingLeftShotLaserLeft, 0xc8bd),
        (WorkRobotProgram.FacingLeftShotLaserUpLeft, 0xc8d1),
        (WorkRobotProgram.FacingLeftLaserShotRecoil, 0xc8e9),
        (WorkRobotProgram.ApproachingFallRight, 0xc91b),
        (WorkRobotProgram.FacingRightWalkingForwards, 0xc92d),
        (WorkRobotProgram.FacingRightWalkingForwardsLoop, 0xc931),
        (WorkRobotProgram.FacingRightHitWallMovingForwards, 0xc985),
        (WorkRobotProgram.FacingRightShotSamusAhead, 0xca01),
        (WorkRobotProgram.FacingRightShotSamusBehind, 0xca7d),
        (WorkRobotProgram.FacingRightShotLaserDownRight, 0xcafd),
        (WorkRobotProgram.FacingRightShotLaserRight, 0xcb09),
        (WorkRobotProgram.FacingRightShotLaserUpRight, 0xcb1d),
        (WorkRobotProgram.FacingRightLaserShotRecoil, 0xcb35),
        (WorkRobotProgram.ApproachingFallLeft, 0xcb65),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (WorkRobotCueKind Kind, string Operands)> Instructions = new()
    {
        [0xcd09] = (WorkRobotCueKind.LeftForwardWatchingLedges, ""),
        [0xcda4] = (WorkRobotCueKind.LeftForward, ""),
        [0xcdea] = (WorkRobotCueKind.LeftBackwardWatchingLedges, ""),
        [0xce85] = (WorkRobotCueKind.LeftBackward, ""),
        [0xcecb] = (WorkRobotCueKind.WalkRight, ""),
        [0xcecf] = (WorkRobotCueKind.RightForwardWatchingLedges, ""),
        [0xcf6a] = (WorkRobotCueKind.RightForward, ""),
        [0xcfb0] = (WorkRobotCueKind.RightBackwardWatchingLedges, ""),
        [0xd04b] = (WorkRobotCueKind.RightBackward, ""),
        [0xd091] = (WorkRobotCueKind.Footstep, ""),
        [0xd0c2] = (WorkRobotCueKind.WalkLeft, ""),
        [0xd0c6] = (WorkRobotCueKind.TryLaserUpRight, ""),
        [0xd0d2] = (WorkRobotCueKind.TryLaserUpLeft, ""),
        [0xd100] = (WorkRobotCueKind.TryLaserRight, ""),
        [0xd107] = (WorkRobotCueKind.TryLaserLeft, ""),
        [0xd131] = (WorkRobotCueKind.TryLaserDownRight, ""),
        [0xd13d] = (WorkRobotCueKind.TryLaserDownLeft, ""),
        [0xd16b] = (WorkRobotCueKind.CountDownLaserCooldown, ""),
    };
}
