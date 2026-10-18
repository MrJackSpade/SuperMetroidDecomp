using SuperMetroid.Core;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Enemies.KiHunters;
using SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that one Ki-Hunter spit requests one sound and cannot prolong a door sound wait.</summary>
internal static class KiHunterSpitAudioVerification
{
    /// <summary>Checks the native sound opcode, real projectile spawn, single SPC start, and door transition.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the KiHunter spit instruction that queues sound $4C")]
    internal static void VerifyKiHunterSpitAudio()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] nativeQueueCall = [0xa9, 0x4c, 0x00, 0x22, 0xcb, 0x90, 0x80];
        AssertSequenceEqual(nativeQueueCall, Enumerable.Range(0, nativeQueueCall.Length)
            .Select(offset => bus.ReadCartridgeByte(0xa8f6dc + offset)),
            "retail spit instruction loads $4C and calls QueueSfx2_Max6 once");
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0x948c);
        var samus = runtime.Player.IntroducedSamus;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(64, 128));
        samus.InputLocked = true;
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation();
        var body = runtime.Enemies.SlotTable.Slots.First(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.KihunterGreen);
        var state = runtime.Enemies.KiHunterStates[body.NativeIndex / 64];
        body.Position = body.Position.WithWhole(new(128, 128));
        (state ?? throw new InvalidOperationException("Verification requires state.")).Function = KiHunterEnemyFunction.NoOp;
        // Stage the real spit opcode; subsequent frames must not replay its one-shot call.
        // The spit cue, four poses into the right spit animation.
        state.Program = new AnimationProgramCursor<KiHunterProgram>(KiHunterProgram.SpitRight, 4);
        body.InstructionTimer = NativeCountdown.Start(1);
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false);
        game.Session.runtime = runtime;
        game.Session.lastAudioRoomStatePointer = runtime.Room.LoadedRoom.State.Pointer;
        game.GameState = SuperMetroidGameState.MainGameplay;
        game.SoundAcknowledgements = new PortWriteEchoAcknowledgements(game.Session.audio);
        var spit = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x4c);
        int requests = 0, starts = 0;
        void Step()
        {
            var frame = game.Step(0);
            requests += runtime.Enemies.Audio.SoundRequests.Count(request => request.SoundEffect == spit);
            starts += game.AudioEventsForVerification.Count(audioEvent => audioEvent.Kind == AudioEventKind.StartSoundEffect && audioEvent.SoundEffect == spit);
        }
        Step();
        AssertEqual(1, runtime.Enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.Kind == RoomEnemyProjectileKind.KiHunterAcidSpitRight), "staged attack spawns one real acid projectile");
        AssertTrue(runtime.Enemies.Audio.SoundRequests.Any(request => request.SoundEffect == spit && request.MaximumQueued == 6), "spit calls library two Max6");
        // Remain in the post-spit waiting state without introducing another attack.
        // The right spit animation's final sleep.
        state.Program = new AnimationProgramCursor<KiHunterProgram>(KiHunterProgram.SpitRight, 8);
        body.InstructionTimer = NativeCountdown.Start(1);
        for (int frame = 1; frame < 20; frame++) Step();
        Console.WriteLine($"One KiHunter spit: {requests} requests, {starts} SPC starts over 20 frames.");
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
        AssertTrue(selected, "real KiHunter room has an exit");
        game.GameState = SuperMetroidGameState.HitDoorBlock;
        int waitFrames = 0;
        for (int frame = 0; frame < 32; frame++)
        {
            if (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.WaitForSoundQueues) waitFrames++;
            Step();
            if (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.FadeOutSourcePalette) break;
        }
        Console.WriteLine($"Door sound wait: {waitFrames} frames; phase={game.DoorTransitionPhaseForVerification}.");
        AssertEqual(1, requests, "one native spit opcode publishes exactly one request, including door-wait frames");
        AssertEqual(1, starts, "the SPC starts the acknowledged spit once");
        AssertEqual(DoorTransitionPhase.FadeOutSourcePalette, game.DoorTransitionPhaseForVerification, "completed spit cannot refill the door sound wait");
        AssertTrue(waitFrames <= 1, "already-drained spit queue allows the first door sound-wait pass to finish");
    }

    /// <summary>Runs the focused Ki-Hunter spit sound-routing check.</summary>
    [VerificationCommand("--kihunter-spit-audio")]
    internal static int KihunterSpitAudioCommand()
    {
     VerifyKiHunterSpitAudio(); return 0; 
    }
}
