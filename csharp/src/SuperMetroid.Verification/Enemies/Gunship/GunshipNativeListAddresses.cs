using SuperMetroid.Core.Enemies.Gunship;

namespace SuperMetroid.Verification.Enemies.Gunship;

/// <summary>
/// The native bank-$A2 lists the gunship ROM oracle (#1290) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class GunshipNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (GunshipProgram Program, ushort Entry)[] Entries =
    [
        (GunshipProgram.EntrancePadOpening, 0xa5be),
        (GunshipProgram.EntrancePadOpen, 0xa5e6),
        (GunshipProgram.EntrancePadClosing, 0xa5ee),
        (GunshipProgram.BottomEntrancePad, 0xa60e),
        (GunshipProgram.TopHull, 0xa616),
        (GunshipProgram.BottomHull, 0xa61c),
    ];

}
