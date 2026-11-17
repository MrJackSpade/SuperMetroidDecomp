using SuperMetroid.TestSupport.MovieReplay;
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

    /// <summary>Each of the Mama Turtle's typed programs' native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<MamaTurtleSegment>[] Entries =
    [
        new NativeProgramEntry<MamaTurtleSegment>(MamaTurtleSegment.Spinning, 0x8c02),
        new NativeProgramEntry<MamaTurtleSegment>(MamaTurtleSegment.EnterShellLeft, 0x8c1c),
        new NativeProgramEntry<MamaTurtleSegment>(MamaTurtleSegment.Asleep, 0x8c44),
        new NativeProgramEntry<MamaTurtleSegment>(MamaTurtleSegment.LeaveShellLeft, 0x8c4a),
        new NativeProgramEntry<MamaTurtleSegment>(MamaTurtleSegment.EnterShellRight, 0x8d00),
        new NativeProgramEntry<MamaTurtleSegment>(MamaTurtleSegment.LeaveShellRight, 0x8d28),
    ];

    /// <summary>Each of the baby turtles' typed programs' native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<BabyTurtleSegment>[] BabyEntries =
    [
        new NativeProgramEntry<BabyTurtleSegment>(BabyTurtleSegment.CrawlingLeft, 0x8b80),
        new NativeProgramEntry<BabyTurtleSegment>(BabyTurtleSegment.Spinning, 0x8bd2),
        new NativeProgramEntry<BabyTurtleSegment>(BabyTurtleSegment.HidingLeft, 0x8c30),
        new NativeProgramEntry<BabyTurtleSegment>(BabyTurtleSegment.LeaveShell, 0x8c62),
        new NativeProgramEntry<BabyTurtleSegment>(BabyTurtleSegment.CrawlingRight, 0x8c72),
        new NativeProgramEntry<BabyTurtleSegment>(BabyTurtleSegment.HidingRight, 0x8d14),
        new NativeProgramEntry<BabyTurtleSegment>(BabyTurtleSegment.LeaveShell, 0x8d40),
    ];

    /// <summary>Each owner instruction the Mama Turtle's lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, MamaTurtleNativeListAddressesInstruction> Instructions = new()
    {
        [0x9447] = new MamaTurtleNativeListAddressesInstruction(new MamaTurtleCue.Signal(MamaTurtleCueKind.EnterShell), ""),
        [0x9451] = new MamaTurtleNativeListAddressesInstruction(new MamaTurtleCue.RiseToHover(HorizontalDirection.Left), ""),
        [0x946b] = new MamaTurtleNativeListAddressesInstruction(new MamaTurtleCue.RiseToHover(HorizontalDirection.Right), ""),
        [0x94d1] = new MamaTurtleNativeListAddressesInstruction(new MamaTurtleCue.Signal(MamaTurtleCueKind.PlaySpinningSound), ""),
    };

    /// <summary>Each owner instruction the baby turtles' lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, BabyTurtleNativeListAddressesInstruction> BabyInstructions = new()
    {
        [0x9381] = new BabyTurtleNativeListAddressesInstruction(new BabyTurtleCue(BabyTurtleCueKind.Crawl), ""),
        [0x9412] = new BabyTurtleNativeListAddressesInstruction(new BabyTurtleCue(BabyTurtleCueKind.LoopOrTurnAround), ""),
        [0x9485] = new BabyTurtleNativeListAddressesInstruction(new BabyTurtleCue(BabyTurtleCueKind.LeaveShell), ""),
        [0x94a1] = new BabyTurtleNativeListAddressesInstruction(new BabyTurtleCue(BabyTurtleCueKind.LeftShell), ""),
        [0x94c7] = new BabyTurtleNativeListAddressesInstruction(new BabyTurtleCue(BabyTurtleCueKind.SetSpinningStoppable), ""),
        [0x94d1] = new BabyTurtleNativeListAddressesInstruction(new BabyTurtleCue(BabyTurtleCueKind.PlaySpinningSound), ""),
    };
}
