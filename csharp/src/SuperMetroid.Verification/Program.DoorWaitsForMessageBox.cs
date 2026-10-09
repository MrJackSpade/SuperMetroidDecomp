using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// A door hit during movement changes game_state at once, but a station notice opened
    /// later in the same gameplay call ($85:8080) suspends that call until the box closes;
    /// only the next dispatch runs state $09. In the 13% movie a missile station's refill
    /// finishes on the frame Samus enters a door.
    /// </summary>
    private static void VerifyDoorWaitsForMessageBox()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        runtime.Samus!.InputLocked = false;
        CartridgeDoorHeader door = RetailDoorHeaderCatalog.EnumeratePointers()
            .Select(pointer => CartridgeDoorHeaderImporter.Load(bus, pointer))
            .First(candidate => (candidate.DestinationRoomPointer & 0x8000) != 0);
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        typeof(SuperMetroidGame).GetField("runtime", flags)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", flags)!.SetValue(game, runtime.ActiveRoom!.State.Pointer);
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);

        // The gameplay call's movement published the door; its PLM handler opened the notice.
        PrivateState.SetProperty(runtime.LevelData!, nameof(RoomLevelData.PendingDoorTransition), door);
        runtime.MessageBox.Begin(bus, GameplayMessageId.MissileRechargeCompleted, 0);
        game.Step(0);
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the open box holds the door's state");

        int frames = 0;
        while (runtime.MessageBox.IsActive)
        {
            AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the box keeps every frame until it closes");
            // Any held button acknowledges the station notice once its ten-frame wait ends.
            game.Step((ushort)SuperMetroid.Core.Input.SnesButton.X);
            if (++frames > 200)
                throw new InvalidOperationException("The station notice did not close.");
        }
        AssertEqual(SuperMetroidGameState.HitDoorBlock, game.GameState, "the box's final frame publishes the door");
        Console.WriteLine("  Door waits for message box: state $09 follows the station notice.");
    }
}
