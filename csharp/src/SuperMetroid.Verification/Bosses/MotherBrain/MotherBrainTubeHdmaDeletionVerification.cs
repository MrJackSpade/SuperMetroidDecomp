using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks that Mother Brain's main-tube landing removes acid HDMA objects and their RNG side effect.</summary>
internal static class MotherBrainTubeHdmaDeletionVerification
{
    /// <summary>Confirms $A9:8C0C/$8C0F clear acid-scroll channels zero through three so $88:B44A stops byte-swapping the RNG after landing.</summary>
    internal static void VerifyMotherBrainTubeHdmaDeletion()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MotherBrain);
        var fx = runtime.RoomLayer3Fx;
        AssertEqual(RoomFxType.Acid, fx.Type, "Mother Brain's room has acid");

        // The first HDMA pass installs the pre-instruction; later passes swap the RNG bytes.
        fx.AdvanceHdmaSharedState(runtime.System, timeIsFrozen: false);
        runtime.System.SetRandomNumber(0xb9ad);
        fx.AdvanceHdmaSharedState(runtime.System, timeIsFrozen: false);
        AssertEqual(0xadb9, runtime.System.RandomNumber, "the live acid object swaps the RNG bytes");

        // The real main-tube landing raises the deletion of HDMA objects 0-3.
        runtime.Enemies.MotherBrainBehavior.SpawnMotherBrainFallingTube(MotherBrainTubePiece.Main);
        RoomEnemySlot tube = runtime.Enemies.SlotTable.Slots.First(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainTubes);
        MotherBrainFallingTubeState tubeState = tube.SpeciesState<MotherBrainFallingTubeState>()
            ?? throw new InvalidOperationException("The spawned main tube has no state.");
        tubeState.Function = MotherBrainTubeFunction.MainTube_Falling;
        tubeState.FallSpeed = default;
        tube.Position = tube.Position.WithWholeY(0x00fc); // the head follows at Y-$38 = $C4, the landing height
        runtime.Enemies.MotherBrainBehavior.RunMotherBrainFallingTubeMain(tube);
        AssertTrue(runtime.Enemies.LastEnemyFrame.DeleteLiquidHdmaObjects, "the main tube's landing deletes HDMA objects 0-3");

        fx.DeleteLiquidHdmaObjects();
        runtime.System.SetRandomNumber(0xb9ad);
        fx.AdvanceHdmaSharedState(runtime.System, timeIsFrozen: false);
        AssertEqual(0xb9ad, runtime.System.RandomNumber, "the deleted acid object no longer touches the RNG");
        Console.WriteLine("Mother Brain tube HDMA deletion: the landing deletes the acid objects and their RNG swap stops.");
    }

    /// <summary>Runs the focused tube-landing HDMA and RNG check.</summary>
    [VerificationCommand("--mother-brain-tube-hdma-deletion")]
    internal static int MotherBrainTubeHdmaDeletionCommand()
    {
        VerifyMotherBrainTubeHdmaDeletion();
        return 0;
    }
}
