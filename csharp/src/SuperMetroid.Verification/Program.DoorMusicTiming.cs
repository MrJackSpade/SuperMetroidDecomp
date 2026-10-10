using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyDoorMusicTiming()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0x92fd);
        var level = runtime.LevelData!;
        int doorX = -1, doorY = -1;
        byte behavior = 0;
        for (int y = 0; y < level.HeightInBlocks && doorX < 0; y++)
        for (int x = 0; x < level.WidthInBlocks; x++)
        {
            var block = level.GetCollisionBlock(x, y);
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            var door = level.ResolveDoorCollision(bus, block.Behavior, 1, false);
            if (door.Door?.DestinationRoomPointer != 0x91f8) continue;
            doorX = x; doorY = y; behavior = block.Behavior; break;
        }
        AssertTrue(doorX >= 0, "Parlor has the retail Landing Site door");
        runtime.LoadCartridgeRoomForDebug(0x92fd,
            cameraX: (ushort)(doorX / 16 * 256), cameraY: (ushort)(doorY / 16 * 256));
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.XPosition = (ushort)(doorX * 16 + 8);
        samus.YPosition = (ushort)(doorY * 16 + 8);
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.LevelData!.ResolveDoorCollision(bus, behavior, samus.Pose, true);
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(SuperMetroidGame).GetField("runtime", flags)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", flags)!.SetValue(game, runtime.ActiveRoom!.State.Pointer);
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.HitDoorBlock);
        var audio = (CartridgeAudioState)typeof(SuperMetroidGame).GetField("audio", flags)!.GetValue(game)!;
        audio.AdvanceFrame(bus, default);
        audio.QueueRoomMusic(9, 5);
        for (int frame = 0; frame < 40; frame++) audio.AdvanceFrame(bus, default);
        AssertEqual((byte)9, audio.MusicDataIndex, "source music data is active");
        AssertEqual((byte)5, audio.MusicTrackIndex, "source track is active");
        byte[] ports = new byte[4];
        int trackFrame = -1, musicWaitFrame = -1, uploadFrame = -1;
        for (int frame = 0; frame < 400; frame++)
        {
            var phase = game.DoorTransitionPhaseForVerification;
            if (phase == DoorTransitionPhase.WaitForMusicQueue && musicWaitFrame < 0)
                musicWaitFrame = frame;
            game.SetAudioAcknowledgements(new(ports[0], ports[1], ports[2], ports[3]));
            var output = game.Step(0);
            foreach (var command in output.AudioCommands)
            {
                if (command.Kind == CartridgeAudioCommandKind.Upload) uploadFrame = frame;
                if (command.Kind != CartridgeAudioCommandKind.WritePort) continue;
                ports[command.Port] = command.Value;
                if (command.Port == 0 && command.Value == 5)
                {
                    Console.WriteLine($"Destination music at frame {frame}, phase {phase}, music wait frame {musicWaitFrame}.");
                    AssertTrue(musicWaitFrame >= 0, "destination track must not start before door scrolling finishes");
                    trackFrame = frame;
                }
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay && trackFrame >= 0) break;
        }
        AssertEqual((ushort)0x91f8, runtime.ActiveRoom!.Pointer, "transition reaches Landing Site");
        AssertEqual(musicWaitFrame + 15, uploadFrame, "stop and upload delays advance on outer dispatches after scrolling");
        AssertEqual(uploadFrame + 9, trackFrame, "track queued after upload is acquired on the next prologue, then waits eight dispatches");
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "music transition resumes gameplay");
        AssertEqual((byte)6, audio.MusicDataIndex, "destination music bank is active");
        audio.QueueRoomMusicTrack(6, 5);
        AssertTrue(!audio.HasQueuedMusic, "unchanged native music pair queues nothing");
        audio.QueueRoomMusicTrack(0, 0);
        AssertTrue(!audio.HasQueuedMusic, "zero room track preserves inherited music");
        audio.QueueRoomMusicTrack(6, 0x80);
        var stopCommands = new List<CartridgeAudioCommand>();
        for (int frame = 0; frame < 9; frame++) stopCommands.AddRange(audio.AdvanceFrame(bus, default));
        AssertTrue(stopCommands.Contains(CartridgeAudioCommand.WritePort(0, 0)), "nonzero masked track-zero remains an explicit stop");
        Console.WriteLine("  Door music: IRQ-only waits preserve queued delays; post-scroll stop/upload and track dispatch timing agree.");
    }
}