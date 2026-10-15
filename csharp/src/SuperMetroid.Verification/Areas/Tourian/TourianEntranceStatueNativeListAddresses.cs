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
    internal static readonly (TourianEntranceStatueProgram Program, ushort Entry)[] Entries =
    [
        (TourianEntranceStatueProgram.Ridley, 0xd7a5),
        (TourianEntranceStatueProgram.Phantoon, 0xd7af),
        (TourianEntranceStatueProgram.BaseDecoration, 0xd7b9),
    ];

}
