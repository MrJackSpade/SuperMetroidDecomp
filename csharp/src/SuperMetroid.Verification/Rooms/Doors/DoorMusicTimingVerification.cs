using SuperMetroid.Core;
using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;

/// <summary>Checks room music dispatch timing during a retail Parlor-to-Landing-Site door transition.</summary>
internal static class DoorMusicTimingVerification
{
    /// <summary>Checks post-scroll stop, upload, and destination-track delays plus unchanged and zero-track handling.</summary>
    internal static void VerifyDoorMusicTiming()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0x92fd);
        var level = runtime.Room.LoadedLevelData;
        int doorX = -1, doorY = -1;
        int behavior = 0;
        for (int y = 0; y < level.Size.Y && doorX < 0; y++)
        for (int x = 0; x < level.Size.X; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            var door = level.ResolveDoorCollision(block.Bts.Value, SamusPoseId.FacingRightNormalPose, false);
            if (door.Door?.DestinationRoomPointer != 0x91f8) continue;
            doorX = x; doorY = y; behavior = block.Bts.Value; break;
        }
        AssertTrue(doorX >= 0, "Parlor has the retail Landing Site door");
        runtime.LoadCartridgeRoomForDebug(0x92fd,
            camera: new Position16<RoomPixels>((ushort)(doorX / 16 * 256), (ushort)(doorY / 16 * 256)));
        var samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new((ushort)(doorX * 16 + 8), (ushort)(doorY * 16 + 8)));
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation();
        runtime.Room.LoadedLevelData.ResolveDoorCollision(behavior, samus.Pose, true);
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false);
        game.Session.runtime = runtime;
        game.Session.lastAudioRoomStatePointer = runtime.Room.LoadedRoom.State.Pointer;
        game.GameState = SuperMetroidGameState.HitDoorBlock;
        var audio = game.Session.audio;
        audio.AdvanceFrame(default);
        audio.QueueRoomMusic(9, 5);
        for (int frame = 0; frame < 40; frame++) audio.AdvanceFrame(default);
        AssertEqual((byte)9, audio.MusicDataIndex, "source music data is active");
        AssertEqual((byte)5, audio.MusicTrackIndex, "source track is active");
        game.SoundAcknowledgements = new PortWriteEchoAcknowledgements(audio);
        int trackFrame = -1, musicWaitFrame = -1, uploadFrame = -1;
        for (int frame = 0; frame < 400; frame++)
        {
            var phase = game.DoorTransitionPhaseForVerification;
            if (phase == DoorTransitionPhase.WaitForMusicQueue && musicWaitFrame < 0)
                musicWaitFrame = frame;
            var output = game.Step(0);
            // Bank loads and music-port writes are published in the update the CPU issues them.
            foreach (var audioEvent in game.AudioEventsForVerification)
            {
                if (audioEvent.Kind == AudioEventKind.LoadAudioBank) uploadFrame = frame;
                if (audioEvent == AudioEvent.WriteMusicPort(MusicCommand.SelectTrack(5)))
                {
                    Console.WriteLine($"Destination music at frame {frame}, phase {phase}, music wait frame {musicWaitFrame}.");
                    AssertTrue(musicWaitFrame >= 0, "destination track must not start before door scrolling finishes");
                    trackFrame = frame;
                }
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay && trackFrame >= 0) break;
        }
        AssertEqual((ushort)0x91f8, runtime.Room.LoadedRoom.Pointer, "transition reaches Landing Site");
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
        for (int frame = 0; frame < 9; frame++) stopCommands.AddRange(audio.AdvanceFrame(default));
        AssertTrue(stopCommands.Contains(CartridgeAudioCommand.WritePort(ApuPort.Music, 0)), "nonzero masked track-zero remains an explicit stop");
        Console.WriteLine("  Door music: IRQ-only waits preserve queued delays; post-scroll stop/upload and track dispatch timing agree.");
    }

    /// <summary>Runs the door music timing verification.</summary>
    [VerificationCommand("--door-music-timing")]
    internal static int DoorMusicTimingCommand()
    {
     VerifyDoorMusicTiming(); return 0; 
    }
}
