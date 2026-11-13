using SuperMetroid.TestSupport.MovieReplay;
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
    internal static readonly NativeProgramEntry<GunshipSegment>[] Entries =
    [
        new NativeProgramEntry<GunshipSegment>(GunshipSegment.EntrancePadOpening, 0xa5be),
        new NativeProgramEntry<GunshipSegment>(GunshipSegment.EntrancePadOpen, 0xa5e6),
        new NativeProgramEntry<GunshipSegment>(GunshipSegment.EntrancePadClosing, 0xa5ee),
        new NativeProgramEntry<GunshipSegment>(GunshipSegment.BottomEntrancePad, 0xa60e),
        new NativeProgramEntry<GunshipSegment>(GunshipSegment.TopHull, 0xa616),
        new NativeProgramEntry<GunshipSegment>(GunshipSegment.BottomHull, 0xa61c),
    ];

}
