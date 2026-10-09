using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// All three sound-queue routines drop requests while a power-bomb explosion's status is
    /// negative ($80:9072/$90F4/$9176). In the 13% movie Samus enters a door during one, so
    /// the door's $32/$71 cancels never reach a ring and $E29E proceeds on its first check;
    /// the port queued them and waited a frame longer.
    /// </summary>
    private static void VerifyDoorSoundsDuringPowerBomb()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        runtime.Samus!.InputLocked = true;
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        typeof(SuperMetroidGame).GetField("runtime", flags)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", flags)!.SetValue(game, runtime.ActiveRoom!.State.Pointer);
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
        game.Step(0);

        var level = runtime.LevelData!;
        bool selected = false;
        for (int y = 0; y < level.HeightInBlocks && !selected; y++)
        for (int x = 0; x < level.WidthInBlocks && !selected; x++)
        {
            var block = level.GetCollisionBlock(x, y);
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            level.ResolveDoorCollision(bus, block.Behavior, runtime.Samus.Pose, true);
            selected = true;
        }
        AssertTrue(selected, "Landing Site has an exit");

        runtime.PowerBombExplosionStatus = 0x8000;
        var audio = game.AudioForVerification;
        byte library1 = audio.SoundQueueForVerification(0).Next;
        byte library2 = audio.SoundQueueForVerification(1).Next;
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.HitDoorBlock);
        game.Step(0);

        AssertEqual(DoorTransitionPhase.WaitForSoundQueues, game.DoorTransitionPhaseForVerification,
            "the entry frame begins the door sound wait");
        AssertEqual(library1, audio.SoundQueueForVerification(0).Next, "library one's cancel is dropped");
        AssertEqual(library2, audio.SoundQueueForVerification(1).Next, "library two's $71 is dropped");
        game.Step(0);
        AssertEqual(DoorTransitionPhase.FadeOutSourcePalette, game.DoorTransitionPhaseForVerification,
            "$E29E finds the rings empty on its first check");
        Console.WriteLine("  Door sounds during a power bomb: the cancels are dropped and the wait ends at once.");
    }
}
