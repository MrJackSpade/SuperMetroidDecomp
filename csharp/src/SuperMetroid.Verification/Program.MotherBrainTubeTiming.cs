using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: every falling-tube spawn after the first ($A9:89E7, $8A0F, $8A9B, $8AC3) starts
    // with DEC timer : BPL return, so it waits out the $20 its clear step set. The port spawned
    // three of them at once: in the 100% movie the second tube fell 32 frames early, while the
    // first still occupied its enemy slot.
    private static void VerifyMotherBrainTubeTiming()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        var collapse = typeof(RoomEnemySystem).GetMethod("RunMotherBrainTubeCollapse", BindingFlags.Instance | BindingFlags.NonPublic)!;
        const EnemyDefinitionId fallingTube = EnemyDefinitionId.MotherBrainTubes;
        int Tubes() => runtime.Enemies.Slots.Count(slot => slot.EnemyDefinitionPointer == fallingTube);

        state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.WaitForFourFreeProjectileSlots;
        int firstSpawn = -1, secondSpawn = -1;
        for (int frame = 0; frame < 200 && secondSpawn < 0; frame++)
        {
            int before = Tubes();
            collapse.Invoke(runtime.Enemies, [state]);
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
}
