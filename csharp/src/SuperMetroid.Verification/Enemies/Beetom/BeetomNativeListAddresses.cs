using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Beetom;

namespace SuperMetroid.Verification.Enemies.Beetom;

/// <summary>
/// The native bank-$A8 lists the Beetom ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class BeetomNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<BeetomProgram>[] Entries =
    [
        new NativeProgramEntry<BeetomProgram>(BeetomProgram.CrawlingLeft, 0xb696),
        new NativeProgramEntry<BeetomProgram>(BeetomProgram.CrawlingLeftLoop, 0xb698),
        new NativeProgramEntry<BeetomProgram>(BeetomProgram.HopLeft, 0xb6ac),
        new NativeProgramEntry<BeetomProgram>(BeetomProgram.DrainingLeft, 0xb6cc),
        new NativeProgramEntry<BeetomProgram>(BeetomProgram.DrainingLeftLoop, 0xb6de),
        new NativeProgramEntry<BeetomProgram>(BeetomProgram.CrawlingRight, 0xb6f2),
        new NativeProgramEntry<BeetomProgram>(BeetomProgram.CrawlingRightLoop, 0xb6f4),
        new NativeProgramEntry<BeetomProgram>(BeetomProgram.HopRight, 0xb708),
        new NativeProgramEntry<BeetomProgram>(BeetomProgram.DrainingRight, 0xb728),
        new NativeProgramEntry<BeetomProgram>(BeetomProgram.DrainingRightLoop, 0xb73a),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, BeetomNativeListAddressesInstruction> Instructions = new()
    {
        [0x8173] = new BeetomNativeListAddressesInstruction(BeetomCueKind.ProcessOffScreen, ""),
        [0x817d] = new BeetomNativeListAddressesInstruction(BeetomCueKind.StopProcessingOffScreen, ""),
        [0xb75e] = new BeetomNativeListAddressesInstruction(BeetomCueKind.Nothing, ""),
    };
}
