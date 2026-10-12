using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using static RetailRuntimeFixtureVerification;

/// <summary>Represents mother brain tube timing verification.</summary>
internal static class MotherBrainTubeTimingVerification
{
    // #1269: every falling-tube spawn after the first ($A9:89E7, $8A0F, $8A9B, $8AC3) starts
    // with DEC timer : BPL return, so it waits out the $20 its clear step set. The port spawned
    // three of them at once: in the 100% movie the second tube fell 32 frames early, while the
    // first still occupied its enemy slot.
    /// <summary>verify mother brain tube timing.</summary>
    internal static void VerifyMotherBrainTubeTiming()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        const EnemyDefinitionId fallingTube = EnemyDefinitionId.MotherBrainTubes;
        int Tubes() => runtime.Enemies.SlotTable.Slots.Count(slot => slot.EnemyDefinitionPointer == fallingTube);

        state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.WaitForFourFreeProjectileSlots;
        int firstSpawn = -1, secondSpawn = -1;
        for (int frame = 0; frame < 200 && secondSpawn < 0; frame++)
        {
            int before = Tubes();
            runtime.Enemies.MotherBrainBehavior.RunMotherBrainTubeCollapse(state);
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

    /// <summary>mother brain tube timing command.</summary>
    [VerificationCommand("--mother-brain-tube-timing")]
    internal static int MotherBrainTubeTimingCommand()
    {
        VerifyMotherBrainTubeTiming();
        return 0;
    }
}
