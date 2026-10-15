using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// The native bank-$A2 lists the Rinka ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class RinkaNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (RinkaProgram Program, ushort Entry)[] Entries =
    [
        (RinkaProgram.Ordinary, 0xb9e0),
        (RinkaProgram.OrdinaryLoop, 0xb9e8),
        (RinkaProgram.Special, 0xba0c),
        (RinkaProgram.SpecialLoop, 0xba14),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (RinkaCueKind Kind, string Operands)> Instructions = new()
    {
        [0xb9b3] = (RinkaCueKind.Hide, ""),
        [0xb9bd] = (RinkaCueKind.HideAndProcessOffScreen, ""),
        [0xb9c7] = (RinkaCueKind.Fire, ""),
    };
}
