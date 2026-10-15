using SuperMetroid.Core.Enemies.MamaTurtles;

namespace SuperMetroid.Verification.Enemies.MamaTurtles;

/// <summary>
/// The native bank-$A2 lists the Mama Turtle ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class MamaTurtleNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (MamaTurtleProgram Program, ushort Entry)[] Entries =
    [
        (MamaTurtleProgram.BabyCrawlingLeft, 0x8b80),
        (MamaTurtleProgram.BabySpinning, 0x8bd2),
        (MamaTurtleProgram.MamaSpinning, 0x8c02),
        (MamaTurtleProgram.MamaEnterShellLeft, 0x8c1c),
        (MamaTurtleProgram.BabyHidingLeft, 0x8c30),
        (MamaTurtleProgram.MamaAsleep, 0x8c44),
        (MamaTurtleProgram.MamaLeaveShellLeft, 0x8c4a),
        (MamaTurtleProgram.BabyLeaveShellLeft, 0x8c62),
        (MamaTurtleProgram.BabyCrawlingRight, 0x8c72),
        (MamaTurtleProgram.MamaEnterShellRight, 0x8d00),
        (MamaTurtleProgram.BabyHidingRight, 0x8d14),
        (MamaTurtleProgram.MamaLeaveShellRight, 0x8d28),
        (MamaTurtleProgram.BabyLeaveShellRight, 0x8d40),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (MamaTurtleCueKind Kind, string Operands)> Instructions = new()
    {
        [0x9381] = (MamaTurtleCueKind.Crawl, ""),
        [0x9412] = (MamaTurtleCueKind.LoopOrTurnAround, ""),
        [0x9447] = (MamaTurtleCueKind.EnterShell, ""),
        [0x9451] = (MamaTurtleCueKind.RiseToHoverRightwards, ""),
        [0x946b] = (MamaTurtleCueKind.RiseToHoverLeftwards, ""),
        [0x9485] = (MamaTurtleCueKind.LeaveShell, ""),
        [0x94a1] = (MamaTurtleCueKind.LeftShell, ""),
        [0x94c7] = (MamaTurtleCueKind.SetSpinningStoppable, ""),
        [0x94d1] = (MamaTurtleCueKind.PlaySpinningSound, ""),
    };
}
