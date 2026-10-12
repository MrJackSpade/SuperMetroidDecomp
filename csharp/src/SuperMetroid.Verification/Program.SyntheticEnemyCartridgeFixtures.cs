using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static AssertionsVerification;
using static VerificationHelpersVerification;

/// <summary>Represents synthetic enemy cartridge fixtures verification.</summary>
internal static class SyntheticEnemyCartridgeFixturesVerification
{
    /// <summary>
    /// Seeds a terminated bank-$A1 room enemy population at <paramref name="population"/>: each
    /// record as its native sixteen little-endian bytes, then the <c>$FFFF</c> terminator and
    /// the room's death-quota byte.
    /// </summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for a terminated bank-$A1 room enemy population")]
    internal static void SeedRoomEnemyPopulation(
        TestAddressSpace bus, ushort population, byte deathQuota, params ReadOnlySpan<RoomEnemyPopulationRecord> records)
    {
        int address = 0xa10000 | population;
        foreach (RoomEnemyPopulationRecord record in records)
        {
            WriteTestWords(bus, address,
                (ushort)record.DefinitionPointer, record.Position.X, record.Position.Y,
                record.InitializationParameter, record.Properties, record.ExtraProperties,
                record.Parameter1, record.Parameter2);
            address += 16;
        }
        WriteTestWord(bus, address, 0xffff);
        WriteTestByte(bus, address + 2, deathQuota);
    }

    /// <summary>
    /// Seeds a terminated bank-$B4 enemy graphics set at <paramref name="tileset"/>: each
    /// record's definition pointer and packed VRAM/palette word, then the <c>$FFFF</c> terminator.
    /// </summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for a terminated bank-$B4 enemy graphics set")]
    internal static void SeedRoomEnemyGraphicsSet(
        TestAddressSpace bus, ushort tileset, params ReadOnlySpan<RoomEnemyGraphicsSetHeader> records)
    {
        int address = 0xb40000 | tileset;
        foreach (RoomEnemyGraphicsSetHeader record in records)
        {
            WriteTestWords(bus, address, (ushort)record.DefinitionPointer, record.VramDestination);
            address += 4;
        }
        WriteTestWord(bus, address, 0xffff);
    }

    /// <summary>Overwrites one little-endian word field of the synthetic bank-$A0 enemy definition.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for one bank-$A0 enemy definition word field")]
    internal static void SeedEnemyDefinitionWord(TestAddressSpace bus, EnemyDefinitionId definition, int fieldOffset, ushort value) =>
        WriteTestWord(bus, 0xa00000 | ((ushort)definition + fieldOffset), value);

    /// <summary>Overwrites one little-endian word field of the synthetic bank-$86 enemy-projectile definition.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for one bank-$86 enemy-projectile definition word field")]
    internal static void SeedEnemyProjectileDefinitionWord(
        TestAddressSpace bus, RoomEnemyProjectileKind kind, int fieldOffset, ushort value) =>
        WriteTestWord(bus, 0x860000 | unchecked((ushort)((ushort)kind + fieldOffset)), value);

    /// <summary>Overwrites one byte field of the synthetic bank-$A0 enemy definition.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for one bank-$A0 enemy definition byte field")]
    internal static void SeedEnemyDefinitionByte(TestAddressSpace bus, EnemyDefinitionId definition, int fieldOffset, byte value) =>
        WriteTestByte(bus, 0xa00000 | ((ushort)definition + fieldOffset), value);
}
