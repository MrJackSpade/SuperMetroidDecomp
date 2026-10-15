using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>
/// The native bank-$86 lists the save-station electricity ROM oracle (#1296) walks: each program's entry point. Only verification reads
/// them; the port runs typed programs.
/// </summary>
internal static class SaveStationElectricityNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (SaveStationElectricityProgram Program, ushort Entry)[] Entries =
    [
        (SaveStationElectricityProgram.Start, 0xe683),
        (SaveStationElectricityProgram.Cycle, 0xe687),
    ];

    /// <summary>The bank-$86 bytes the lists occupy, which production must never read.</summary>
    internal static readonly (ushort First, ushort End) Extent = (0xe683, 0xe6ad);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(SaveStationElectricityProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
