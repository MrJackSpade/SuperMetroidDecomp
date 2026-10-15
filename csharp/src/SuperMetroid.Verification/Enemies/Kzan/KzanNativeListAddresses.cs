using SuperMetroid.Core.Enemies.Kzan;

namespace SuperMetroid.Verification.Enemies.Kzan;

/// <summary>
/// The native bank-$A6 lists the Kzan ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class KzanNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (KzanProgram Program, ushort Entry)[] Entries =
    [
        (KzanProgram.Idle, 0x8b29),
    ];

}
