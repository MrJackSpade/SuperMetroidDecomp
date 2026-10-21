using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<GunshipProgram>[] Entries =
    [
        new NativeProgramEntry<GunshipProgram>(GunshipProgram.EntrancePadOpening, 0xa5be),
        new NativeProgramEntry<GunshipProgram>(GunshipProgram.EntrancePadOpen, 0xa5e6),
        new NativeProgramEntry<GunshipProgram>(GunshipProgram.EntrancePadClosing, 0xa5ee),
        new NativeProgramEntry<GunshipProgram>(GunshipProgram.BottomEntrancePad, 0xa60e),
        new NativeProgramEntry<GunshipProgram>(GunshipProgram.TopHull, 0xa616),
        new NativeProgramEntry<GunshipProgram>(GunshipProgram.BottomHull, 0xa61c),
    ];

}
