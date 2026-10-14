using SuperMetroid.Core.Enemies.LavaJumpers;

namespace SuperMetroid.Verification.Enemies.LavaJumpers;

/// <summary>
/// The native bank-$A2 lists the Norfair lava jumper ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class NorfairLavaJumperNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (NorfairLavaJumperProgram Program, ushort Entry)[] Entries =
    [
        (NorfairLavaJumperProgram.Hidden, 0xbe3c),
        (NorfairLavaJumperProgram.Jump, 0xbe42),
        (NorfairLavaJumperProgram.Follower, 0xbe62),
        (NorfairLavaJumperProgram.FollowerLoop, 0xbe6e),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (NorfairLavaJumperCueKind Kind, string Operands)> Instructions = new()
    {
        [0xbe8e] = (NorfairLavaJumperCueKind.FinishAnimation, ""),
    };
}
