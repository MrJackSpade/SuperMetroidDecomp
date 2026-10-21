using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<MamaTurtleProgram>[] Entries =
    [
        new NativeProgramEntry<MamaTurtleProgram>(MamaTurtleProgram.BabyCrawlingLeft, 0x8b80),
        new NativeProgramEntry<MamaTurtleProgram>(MamaTurtleProgram.BabySpinning, 0x8bd2),
        new NativeProgramEntry<MamaTurtleProgram>(MamaTurtleProgram.MamaSpinning, 0x8c02),
        new NativeProgramEntry<MamaTurtleProgram>(MamaTurtleProgram.MamaEnterShellLeft, 0x8c1c),
        new NativeProgramEntry<MamaTurtleProgram>(MamaTurtleProgram.BabyHidingLeft, 0x8c30),
        new NativeProgramEntry<MamaTurtleProgram>(MamaTurtleProgram.MamaAsleep, 0x8c44),
        new NativeProgramEntry<MamaTurtleProgram>(MamaTurtleProgram.MamaLeaveShellLeft, 0x8c4a),
        new NativeProgramEntry<MamaTurtleProgram>(MamaTurtleProgram.BabyLeaveShellLeft, 0x8c62),
        new NativeProgramEntry<MamaTurtleProgram>(MamaTurtleProgram.BabyCrawlingRight, 0x8c72),
        new NativeProgramEntry<MamaTurtleProgram>(MamaTurtleProgram.MamaEnterShellRight, 0x8d00),
        new NativeProgramEntry<MamaTurtleProgram>(MamaTurtleProgram.BabyHidingRight, 0x8d14),
        new NativeProgramEntry<MamaTurtleProgram>(MamaTurtleProgram.MamaLeaveShellRight, 0x8d28),
        new NativeProgramEntry<MamaTurtleProgram>(MamaTurtleProgram.BabyLeaveShellRight, 0x8d40),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, MamaTurtleNativeListAddressesInstruction> Instructions = new()
    {
        [0x9381] = new MamaTurtleNativeListAddressesInstruction(MamaTurtleCueKind.Crawl, ""),
        [0x9412] = new MamaTurtleNativeListAddressesInstruction(MamaTurtleCueKind.LoopOrTurnAround, ""),
        [0x9447] = new MamaTurtleNativeListAddressesInstruction(MamaTurtleCueKind.EnterShell, ""),
        [0x9451] = new MamaTurtleNativeListAddressesInstruction(MamaTurtleCueKind.RiseToHoverRightwards, ""),
        [0x946b] = new MamaTurtleNativeListAddressesInstruction(MamaTurtleCueKind.RiseToHoverLeftwards, ""),
        [0x9485] = new MamaTurtleNativeListAddressesInstruction(MamaTurtleCueKind.LeaveShell, ""),
        [0x94a1] = new MamaTurtleNativeListAddressesInstruction(MamaTurtleCueKind.LeftShell, ""),
        [0x94c7] = new MamaTurtleNativeListAddressesInstruction(MamaTurtleCueKind.SetSpinningStoppable, ""),
        [0x94d1] = new MamaTurtleNativeListAddressesInstruction(MamaTurtleCueKind.PlaySpinningSound, ""),
    };
}
