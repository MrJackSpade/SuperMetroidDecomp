using SuperMetroid.Core.Enemies.Flies;

namespace SuperMetroid.Verification.Enemies.Flies;

/// <summary>
/// The native bank-$A2 lists the fly-family ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class FlyNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (FlyProgram Program, ushort Entry)[] Entries =
    [
        (FlyProgram.Flight, 0xb013),
    ];

}
