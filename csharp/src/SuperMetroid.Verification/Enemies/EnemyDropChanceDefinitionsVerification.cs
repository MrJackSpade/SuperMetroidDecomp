using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.EnemyDropsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the imported enemy drop table against the cartridge and the ROM-free selection path.</summary>
internal static class EnemyDropChanceDefinitionsVerification
{
    /// <summary>Compares every native six-byte record and verifies that production selection rejects invalid restored pointers.</summary>
    internal static void VerifyEnemyDropChanceDefinitions(SuperMetroidAddressSpace rom)
    {
        Span<byte> actual = stackalloc byte[EnemyDropChanceDefinitions.RecordSize];
        for (int record = 0; record < EnemyDropChanceDefinitions.RecordCount; record++)
        {
            var table = new EnemyDropTable(record);
            ushort pointer = EnemyDropChanceDefinitionsConstants.Pointer(table);
            EnemyDropChanceDefinitions.Copy(table, actual);
            for (int field = 0; field < actual.Length; field++)
            {
                int address = EnemyDropChanceDefinitions.NativeBank |
                    unchecked((ushort)(pointer + field));
                AssertEqual(rom.ReadCartridgeByte(address), actual[field],
                    $"enemy drop record ${pointer:X4} byte {field}");
            }
        }

        AssertEqual(EnemyDropChanceDefinitions.LastPointer,
            EnemyDropChanceDefinitionsConstants.Pointer(new EnemyDropTable(EnemyDropChanceDefinitions.RecordCount - 1)),
            "the last compiled drop table is the last native record");
        AssertThrows<InvalidDataException>(
            () => EnemyDropChanceDefinitions.Copy(new EnemyDropTable(-1), new byte[6]),
            "enemy drop table below the catalog is rejected");
        AssertThrows<InvalidDataException>(
            () => EnemyDropChanceDefinitions.Copy(new EnemyDropTable(EnemyDropChanceDefinitions.RecordCount), new byte[6]),
            "enemy drop table above the catalog is rejected");
        AssertThrows<ArgumentException>(
            () => EnemyDropChanceDefinitions.Copy(new EnemyDropTable(0), new byte[5]),
            "short enemy drop destination");

        var samus = new SamusState
        {
            Health = 99,
            MaxHealth = 99,
        };
        // The fixture's population is empty; drop selection reads the compiled enemy definitions.
        EnemyDropFixture fixture = CreateEnemyDropFixture(samus, randomValues: [1], CompiledRoomEnemyDefinitions.Instance);
        var projectile = new RoomEnemyProjectileSlot(1);

        for (int record = 0; record < EnemyDropChanceDefinitions.RecordCount; record++)
        {
            var table = new EnemyDropTable(record);
            projectile.DropChancesOverride = EnemyDropChanceDefinitions.At(table);
            EnemyPickupKind selected = fixture.System.DeathsAndDrops.SelectRandomEnemyDrop(projectile);
            AssertTrue(Enum.IsDefined(selected),
                $"production enemy drop table {record} returns a defined kind");
        }

        AssertThrows<InvalidDataException>(
            () => EnemyDropChanceDefinitions.At(new EnemyDropTable(EnemyDropChanceDefinitions.RecordCount)),
            "production enemy drop rejects a table outside the catalog");
        Console.WriteLine(
            "Enemy drop chance definitions: all 118 native records match ROM and execute " +
            "through production selection with RAM-only memory; tables outside the catalog reject without a ROM fallback.");
    }

    /// <summary>Runs the drop table comparison using the retail ROM as the source oracle.</summary>
    [VerificationCommand("--enemy-drop-chance-definitions")]
    internal static int EnemyDropChanceDefinitionsCommand()
    {
        Suite(nameof(VerifyEnemyDropChanceDefinitions), () => VerifyEnemyDropChanceDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
