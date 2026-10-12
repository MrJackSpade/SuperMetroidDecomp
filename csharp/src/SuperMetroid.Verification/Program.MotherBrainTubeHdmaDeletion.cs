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

internal static class MotherBrainTubeHdmaDeletionVerification
{
    // #1269: when Mother Brain's main tube lands, $A9:8C0C/$8C0F zero the channel words of
    // HDMA objects 0-3, deleting the room's acid scroll objects. Their pre-instruction
    // ($88:B44A) byte-swaps the RNG every pass; the port kept running it, so in the 100%
    // movie its seed was the swapped $ADB9 stepped once instead of $B9AD.
    internal static void VerifyMotherBrainTubeHdmaDeletion()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        var fx = runtime.RoomLayer3Fx;
        AssertEqual(RoomFxType.Acid, fx.Type, "Mother Brain's room has acid");

        // The first HDMA pass installs the pre-instruction; later passes swap the RNG bytes.
        fx.AdvanceHdmaSharedState(runtime.System, timeIsFrozen: false);
        runtime.System.SetRandomNumber(0xb9ad);
        fx.AdvanceHdmaSharedState(runtime.System, timeIsFrozen: false);
        AssertEqual((ushort)0xadb9, runtime.System.RandomNumber, "the live acid object swaps the RNG bytes");

        // The real main-tube landing raises the deletion of HDMA objects 0-3.
        runtime.Enemies.MotherBrainBehavior.SpawnMotherBrainFallingTube(MotherBrainFallingTubePopulationDefinitions.Main);
        RoomEnemySlot tube = runtime.Enemies.SlotTable.Slots.First(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainTubes);
        tube.VariableA = (ushort)MotherBrainTubeFunction.MainTube_Falling;
        tube.VariableC = 0;
        tube.Position = tube.Position.WithWholeY(0x00fc); // the head follows at Y-$38 = $C4, the landing height
        runtime.Enemies.MotherBrainBehavior.RunMotherBrainFallingTubeMain(tube);
        AssertTrue(runtime.Enemies.MotherBrainEncounter.MotherBrainDeletedHdmaObjects, "the main tube's landing deletes HDMA objects 0-3");

        fx.DeleteLiquidHdmaObjects();
        runtime.System.SetRandomNumber(0xb9ad);
        fx.AdvanceHdmaSharedState(runtime.System, timeIsFrozen: false);
        AssertEqual((ushort)0xb9ad, runtime.System.RandomNumber, "the deleted acid object no longer touches the RNG");
        Console.WriteLine("Mother Brain tube HDMA deletion: the landing deletes the acid objects and their RNG swap stops.");
    }

    [VerificationCommand("--mother-brain-tube-hdma-deletion")]
    internal static int MotherBrainTubeHdmaDeletionCommand()
    {
        VerifyMotherBrainTubeHdmaDeletion();
        return 0;
    }
}
