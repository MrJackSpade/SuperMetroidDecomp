using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Botwoon;

/// <summary>Checks Botwoon's typed speed stages' movement, spit and body-spacing values against the native stage tables.</summary>
internal static class BotwoonSpeedsVerification
{
    /// <summary>The stages in the order the cartridge's tables hold them.</summary>
    private static readonly BotwoonSpeedStage[] CartridgeOrder =
        [BotwoonSpeedStageDefinitions.Full, BotwoonSpeedStageDefinitions.BelowHalf, BotwoonSpeedStageDefinitions.BelowQuarter];

    /// <summary>Runs separate speed and segment-spacing checks for the three speed stages.</summary>
    internal static void VerifyCompiledBotwoonSpeeds(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyBotwoonSpeedAlgorithm), () => VerifyBotwoonSpeedAlgorithm(rom));
        Suite(nameof(VerifyBotwoonSpacingAlgorithm), () => VerifyBotwoonSpacingAlgorithm(rom));
    }

    /// <summary>Checks native speeds and production speed-stage transitions, including the stage held inside a hole.</summary>
    private static void VerifyBotwoonSpeedAlgorithm(CartridgeImportAddressSpace rom)
    {
        for (int row = 0; row < CartridgeOrder.Length; row++)
        {
            BotwoonSpeedStage stage = CartridgeOrder[row];
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(BotwoonSpeedStageDefinitions.MovementReferenceAddress + row * 4), stage.MovementSpeed, $"Botwoon {stage} native movement speed");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xb39e77 + row * 2), stage.SpitSpeed, $"Botwoon {stage} native spit speed");
        }
        Action<RoomEnemyPlace, BotwoonEnemyState> update = BotwoonBehavior.UpdateBotwoonSpeedStage;
        var head = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom()).Places.InUpdateOrder[0];
        var state = new BotwoonEnemyState(body: []) { HalfHealth = 1500, QuarterHealth = 750 };
        for (int health = 0; health <= 3000; health++)
        {
            head.Health = (ushort)health;
            int row = health is 0 or >= 1500 ? 0 : health >= 750 ? 1 : 2;
            state.InsideHole = false;
            update(head, state);
            AssertEqual(CartridgeOrder[row], state.SpeedStage, $"Botwoon at health {health} takes its speed stage");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(BotwoonSpeedStageDefinitions.MovementReferenceAddress + row * 4), state.SpeedStage.MovementSpeed, "Botwoon real stage speed");
            state.InsideHole = true;
            head.Health = (ushort)(3000 - health);
            update(head, state);
            AssertEqual(CartridgeOrder[row], state.SpeedStage, "Botwoon keeps his stage inside a hole");
        }
        Console.WriteLine("Botwoon speed algorithm: both three-word native views and 3,001 real health transitions match; stages inside a hole are held.");
    }

    /// <summary>Checks native body-history spacing for each speed stage.</summary>
    private static void VerifyBotwoonSpacingAlgorithm(CartridgeImportAddressSpace rom)
    {
        for (int row = 0; row < CartridgeOrder.Length; row++)
        {
            int address = BotwoonSpeedStageDefinitions.MovementReferenceAddress + row * 4 + 2;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            // The cartridge spaces segments by a byte distance into its four-byte history entries.
            AssertEqual(expected, BotwoonSpeedStageDefinitions.HistoryEntryBytes * CartridgeOrder[row].SegmentSpacingEntries,
                $"Botwoon native body spacing for {CartridgeOrder[row]}");
        }
        Console.WriteLine("Botwoon spacing algorithm: all three native body-history spacings match.");
    }
}
