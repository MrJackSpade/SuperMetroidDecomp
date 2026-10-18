using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a station notice delays dispatch of a door transition discovered in the same gameplay update.</summary>
internal static class DoorWaitsForMessageBoxVerification
{
    /// <summary>
    /// A door hit during movement changes game_state at once, but a station notice opened
    /// later in the same gameplay call ($85:8080) suspends that call until the box closes;
    /// only the next dispatch runs state $09. In the 13% movie a missile station's refill
    /// finishes on the frame Samus enters a door.
    /// </summary>
    internal static void VerifyDoorWaitsForMessageBox()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        runtime.Player.IntroducedSamus.InputLocked = false;
        CartridgeDoorHeader door = RetailDoorHeaderCatalog.EnumeratePointers()
            .Select(pointer => CartridgeDoorHeaderImporter.Load(bus, pointer))
            .First(candidate => (candidate.DestinationRoomPointer & 0x8000) != 0);
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false);
        game.Session.runtime = runtime;
        game.Session.lastAudioRoomStatePointer = runtime.Room.LoadedRoom.State.Pointer;
        game.GameState = SuperMetroidGameState.MainGameplay;

        // The gameplay call's movement published the door; its PLM handler opened the notice.
        (runtime.Room.LoadedLevelData).PendingDoorTransition = door;
        runtime.MessageBox.Begin(GameplayMessageId.MissileRechargeCompleted, 0);
        game.Step(0);
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the open box holds the door's state");

        int frames = 0;
        while (runtime.MessageBox.IsActive)
        {
            AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the box keeps every frame until it closes");
            // Any held button acknowledges the station notice once its ten-frame wait ends.
            game.Step(SnesButton.X);
            if (++frames > 200)
                throw new InvalidOperationException("The station notice did not close.");
        }
        AssertEqual(SuperMetroidGameState.HitDoorBlock, game.GameState, "the box's final frame publishes the door");
        Console.WriteLine("  Door waits for message box: state $09 follows the station notice.");
    }

    /// <summary>Runs the focused 13% movie door and missile-station notice ordering check.</summary>
    [VerificationCommand("--door-waits-for-message-box")]
    internal static int DoorWaitsForMessageBoxCommand()
    {
        Suite(nameof(VerifyDoorWaitsForMessageBox), () => VerifyDoorWaitsForMessageBox());
        return 0;
    }
}
