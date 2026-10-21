using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Reproduces the staggered falling tubes in Mother Brain's collapse sequence.</summary>
internal static class MotherBrainTubeTimingVerification
{
    /// <summary>
    /// Confirms each later falling-tube spawn ($A9:89E7, $8A0F, $8A9B, $8AC3) waits
    /// for its predecessor's $20 timer. The 100% movie exposed a second tube that
    /// fell 32 frames early while the first still occupied an enemy slot.
    /// </summary>
    internal static void VerifyMotherBrainTubeTiming()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        const EnemyDefinitionId fallingTube = EnemyDefinitionId.MotherBrainTubes;
        int Tubes() => runtime.Enemies.SlotTable.Slots.Count(slot => slot.DefinitionId == fallingTube);

        state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.WaitForFourFreeProjectileSlots;
        int firstSpawn = -1, secondSpawn = -1;
        for (int frame = 0; frame < 200 && secondSpawn < 0; frame++)
        {
            int before = Tubes();
            runtime.Enemies.MotherBrainBehavior.FakeDeath.RunMotherBrainTubeCollapse(state);
            if (Tubes() > before)
            {
                if (firstSpawn < 0) firstSpawn = frame;
                else secondSpawn = frame;
            }
        }
        AssertEqual(0, firstSpawn, "the bottom-left tube spawns once four projectile slots are free");
        // Clear, wait $20, top-right projectile, clear, wait $20, top-left projectile, clear,
        // wait $20: 102 frames, as between the movie's first two tubes (391,197 -> 391,299).
        AssertEqual(102, secondSpawn - firstSpawn, "the bottom-right tube waits out its timer");
        Console.WriteLine("Mother Brain tube timing: each falling-tube enemy waits out the preceding $20 timer.");
    }

    /// <summary>Runs the focused falling-tube spacing regression.</summary>
    [VerificationCommand("--mother-brain-tube-timing")]
    internal static int MotherBrainTubeTimingCommand()
    {
        VerifyMotherBrainTubeTiming();
        return 0;
    }
}
