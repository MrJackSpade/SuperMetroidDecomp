using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.EnemyDropsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the imported enemy drop chances against the cartridge and the ROM-free selection path.</summary>
internal static class EnemyDropChanceDefinitionsVerification
{
    /// <summary>
    /// Decodes every authored six-byte table, compares each chance with its cartridge byte, runs every table through
    /// production selection, and checks that words naming no table are rejected.
    /// </summary>
    internal static void VerifyEnemyDropChanceDefinitions(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var samus = new SamusState(InstalledFixtures.AnimationContinuation)
        {
            Health = 99,
            MaxHealth = 99,
        };
        // The fixture's population is empty; drop selection reads the chances it is given.
        EnemyDropFixture fixture = CreateEnemyDropFixture(samus, randomValues: [1], CompiledRoomEnemyDefinitions.Instance);
        var projectile = new RoomEnemyProjectileSlot(1);
        for (int table = 0; table < EnemyHeaderTableDefinitions.DropChancesCount; table++)
        {
            int word = EnemyHeaderTableDefinitions.FirstDropChances + table * EnemyHeaderTableDefinitions.DropChancesSize;
            EnemyDropChances chances = EnemyHeaderTables.DropChances(cartridge, word)
                ?? throw new InvalidOperationException($"Drop table ${word:X4} decoded as no drop.");
            int[] bytes = [.. Enumerable.Range(0, EnemyHeaderTableDefinitions.DropChancesSize)
                .Select(column => (int)rom.ReadCartridgeByte(EnemyHeaderTableDefinitions.TableBank | (word + column)))];
            AssertEqual(new EnemyDropChances(bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5]), chances,
                $"enemy drop table ${word:X4} chances in column order");

            projectile.DropChancesOverride = chances;
            EnemyPickupKind selected = fixture.System.DeathsAndDrops.SelectRandomEnemyDrop(projectile);
            AssertTrue(Enum.IsDefined(selected), $"production enemy drop table ${word:X4} returns a defined kind");
        }

        AssertEqual<EnemyDropChances?>(null, EnemyHeaderTables.DropChances(cartridge, 0), "a zero word drops nothing");
        AssertThrows<InvalidDataException>(
            () => EnemyHeaderTables.DropChances(cartridge, EnemyHeaderTableDefinitions.FirstDropChances - EnemyHeaderTableDefinitions.DropChancesSize),
            "a word below the tables is rejected");
        AssertThrows<InvalidDataException>(
            () => EnemyHeaderTables.DropChances(cartridge, EnemyHeaderTableDefinitions.FirstDropChances + 1),
            "an unaligned word is rejected");
        AssertThrows<InvalidDataException>(
            () => EnemyHeaderTables.DropChances(cartridge, EnemyHeaderTableDefinitions.FirstDropChances +
                EnemyHeaderTableDefinitions.DropChancesCount * EnemyHeaderTableDefinitions.DropChancesSize),
            "a word past the tables is rejected");
        Console.WriteLine(
            "Enemy drop chance definitions: all 118 authored tables decode to their cartridge chances and execute " +
            "through production selection; words naming no table are rejected.");
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
