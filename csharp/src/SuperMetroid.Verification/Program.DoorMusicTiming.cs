using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailGameFixtureVerification;
using static RetailRuntimeFixtureVerification;

internal static class DoorMusicTimingVerification
{
    internal static void VerifyDoorMusicTiming()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0x92fd);
        var level = runtime.Room.LevelData!;
        int doorX = -1, doorY = -1;
        byte behavior = 0;
        for (int y = 0; y < level.Size.Y && doorX < 0; y++)
        for (int x = 0; x < level.Size.X; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            var door = level.ResolveDoorCollision(bus, block.Behavior, SamusPoseId.FacingRightNormalPose, false);
            if (door.Door?.DestinationRoomPointer != 0x91f8) continue;
            doorX = x; doorY = y; behavior = block.Behavior; break;
        }
        AssertTrue(doorX >= 0, "Parlor has the retail Landing Site door");
        runtime.LoadCartridgeRoomForDebug(0x92fd,
            camera: new Position16<RoomPixels>((ushort)(doorX / 16 * 256), (ushort)(doorY / 16 * 256)));
        var samus = runtime.Player.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new((ushort)(doorX * 16 + 8), (ushort)(doorY * 16 + 8)));
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        runtime.Room.LevelData!.ResolveDoorCollision(bus, behavior, samus.Pose, true);
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        game.runtime = runtime;
        game.lastAudioRoomStatePointer = runtime.Room.ActiveRoom!.State.Pointer;
        game.GameState = SuperMetroidGameState.HitDoorBlock;
        var audio = game.audio;
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
                ports[(int)command.InputPort] = command.Value;
                if (command.InputPort == ApuPort.Music && command.Value == 5)
                {
                    Console.WriteLine($"Destination music at frame {frame}, phase {phase}, music wait frame {musicWaitFrame}.");
                    AssertTrue(musicWaitFrame >= 0, "destination track must not start before door scrolling finishes");
                    trackFrame = frame;
                }
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay && trackFrame >= 0) break;
        }
        AssertEqual((ushort)0x91f8, runtime.Room.ActiveRoom!.Pointer, "transition reaches Landing Site");
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
        AssertTrue(stopCommands.Contains(CartridgeAudioCommand.WritePort(ApuPort.Music, 0)), "nonzero masked track-zero remains an explicit stop");
        Console.WriteLine("  Door music: IRQ-only waits preserve queued delays; post-scroll stop/upload and track dispatch timing agree.");
    }

    [VerificationCommand("--door-music-timing")]
    internal static int DoorMusicTimingCommand()
    {
     VerifyDoorMusicTiming(); return 0; 
    }
}