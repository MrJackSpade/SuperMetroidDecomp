using SuperMetroid.Core.Enemies.WreckedShip;

namespace SuperMetroid.Verification.Enemies.WreckedShip;

/// <summary>
/// The native bank-$A8 lists the Wrecked Ship ghost ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class WreckedShipGhostNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (WreckedShipGhostProgram Program, ushort Entry)[] Entries =
    [
        (WreckedShipGhostProgram.Floating, 0x9a8c),
    ];

}
