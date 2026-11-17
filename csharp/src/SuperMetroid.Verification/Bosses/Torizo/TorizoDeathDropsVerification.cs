using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// Both Torizos' death drops roll named chances (#2244): the table the cartridge's death drops name through Bomb Torizo's
/// header, which Golden Torizo's death rolls as well.
/// </summary>
internal static class TorizoDeathDropsVerification
{
    /// <summary>Decodes the drop table Bomb Torizo's header names and compares it with the death drops' chances.</summary>
    internal static void VerifyTorizoDeathDropChances(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        // The header's drop-chance word lies 58 bytes in.
        int header = RoomEnemyRomLayout.DefinitionBank | EnemyDefinitionIds.HeaderPointer(EnemyDefinitionId.BombTorizo);
        AssertEqual(EnemyHeaderTables.DropChances(cartridge, cartridge.ReadCartridgeWord(header + 58)),
            TorizoDeathDefinitions.DeathDropChances, "the Torizos' death drop chances are Bomb Torizo's header's");
        // Each orb's drop rolls the table its own header names (#2332).
        foreach ((EnemyDefinitionId orb, EnemyDropChances chances) in new[]
                 {
                     (EnemyDefinitionId.BombTorizoOrb, TorizoChozoOrbDefinitions.BombTorizoOrbDropChances),
                     (EnemyDefinitionId.GoldenTorizoOrb, TorizoChozoOrbDefinitions.GoldenTorizoOrbDropChances),
                 })
        {
            int orbHeader = RoomEnemyRomLayout.DefinitionBank | EnemyDefinitionIds.HeaderPointer(orb);
            AssertEqual(EnemyHeaderTables.DropChances(cartridge, cartridge.ReadCartridgeWord(orbHeader + 58)), chances,
                $"{orb}'s drop chances are its header's");
        }
        Console.WriteLine("  Torizo death drops: both deaths roll the chances Bomb Torizo's header names, and each orb its own header's.");
    }

    /// <summary>Runs the Torizo death drop chance check against the repository cartridge.</summary>
    [VerificationCommand("--torizo-death-drops")]
    internal static int TorizoDeathDropsCommand()
    {
        Suite(nameof(VerifyTorizoDeathDropChances), () => VerifyTorizoDeathDropChances(LoadRepositoryRom()));
        return 0;
    }
}
