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
    internal static readonly NativeProgramEntry<RinkaProgram>[] Entries =
    [
        new NativeProgramEntry<RinkaProgram>(RinkaProgram.Ordinary, 0xb9e0),
        new NativeProgramEntry<RinkaProgram>(RinkaProgram.OrdinaryLoop, 0xb9e8),
        new NativeProgramEntry<RinkaProgram>(RinkaProgram.Special, 0xba0c),
        new NativeProgramEntry<RinkaProgram>(RinkaProgram.SpecialLoop, 0xba14),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, RinkaNativeListAddressesInstruction> Instructions = new()
    {
        [0xb9b3] = new RinkaNativeListAddressesInstruction(RinkaCueKind.Hide, ""),
        [0xb9bd] = new RinkaNativeListAddressesInstruction(RinkaCueKind.HideAndProcessOffScreen, ""),
        [0xb9c7] = new RinkaNativeListAddressesInstruction(RinkaCueKind.Fire, ""),
    };
}
