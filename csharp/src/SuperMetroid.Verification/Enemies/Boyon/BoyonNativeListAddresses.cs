using SuperMetroid.Core.Enemies.Boyon;

namespace SuperMetroid.Verification.Enemies.Boyon;

/// <summary>
/// The native bank-$A2 lists the Boyon ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class BoyonNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (BoyonProgram Program, ushort Entry)[] Entries =
    [
        (BoyonProgram.Idle, 0x86a7),
        (BoyonProgram.IdleLoop, 0x86ab),
        (BoyonProgram.Bouncing, 0x86bf),
        (BoyonProgram.BouncingLoop, 0x86c3),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (BoyonCueKind Kind, string Operands)> Instructions = new()
    {
        [0x8173] = (BoyonCueKind.ProcessOffScreen, ""),
        [0x817d] = (BoyonCueKind.StopProcessingOffScreen, ""),
        [0x88c5] = (BoyonCueKind.Return, ""),
        [0x88c6] = (BoyonCueKind.StartBounce, ""),
    };
}
