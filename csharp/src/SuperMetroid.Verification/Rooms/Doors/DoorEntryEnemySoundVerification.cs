using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend;

/// <summary>Reproduces enemy sound ordering on the first door-entry update.</summary>
internal static class DoorEntryEnemySoundVerification
{
    /// <summary>
    /// Confirms state $0A ($82:E1B7) runs enemy instructions before the door's $71
    /// cancel and DisableSounds. The 100% movie exposed an entry-frame enemy sound
    /// that lengthened the Metal Pirates exit's sound-queue wait.
    /// </summary>
    internal static void VerifyDoorEntryEnemySound()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var random = new CartridgeRandom();
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true, random: random);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.CrateriaKihunter);
        var samus = runtime.Player.IntroducedSamus;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(64, 128));
        samus.Control.LockHandlers();
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation();
        var body = runtime.Enemies.SlotTable.Slots.First(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KihunterGreen);
        var state = runtime.Enemies.KiHunterStates[body.NativeIndex / 64];
        body.Position = body.Position.WithWhole(new(128, 128));
        (state ?? throw new InvalidOperationException("Verification requires state.")).Function = KiHunterEnemyFunction.NoOp;
        // The real spit opcode (library two $4C) executes during the entry frame's enemy pass.
        // The spit cue, four poses into the right spit animation.
        state.Program = new AnimationProgramCursor<KiHunterProgram>(KiHunterProgram.SpitRight, 4);
        body.InstructionTimer = NativeCountdown.Start(1);
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false, random: random);
        game.AdoptRuntime(runtime);
        game.Session.LastAudioRoomState = runtime.Room.LoadedRoom.State;
        var level = runtime.Room.LoadedLevelData;
        bool selected = false;
        for (int y = 0; y < level.Size.Y && !selected; y++)
        for (int x = 0; x < level.Size.X && !selected; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            level.ResolveDoorCollision(block.Bts.Value, samus.Pose, true);
            selected = true;
        }
        AssertTrue(selected, "the Ki Hunter room has an exit");
        var audio = game.AudioForVerification;
        byte start = audio.SoundQueueForVerification(1).Next;
        game.GameState = SuperMetroidGameState.HitDoorBlock;
        game.Step(0);

        AssertEqual((byte)0x4c, audio.SoundQueueEntryForVerification(1, start),
            "the entry frame's enemy pass queues the spit first");
        AssertEqual((byte)0x71, audio.SoundQueueEntryForVerification(1, (start + 1) & 0x0f),
            "the door's library-two cancel follows the enemy sound");
        Console.WriteLine("Door entry enemy sound: the entry frame's enemy sounds precede the door's cancel and DisableSounds.");
    }

    /// <summary>Runs the focused enemy-before-door sound ordering check.</summary>
    [VerificationCommand("--door-entry-enemy-sound")]
    internal static int DoorEntryEnemySoundCommand()
    {
        VerifyDoorEntryEnemySound();
        return 0;
    }
}
