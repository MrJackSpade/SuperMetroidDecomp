using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailGameFixtureVerification;
using static RetailRuntimeFixtureVerification;
using static SuiteTimingVerification;

/// <summary>Confirms gameplay audio acknowledgements no longer depend on whether the host renders audio (#1303).</summary>
internal static class HostIndependentAudioAcknowledgementsVerification
{
    /// <summary>Updates allowed for the door transition to finish waiting on the sound queues.</summary>
    private const int DoorFrames = 300;

    /// <summary>
    /// The SFX handshake must not depend on whether a host renders audio (#1303). Three
    /// identical Landing Site sessions enter the same door with library 2 already waiting
    /// for an echo. The oracle reads a separate renderer's acknowledgements, as the desktop
    /// host fed them back before #1303 with audio enabled. The silent subject has no audio
    /// catalog and no host audio calls (audio disabled or no device); the playing subject binds
    /// the catalog and copies its PCM every update (audio enabled). Every frame must agree, the
    /// sessions' own acknowledgement bytes must equal the renderer's, and the playing subject's
    /// PCM must equal the renderer's.
    /// </summary>
    internal static void VerifyHostIndependentAudioAcknowledgements()
    {
        var assets = RepositoryInstallation.Installation.LoadAudio();
        var renderer = new CartridgeAudioRenderer(assets);
        SuperMetroidGame oracle = CreateLandingSiteSession();
        oracle.SoundAcknowledgements = new RendererAcknowledgements(renderer);
        SuperMetroidGame silent = CreateLandingSiteSession();
        SuperMetroidGame playing = CreateLandingSiteSession();
        playing.BindAudio(assets);
        SuperMetroidGame[] subjects = [silent, playing];
        SuperMetroidGame[] sessions = [oracle, silent, playing];
        var pcm = new short[CartridgeAudioSession.PcmSamplesPerFrame];
        int update = 0;

        void StepAll()
        {
            FrontendFrame frame = oracle.Step(0);
            short[] expectedPcm = renderer.RenderFrame(frame.AudioCommands);
            CartridgeAudioAcknowledgements expected = renderer.ReadAcknowledgements();
            foreach (SuperMetroidGame subject in subjects)
            {
                subject.Step(0);
                CartridgeAudioAcknowledgements actual = subject.AudioAcknowledgementsForVerification;
                AssertEqual((expected.Port1, expected.Port2, expected.Port3), (actual.Port1, actual.Port2, actual.Port3),
                    $"session acknowledgements after update {update}");
            }
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
        int oracleLeftSoundWait = -1;
        for (int frame = 0; frame < DoorFrames; frame++)
        {
            StepAll();
            foreach (SuperMetroidGame subject in subjects)
            {
                AssertEqual(oracle.DoorTransitionPhaseForVerification, subject.DoorTransitionPhaseForVerification,
                    $"door phase after door frame {frame}");
                AssertEqual(oracle.AudioForVerification.HasQueuedSounds, subject.AudioForVerification.HasQueuedSounds,
                    $"queued sounds after door frame {frame}");
                for (int library = 0; library < 3; library++)
                    AssertEqual(oracle.AudioForVerification.SoundQueueForVerification(library),
                        subject.AudioForVerification.SoundQueueForVerification(library),
                        $"library {library + 1} queue after door frame {frame}");
            }
            if (oracleLeftSoundWait < 0 && oracle.DoorTransitionPhaseForVerification > DoorTransitionPhase.WaitForSoundQueues)
                oracleLeftSoundWait = frame;
        }
        AssertTrue(oracleLeftSoundWait >= 0, "the renderer-fed session leaves WaitForSoundQueues");
        Console.WriteLine($"  Host-independent acknowledgements: all sessions left the door sound wait at door frame {oracleLeftSoundWait}.");
    }

    /// <summary>The pre-#1303 audio-enabled host path: acknowledgements read back from a separate renderer.</summary>
    private sealed class RendererAcknowledgements(CartridgeAudioRenderer renderer) : ISoundAcknowledgementSource
    {
        /// <summary>Returns the renderer's acknowledgements in place of the session's modeled ones.</summary>
        public CartridgeAudioAcknowledgements ForNextFrame(CartridgeAudioAcknowledgements modeled) =>
            renderer.ReadAcknowledgements();
    }

    /// <summary>Builds a retail gameplay session standing in the Landing Site with a reset audio queue.</summary>
    private static SuperMetroidGame CreateLandingSiteSession()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        runtime.Player.IntroducedSamus.InputLocked = true;
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        game.runtime = runtime;
        game.lastAudioRoomStatePointer = runtime.Room.LoadedRoom.State.Pointer;
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
            level.ResolveDoorCollision(game.bus, block.Behavior, runtime.Player.IntroducedSamus.Pose, true);
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
