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
    internal static readonly (BeetomProgram Program, ushort Entry)[] Entries =
    [
        (BeetomProgram.CrawlingLeft, 0xb696),
        (BeetomProgram.CrawlingLeftLoop, 0xb698),
        (BeetomProgram.HopLeft, 0xb6ac),
        (BeetomProgram.DrainingLeft, 0xb6cc),
        (BeetomProgram.DrainingLeftLoop, 0xb6de),
        (BeetomProgram.CrawlingRight, 0xb6f2),
        (BeetomProgram.CrawlingRightLoop, 0xb6f4),
        (BeetomProgram.HopRight, 0xb708),
        (BeetomProgram.DrainingRight, 0xb728),
        (BeetomProgram.DrainingRightLoop, 0xb73a),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (BeetomCueKind Kind, string Operands)> Instructions = new()
    {
        [0x8173] = (BeetomCueKind.ProcessOffScreen, ""),
        [0x817d] = (BeetomCueKind.StopProcessingOffScreen, ""),
        [0xb75e] = (BeetomCueKind.Nothing, ""),
    };
}
