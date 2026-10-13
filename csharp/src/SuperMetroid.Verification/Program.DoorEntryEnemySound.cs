using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailGameFixtureVerification;
using static RetailRuntimeFixtureVerification;

internal static class DoorEntryEnemySoundVerification
{
    // #1269: state $0A ($82:E1B7) runs the enemy routine before Clear_Sounds_When_Going_
    // Through_Door, $71 and DisableSounds, so an enemy instruction's sound on the entry frame
    // is admitted ahead of the door's cancel. In the 100% movie a Ninja Pirate's $3F queued
    // there lengthens the Metal Pirates exit's sound-queue wait.
    internal static void VerifyDoorEntryEnemySound()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0x948c);
        var samus = runtime.Player.Samus!;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(64, 128));
        samus.InputLocked = true;
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        var body = runtime.Enemies.SlotTable.Slots.First(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KihunterGreen);
        var state = runtime.Enemies.KiHunterStates[body.NativeIndex / 64]!;
        body.Position = body.Position.WithWhole(new(128, 128));
        state.Function = KiHunterEnemyFunction.NoOp;
        // The real spit opcode (library two $4C) executes during the entry frame's enemy pass.
        body.CurrentInstruction = (ushort)(KiHunterInstructionProgramDefinitions.SpitRight + 0x10);
        body.InstructionTimer = 1;
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        game.runtime = runtime;
        game.lastAudioRoomStatePointer = runtime.Room.ActiveRoom!.State.Pointer;
        var level = runtime.Room.LevelData!;
        bool selected = false;
        for (int y = 0; y < level.Size.Y && !selected; y++)
        for (int x = 0; x < level.Size.X && !selected; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            level.ResolveDoorCollision(bus, block.Behavior, samus.Pose, true);
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

    [VerificationCommand("--door-entry-enemy-sound")]
    internal static int DoorEntryEnemySoundCommand()
    {
        VerifyDoorEntryEnemySound();
        return 0;
    }
}
