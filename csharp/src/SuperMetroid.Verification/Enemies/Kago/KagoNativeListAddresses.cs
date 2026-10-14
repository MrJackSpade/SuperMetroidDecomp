using SuperMetroid.Core.Enemies.Kago;

namespace SuperMetroid.Verification.Enemies.Kago;

/// <summary>
/// The native bank-$A8 lists the Kago ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class KagoNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (KagoProgram Program, ushort Entry)[] Entries =
    [
        (KagoProgram.Slow, 0xab1e),
        (KagoProgram.Fast, 0xab32),
    ];

}
