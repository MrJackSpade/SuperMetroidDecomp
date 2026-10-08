using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyKiHunterSpitAudio()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] nativeQueueCall = [0xa9, 0x4c, 0x00, 0x22, 0xcb, 0x90, 0x80];
        AssertSequenceEqual(nativeQueueCall, Enumerable.Range(0, nativeQueueCall.Length)
            .Select(offset => bus.ReadCartridgeByte(0xa8f6dc + offset)),
            "retail spit instruction loads $4C and calls QueueSfx2_Max6 once");
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
        var body = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer == RoomEnemySystem.KiHunterDefinition);
        var state = runtime.Enemies.KiHunterStates[body.NativeIndex / 64]!;
        body.XPosition = 128; body.YPosition = 128;
        state.Function = KiHunterEnemyFunction.NoOp;
        // Stage the real spit opcode; subsequent frames must not replay its one-shot call.
        body.CurrentInstruction = (ushort)(KiHunterInstructionProgramDefinitions.SpitRight + 0x10);
        body.InstructionTimer = 1;
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(SuperMetroidGame).GetField("runtime", flags)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", flags)!.SetValue(game, runtime.ActiveRoom!.State.Pointer);
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
        byte[] ports = new byte[4];
        var spit = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x4c);
        int requests = 0, writes = 0;
        void Step()
        {
            game.SetAudioAcknowledgements(new(ports[0], ports[1], ports[2], ports[3]));
            var frame = game.Step(0);
            requests += runtime.Enemies.SoundRequests.Count(request => request.SoundEffect == spit);
            foreach (var command in frame.AudioCommands)
            {
                if (command.Kind != CartridgeAudioCommandKind.WritePort) continue;
                ports[command.Port] = command.Value;
                if (command.Port == 2 && command.Value == 0x4c) writes++;
            }
        }
        Step();
        AssertEqual(1, runtime.Enemies.EnemyProjectiles.Count(projectile => projectile.Kind == RoomEnemyProjectileKind.KiHunterAcidSpitRight), "staged attack spawns one real acid projectile");
        AssertTrue(runtime.Enemies.SoundRequests.Any(request => request.SoundEffect == spit && request.MaximumQueued == 6), "spit calls library two Max6");
        // Remain in the post-spit waiting state without introducing another attack.
        body.CurrentInstruction = (ushort)(KiHunterInstructionProgramDefinitions.SpitRight + 0x1c);
        body.InstructionTimer = 1;
        for (int frame = 1; frame < 20; frame++) Step();
        Console.WriteLine($"One KiHunter spit: {requests} requests, {writes} APU writes over 20 frames.");
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
        AssertTrue(selected, "real KiHunter room has an exit");
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.HitDoorBlock);
        int waitFrames = 0;
        for (int frame = 0; frame < 32; frame++)
        {
            if (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.WaitForSoundQueues) waitFrames++;
            Step();
            if (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.FadeOutSourcePalette) break;
        }
        Console.WriteLine($"Door sound wait: {waitFrames} frames; phase={game.DoorTransitionPhaseForVerification}.");
        AssertEqual(1, requests, "one native spit opcode publishes exactly one request, including door-wait frames");
        AssertEqual(1, writes, "acknowledged spit is sent once to APU port two");
        AssertEqual(DoorTransitionPhase.FadeOutSourcePalette, game.DoorTransitionPhaseForVerification, "completed spit cannot refill the door sound wait");
        AssertTrue(waitFrames <= 1, "already-drained spit queue allows the first door sound-wait pass to finish");
    }
}