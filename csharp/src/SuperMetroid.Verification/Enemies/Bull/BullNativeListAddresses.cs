using SuperMetroid.Core.Enemies.Bull;

namespace SuperMetroid.Verification.Enemies.Bull;

/// <summary>
/// The native bank-$A8 lists the Bull ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class BullNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (BullProgram Program, ushort Entry)[] Entries =
    [
        (BullProgram.Normal, 0xd841),
        (BullProgram.Shot, 0xd855),
        (BullProgram.ShotLoop, 0xd859),
    ];

}
