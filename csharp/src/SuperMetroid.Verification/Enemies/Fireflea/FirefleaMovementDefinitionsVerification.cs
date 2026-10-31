using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Enemies.Fireflea;
/// <summary>Checks compiled Fireflea movement radii and production initialization without ROM table reads.</summary>
internal static class FirefleaMovementDefinitionsVerification
{
    /// <summary>Compares all eight native radii and rejects selectors outside the authored table.</summary>
    internal static void VerifyFirefleaMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const ushort radiusTable = 0x8d1d;

        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot> initialize = FirefleaBehavior.InitializeFireflea;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Fireflea, slot.Definition);

        for (byte radiusIndex = 0; radiusIndex < 8; radiusIndex++)
        {
            ushort address = checked((ushort)(radiusTable + radiusIndex * 2));
            ushort native = LittleEndianBytes.Word(FirefleaMovementDefinitionsRomByte(rom, 0xa30000 | address), FirefleaMovementDefinitionsRomByte(rom, 0xa30000 | (address + 1)));
            AssertEqual(native, FirefleaOrbit.Radius(radiusIndex),
                $"compiled Fireflea radius {radiusIndex}");

            slot.SetSpawnParameter1(EnemyDefinitionId.Fireflea, 0x0002);
            slot.SetSpawnParameter2(EnemyDefinitionId.Fireflea, (ushort)(radiusIndex << 8));
            slot.Position = slot.Position.WithWhole(new(0x100, 0x100));
            initialize(slot);
            AssertEqual(native, (enemies.FirefleaStates[0] ?? throw new InvalidOperationException("Verification requires enemies.FirefleaStates[0].")).Radius,
                $"Fireflea initializer radius {radiusIndex}");
        }

        AssertThrows<InvalidDataException>(() => FirefleaOrbit.Radius(8),
            "Fireflea radius beyond authored table");
        AssertThrows<InvalidDataException>(() => FirefleaOrbit.Radius(byte.MaxValue),
            "Fireflea restored radius selector does not read adjacent code");

        Console.WriteLine(
            "Fireflea movement radii: all eight native words and production initializers pass with table reads forbidden.");
    }

    /// <summary>Reads one pinned bank-$A3 radius-table byte for the verification oracle.</summary>
    private static byte FirefleaMovementDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
