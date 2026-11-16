using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Torizo;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses;

/// <summary>
/// #2242: Golden Torizo's and Ceres Ridley's typed health bands select the installed colour rows the bare stage numbers
/// selected before they were named.
/// </summary>
internal static class BossHealthBandsVerification
{
    /// <summary>
    /// At every health he can have, Golden Torizo shows the row of his installed health colours his health divided by 2048
    /// selected, clamped to the last; his health never reaches the eighth row, so seven bands hold every row he shows.
    /// </summary>
    internal static void VerifyGoldenTorizoHealthBands()
    {
        const int startingHealth = 13500;
        AssertEqual(startingHealth, SuperMetroid.Core.World.CompiledRoomEnemyDefinitions.Instance.ReadEnemyDefinition(SuperMetroid.Core.Enemies.EnemyDefinitionId.GoldenTorizo).Health,
            "Golden Torizo starts at 13500 health");
        for (int health = 0; health <= startingHealth; health++)
        {
            int row = 0;
            GoldenTorizoHealthBand band = GoldenTorizoHealthBand.Of(health);
            while (GoldenTorizoHealthBandDefinitions.LowestFirst[row] != band)
                row++;
            AssertEqual(Math.Min(health / 2048, 7), row, $"Golden Torizo at {health} health shows health row {row}");
        }
        AssertEqual(GoldenTorizoHealthBandDefinitions.Unhurt, GoldenTorizoHealthBand.Of(startingHealth), "Golden Torizo starts unhurt");
        AssertEqual("torizo-golden-health-gravely-wounded", SuperMetroid.Core.Bosses.Torizo.Artwork.TorizoShades.Body(TorizoLook.GoldenHealth(GoldenTorizoHealthBandDefinitions.GravelyWounded)).Effect,
            "a band's shade is named for the band");
        Console.WriteLine("  Golden Torizo health bands: every health up to his 13500 shows the row health / 2048 selected.");
    }

    /// <summary>
    /// Ceres Ridley keeps his own colours before his fiftieth accepted hit, shows the wounded colouring from it and the
    /// nearly-dead colouring from his seventieth on, never the badly wounded one.
    /// </summary>
    internal static void VerifyCeresRidleyHealthBands()
    {
        for (int hits = 0; hits <= 100; hits++)
        {
            RidleyHealthShade? expected = hits < 50 ? null : hits < 70 ? RidleyHealthShadeDefinitions.Wounded : RidleyHealthShadeDefinitions.NearlyDead;
            AssertEqual(expected, CeresRidleyHealthBand.Of(hits).HealthShade, $"Ceres Ridley after {hits} accepted hits");
        }
        Console.WriteLine("  Ceres Ridley health bands: own colours below 50 hits, wounded from 50, nearly dead from 70.");
    }

    /// <summary>Runs the boss health band checks.</summary>
    [VerificationCommand("--boss-health-bands")]
    internal static int BossHealthBandsCommand()
    {
        Suite(nameof(VerifyGoldenTorizoHealthBands), () => VerifyGoldenTorizoHealthBands());
        Suite(nameof(VerifyCeresRidleyHealthBands), () => VerifyCeresRidleyHealthBands());
        return 0;
    }
}
