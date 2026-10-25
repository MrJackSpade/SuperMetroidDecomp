using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Rooms.Doors;

using SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

/// <summary>Checks room music dispatch timing during a retail Parlor-to-Landing-Site door transition.</summary>
internal static class DoorMusicTimingVerification
{
    /// <summary>Checks post-scroll stop, upload, and destination-track delays plus unchanged and zero-track handling.</summary>
    internal static void VerifyDoorMusicTiming()
    {
        var random = new SharedRandom();
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true, random: random);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Parlor);
        var level = runtime.Room.LoadedLevelData;
        int doorX = -1, doorY = -1;
        int behavior = 0;
        for (int y = 0; y < level.Size.Y && doorX < 0; y++)
        for (int x = 0; x < level.Size.X; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            var door = level.ResolveDoorCollision(block.Bts.Value, SamusPoseId.FacingRightNormalPose.ForRoomCollision(), false);
            if (door.Door?.DestinationRoom != RoomHeaderDefinitions.LandingSite) continue;
            doorX = x; doorY = y; behavior = block.Bts.Value; break;
        }
        AssertTrue(doorX >= 0, "Parlor has the retail Landing Site door");
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Parlor,
            camera: new Position16<RoomPixels>((ushort)(doorX / 16 * 256), (ushort)(doorY / 16 * 256)));
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new((ushort)(doorX * 16 + 8), (ushort)(doorY * 16 + 8)));
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        runtime.Room.LoadedLevelData.ResolveDoorCollision(behavior, samus.Pose.ForRoomCollision(), true);
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false,
            hostServices: SuperMetroidGameHostServices.Default with { SoundAcknowledgements = PortWriteEchoAcknowledgements.Instance }, random: random);
        game.AdoptRuntime(runtime);
        game.Session.LastAudioRoomState = runtime.Room.LoadedRoom.State;
        game.GameState = SuperMetroidGameState.HitDoorBlock;
        var audio = game.Session.audio;
        audio.AdvanceFrame(default);
        audio.QueueRoomMusic(new RoomMusic(MusicDataSet.LowerCrateria, RoomMusicTrack.Plays(5)));
        for (int frame = 0; frame < 40; frame++) audio.AdvanceFrame(default);
        AssertEqual(MusicDataSet.LowerCrateria, audio.DataSet, "source music data is active");
        AssertEqual(new MusicTrack(5), audio.PlayingTrack, "source track is active");
        int trackFrame = -1, musicWaitFrame = -1, uploadFrame = -1;
        for (int frame = 0; frame < 400; frame++)
        {
            var phase = game.DoorTransitionPhaseForVerification;
            if (phase == DoorTransitionPhase.WaitForMusicQueue && musicWaitFrame < 0)
                musicWaitFrame = frame;
            game.Step(0);
            // Bank loads and music-port writes are published in the update the CPU issues them.
            foreach (var audioEvent in game.AudioEventsForVerification)
            {
                if (audioEvent is AudioEvent.ChangeMusicDataSet) uploadFrame = frame;
                if (audioEvent == new AudioEvent.SelectMusicTrack(new MusicTrack(5)))
                {
                    Console.WriteLine($"Destination music at frame {frame}, phase {phase}, music wait frame {musicWaitFrame}.");
                    AssertTrue(musicWaitFrame >= 0, "destination track must not start before door scrolling finishes");
                    trackFrame = frame;
                }
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay && trackFrame >= 0) break;
        }
        AssertEqual(RoomHeaderDefinitions.LandingSite, runtime.Room.LoadedRoom.Definition, "transition reaches Landing Site");
        AssertEqual(musicWaitFrame + 15, uploadFrame, "stop and upload delays advance on outer dispatches after scrolling");
        AssertEqual(uploadFrame + 9, trackFrame, "track queued after upload is acquired on the next prologue, then waits eight dispatches");
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "music transition resumes gameplay");
        AssertEqual(MusicDataSet.EmptyCrateria, audio.DataSet, "destination music data set is active");
        audio.QueueRoomMusicTrack(new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(5)));
        AssertTrue(!audio.HasQueuedMusic, "unchanged data set and track queues nothing");
        audio.QueueRoomMusicTrack(RoomMusic.Inherited);
        AssertTrue(!audio.HasQueuedMusic, "an inherited room track preserves the playing music");
        audio.QueueRoomMusicTrack(new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Silenced));
        var stopCommands = new List<AudioRequest>();
        for (int frame = 0; frame < 9; frame++) stopCommands.AddRange(audio.AdvanceFrame(default));
        AssertTrue(stopCommands.Contains(new AudioRequest.StopMusic()), "a silenced room track is an explicit stop");
        Console.WriteLine("  Door music: IRQ-only waits preserve queued delays; post-scroll stop/upload and track dispatch timing agree.");
    }

    /// <summary>Runs the door music timing verification.</summary>
    [VerificationCommand("--door-music-timing")]
    internal static int DoorMusicTimingCommand()
    {
     VerifyDoorMusicTiming(); return 0; 
    }
}
