using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Fireflea;
/// <summary>Checks compiled Fireflea movement radii and production initialization without ROM table reads.</summary>
internal static class FirefleaMovementDefinitionsVerification
{
    /// <summary>Compares all eight native radii and rejects selectors outside the authored table.</summary>
    internal static void VerifyFirefleaMovementDefinitions(CartridgeImportAddressSpace rom)
    {
        const ushort radiusTable = 0x8d1d;

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot> initialize = FirefleaBehavior.InitializeFireflea;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Fireflea, slot.Definition);

        for (byte radiusIndex = 0; radiusIndex < 8; radiusIndex++)
        {
            ushort address = checked((ushort)(radiusTable + radiusIndex * 2));
            ushort native = LittleEndianBytes.Word(FirefleaMovementDefinitionsRomByte(rom, 0xa30000 | address), FirefleaMovementDefinitionsRomByte(rom, 0xa30000 | (address + 1)));
            slot.SetSpawnParameter1(EnemyDefinitionId.Fireflea, 0x0002);
            slot.SetSpawnParameter2(EnemyDefinitionId.Fireflea, (ushort)(radiusIndex << 8));
            AssertEqual(native, slot.SpawnSettings<FirefleaSpawnSettings>().Radius,
                $"imported Fireflea radius {radiusIndex}");
            slot.Position = slot.Position.WithWhole(new(0x100, 0x100));
            initialize(slot);
            AssertEqual(native, (enemies.FirefleaStates[0] ?? throw new InvalidOperationException("Verification requires enemies.FirefleaStates[0].")).Radius,
                $"Fireflea initializer radius {radiusIndex}");
        }

        AssertThrows<InvalidDataException>(() => slot.SetSpawnParameter2(EnemyDefinitionId.Fireflea, 8 << 8),
            "Fireflea import rejects the radius past its table");
        AssertThrows<InvalidDataException>(() => slot.SetSpawnParameter2(EnemyDefinitionId.Fireflea, byte.MaxValue << 8),
            "Fireflea import rejects a radius selector that would read adjacent code");

        // The circle turns by the speed's whole pixels' low byte as steps and its fraction's high byte as 256ths of a
        // step, wrapping around the turn, for speeds either way and fractions on both sides of every byte boundary.
        foreach (int whole in new[] { short.MinValue, -257, -256, -255, -129, -128, -2, -1, 0, 1, 2, 127, 128, 255, 256, short.MaxValue })
        foreach (int fraction in new[] { 0, 0x00ff, 0x0100, 0x7fff, 0x8000, 0xff00, 0xffff })
        {
            var speed = SignedSixteenSixteen.FromParts(whole, fraction);
            ushort expected = unchecked((ushort)((whole << 8) | (fraction >> 8)));
            AssertEqual(expected, FirefleaBehavior.TurnPerUpdate(speed).RawValue, $"Fireflea turn at speed {whole}+{fraction}/65536");
        }

        Console.WriteLine(
            "Fireflea movement radii: all eight native words and production initializers pass with table reads forbidden; " +
            "the circle's turn per update takes the speed's middle bytes around the turn.");
    }

    /// <summary>Reads one pinned bank-$A3 radius-table byte for the verification oracle.</summary>
    private static byte FirefleaMovementDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
