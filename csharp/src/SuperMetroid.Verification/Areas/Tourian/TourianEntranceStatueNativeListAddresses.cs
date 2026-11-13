using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Areas.Tourian;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>
/// The native bank-$AA lists the Tourian entrance statue ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class TourianEntranceStatueNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xaa;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<TourianEntranceStatueSegment>[] Entries =
    [
        new NativeProgramEntry<TourianEntranceStatueSegment>(TourianEntranceStatueSegment.Ridley, 0xd7a5),
        new NativeProgramEntry<TourianEntranceStatueSegment>(TourianEntranceStatueSegment.Phantoon, 0xd7af),
        new NativeProgramEntry<TourianEntranceStatueSegment>(TourianEntranceStatueSegment.BaseDecoration, 0xd7b9),
    ];

}
