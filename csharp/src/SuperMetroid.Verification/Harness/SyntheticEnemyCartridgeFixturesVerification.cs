using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Writes terminated enemy-population and graphics-set records into synthetic cartridge
/// memory so production parsers can be verified against native record layouts.
/// </summary>
internal static class SyntheticEnemyCartridgeFixturesVerification
{
    /// <summary>
    /// Seeds a terminated bank-$A1 room enemy population at <paramref name="population"/>: each
    /// record as its native sixteen little-endian bytes, then the <c>$FFFF</c> terminator and
    /// the room's death-quota byte.
    /// </summary>
    internal static void SeedRoomEnemyPopulation(
        TestAddressSpace bus, ushort population, byte deathQuota, params ReadOnlySpan<RoomEnemyPopulationRecord> records)
    {
        int address = 0xa10000 | population;
        foreach (RoomEnemyPopulationRecord record in records)
        {
            WriteTestWords(bus, address,
                TestAddressSpace.FixtureHeaderPointer(record.DefinitionId), record.Position.X, record.Position.Y,
                checked((ushort)SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Encode(record.SpawnSettings)), record.Properties.ToWord(), record.ExtraProperties.ToWord(),
                checked((ushort)record.Parameter1), checked((ushort)record.Parameter2));
            address += 16;
        }
        WriteTestWord(bus, address, 0xffff);
        WriteTestByte(bus, address + 2, deathQuota);
    }

    /// <summary>
    /// Seeds a terminated bank-$B4 enemy graphics set at <paramref name="tileset"/>: each
    /// record's definition pointer and packed VRAM/palette word, then the <c>$FFFF</c> terminator.
    /// </summary>
    internal static void SeedRoomEnemyGraphicsSet(
        TestAddressSpace bus, ushort tileset, params ReadOnlySpan<RoomEnemyGraphicsSetHeader> records)
    {
        int address = 0xb40000 | tileset;
        foreach (RoomEnemyGraphicsSetHeader record in records)
        {
            WriteTestWords(bus, address, TestAddressSpace.FixtureHeaderPointer(record.DefinitionId), record.NativePlacement);
            address += 4;
        }
        WriteTestWord(bus, address, 0xffff);
    }

    /// <summary>Overwrites one little-endian word field of the synthetic bank-$A0 enemy definition.</summary>
    internal static void SeedEnemyDefinitionWord(TestAddressSpace bus, EnemyDefinitionId definition, int fieldOffset, ushort value) =>
        WriteTestWord(bus, 0xa00000 | (TestAddressSpace.FixtureHeaderPointer(definition) + fieldOffset), value);

    /// <summary>Overwrites one little-endian word field of the synthetic bank-$86 enemy-projectile definition.</summary>
    internal static void SeedEnemyProjectileDefinitionWord(
        TestAddressSpace bus, RoomEnemyProjectileKind kind, int fieldOffset, ushort value) =>
        WriteTestWord(bus, 0x860000 | unchecked((ushort)(EnemyProjectileNativeWords.Kinds.Word(kind) + fieldOffset)), value);

    /// <summary>Overwrites one byte field of the synthetic bank-$A0 enemy definition.</summary>
    internal static void SeedEnemyDefinitionByte(TestAddressSpace bus, EnemyDefinitionId definition, int fieldOffset, byte value) =>
        WriteTestByte(bus, 0xa00000 | (TestAddressSpace.FixtureHeaderPointer(definition) + fieldOffset), value);
}
