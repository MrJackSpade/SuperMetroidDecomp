using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: when Mother Brain's main tube lands, $A9:8C0C/$8C0F zero the channel words of
    // HDMA objects 0-3, deleting the room's acid scroll objects. Their pre-instruction
    // ($88:B44A) byte-swaps the RNG every pass; the port kept running it, so in the 100%
    // movie its seed was the swapped $ADB9 stepped once instead of $B9AD.
    private static void VerifyMotherBrainTubeHdmaDeletion()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        var fx = runtime.RoomLayer3Fx;
        AssertEqual(RoomFxType.Acid, fx.Type, "Mother Brain's room has acid");

        // The first HDMA pass installs the pre-instruction; later passes swap the RNG bytes.
        fx.AdvanceHdmaSharedState(runtime.System, timeIsFrozen: false);
        runtime.System.SetRandomNumber(0xb9ad);
        fx.AdvanceHdmaSharedState(runtime.System, timeIsFrozen: false);
        AssertEqual((ushort)0xadb9, runtime.System.RandomNumber, "the live acid object swaps the RNG bytes");

        // The real main-tube landing raises the deletion of HDMA objects 0-3.
        typeof(RoomEnemySystem).GetMethod("SpawnMotherBrainFallingTube", flags)!
            .Invoke(runtime.Enemies, [MotherBrainFallingTubePopulationDefinitions.Main]);
        RoomEnemySlot tube = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer == 0xecff);
        tube.VariableA = MotherBrainInstructionCodes.Function_MotherBrainTubes_MainTube_Falling;
        tube.VariableC = 0;
        tube.YPosition = 0x00fc; // the head follows at Y-$38 = $C4, the landing height
        typeof(RoomEnemySystem).GetMethod("RunMotherBrainFallingTubeMain", flags)!.Invoke(runtime.Enemies, [tube]);
        AssertTrue(runtime.Enemies.MotherBrainDeletedHdmaObjects, "the main tube's landing deletes HDMA objects 0-3");

        fx.DeleteLiquidHdmaObjects();
        runtime.System.SetRandomNumber(0xb9ad);
        fx.AdvanceHdmaSharedState(runtime.System, timeIsFrozen: false);
        AssertEqual((ushort)0xb9ad, runtime.System.RandomNumber, "the deleted acid object no longer touches the RNG");
        Console.WriteLine("Mother Brain tube HDMA deletion: the landing deletes the acid objects and their RNG swap stops.");
    }
}
