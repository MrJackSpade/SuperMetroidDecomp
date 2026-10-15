using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>
/// The native bank-$A9 lists the Dead Torizo ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class DeadTorizoNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa9;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (DeadTorizoProgram Program, ushort Entry)[] Entries =
    [
        (DeadTorizoProgram.Stationary, 0xd6dc),
    ];

}
