using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // #1269: state $0A ($82:E1B7) runs the enemy routine before Clear_Sounds_When_Going_
    // Through_Door, $71 and DisableSounds, so an enemy instruction's sound on the entry frame
    // is admitted ahead of the door's cancel. In the 100% movie a Ninja Pirate's $3F queued
    // there lengthens the Metal Pirates exit's sound-queue wait.
    private static void VerifyDoorEntryEnemySound()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0x948c);
        var samus = runtime.Samus!;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.XPosition = 64; samus.YPosition = 128;
        samus.InputLocked = true;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        var body = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KihunterGreen);
        var state = runtime.Enemies.KiHunterStates[body.NativeIndex / 64]!;
        body.XPosition = 128; body.YPosition = 128;
        state.Function = KiHunterEnemyFunction.NoOp;
        // The real spit opcode (library two $4C) executes during the entry frame's enemy pass.
        body.CurrentInstruction = (ushort)(KiHunterInstructionProgramDefinitions.SpitRight + 0x10);
        body.InstructionTimer = 1;
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(SuperMetroidGame).GetField("runtime", flags)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", flags)!.SetValue(game, runtime.ActiveRoom!.State.Pointer);
        var level = runtime.LevelData!;
        bool selected = false;
        for (int y = 0; y < level.HeightInBlocks && !selected; y++)
        for (int x = 0; x < level.WidthInBlocks && !selected; x++)
        {
            var block = level.GetCollisionBlock(x, y);
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            level.ResolveDoorCollision(bus, block.Behavior, samus.Pose, true);
            selected = true;
        }
        AssertTrue(selected, "the Ki Hunter room has an exit");
        var audio = game.AudioForVerification;
        byte start = audio.SoundQueueForVerification(1).Next;
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.HitDoorBlock);
        game.Step(0);

        AssertEqual((byte)0x4c, audio.SoundQueueEntryForVerification(1, start),
            "the entry frame's enemy pass queues the spit first");
        AssertEqual((byte)0x71, audio.SoundQueueEntryForVerification(1, (start + 1) & 0x0f),
            "the door's library-two cancel follows the enemy sound");
        Console.WriteLine("Door entry enemy sound: the entry frame's enemy sounds precede the door's cancel and DisableSounds.");
    }
}
