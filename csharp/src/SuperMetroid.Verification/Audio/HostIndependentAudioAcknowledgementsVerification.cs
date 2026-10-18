using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;

/// <summary>Confirms gameplay audio acknowledgements no longer depend on whether the host renders audio (#1303).</summary>
internal static class HostIndependentAudioAcknowledgementsVerification
{
    /// <summary>Updates allowed for the door transition to finish waiting on the sound queues.</summary>
    private const int DoorFrames = 300;

    /// <summary>
    /// The SFX handshake must not depend on whether a host renders audio (#1303). Two
    /// identical Landing Site sessions enter the same door with library 2 already waiting
    /// for an echo. The silent subject has no audio catalog and no host audio calls (audio
    /// disabled or no device); the playing subject binds the catalog and copies its PCM every
    /// update (audio enabled). Every frame must agree. Backends publish no acknowledgements
    /// (#1304), so the oracle for the playing subject's PCM is a separate backend driven only by
    /// the silent subject's published audio events: the events a session publishes without
    /// synthesis must reproduce the synthesizing session's sound exactly.
    /// </summary>
    internal static void VerifyHostIndependentAudioAcknowledgements()
    {
        var assets = RepositoryInstallation.Installation.LoadAudioClips();
        var oracle = new ClipAudioBackend(assets);
        SuperMetroidGame silent = CreateLandingSiteSession();
        SuperMetroidGame playing = CreateLandingSiteSession();
        playing.BindAudio(assets);
        SuperMetroidGame[] subjects = [silent, playing];
        SuperMetroidGame[] sessions = subjects;
        var pcm = new short[CartridgeAudioSession.PcmSamplesPerFrame];
        var expectedPcm = new short[CartridgeAudioSession.PcmSamplesPerFrame];
        int update = 0;

        void StepAll()
        {
            FrontendFrame silentFrame = silent.Step(0);
            FrontendFrame playingFrame = playing.Step(0);
            AssertSequenceEqual(silent.AudioEventsForVerification, playing.AudioEventsForVerification, $"published audio events after update {update}");
            AssertEqual(silent.AudioAcknowledgementsForVerification, playing.AudioAcknowledgementsForVerification,
                $"session acknowledgements after update {update}");
            oracle.Render(new AudioFrameEvents(silent.AudioEventsForVerification), expectedPcm);
            playing.CopyAudioFrame(pcm);
            AssertTrue(pcm.SequenceEqual(expectedPcm), $"session PCM after update {update}");
            update++;
        }

        // The first frame carries the reset upload, as a booted session's does.
        StepAll();
        foreach (SuperMetroidGame game in sessions)
            game.AudioForVerification.QueueSound(SoundEffectLibrary2Sounds.DoorOpening, 6);
        StepAll();
        foreach (SuperMetroidGame game in sessions)
            AssertEqual((byte)1, game.AudioForVerification.SoundQueueForVerification(1).State,
                "library 2 waits for its echo before the door");

        foreach (SuperMetroidGame game in sessions)
            EnterLandingSiteDoor(game);
        int silentLeftSoundWait = -1;
        for (int frame = 0; frame < DoorFrames; frame++)
        {
            StepAll();
            AssertEqual(silent.DoorTransitionPhaseForVerification, playing.DoorTransitionPhaseForVerification,
                $"door phase after door frame {frame}");
            AssertEqual(silent.AudioForVerification.HasQueuedSounds, playing.AudioForVerification.HasQueuedSounds,
                $"queued sounds after door frame {frame}");
            for (int library = 0; library < 3; library++)
                AssertEqual(silent.AudioForVerification.SoundQueueForVerification(library),
                    playing.AudioForVerification.SoundQueueForVerification(library),
                    $"library {library + 1} queue after door frame {frame}");
            if (silentLeftSoundWait < 0 && silent.DoorTransitionPhaseForVerification > DoorTransitionPhase.WaitForSoundQueues)
                silentLeftSoundWait = frame;
        }
        AssertTrue(silentLeftSoundWait >= 0, "the sessions leave WaitForSoundQueues");
        Console.WriteLine($"  Host-independent acknowledgements: both sessions left the door sound wait at door frame {silentLeftSoundWait}.");
    }

    /// <summary>Builds a retail gameplay session standing in the Landing Site with a reset audio queue.</summary>
    private static SuperMetroidGame CreateLandingSiteSession()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        runtime.Player.IntroducedSamus.InputLocked = true;
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false);
        game.Session.runtime = runtime;
        game.Session.lastAudioRoomStatePointer = runtime.Room.LoadedRoom.State.Pointer;
        game.GameState = SuperMetroidGameState.MainGameplay;
        game.AudioForVerification.Reset();
        return game;
    }

    /// <summary>Resolves the Landing Site's first door block and enters the door transition.</summary>
    private static void EnterLandingSiteDoor(SuperMetroidGame game)
    {
        SuperMetroidRuntime runtime = game.GameplayRuntimeForVerification;
        var level = runtime.Room.LoadedLevelData;
        for (int y = 0; y < level.Size.Y; y++)
        for (int x = 0; x < level.Size.X; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            level.ResolveDoorCollision(block.Behavior, runtime.Player.IntroducedSamus.Pose, true);
            game.GameState = SuperMetroidGameState.HitDoorBlock;
            return;
        }
        throw new InvalidDataException("Landing Site has no door block.");
    }

    /// <summary>Runs the host-independent audio acknowledgement suite.</summary>
    [VerificationCommand("--host-independent-audio-acknowledgements")]
    internal static int HostIndependentAudioAcknowledgementsCommand()
    {
        Suite(nameof(VerifyHostIndependentAudioAcknowledgements), () => VerifyHostIndependentAudioAcknowledgements());
        return 0;
    }
}
