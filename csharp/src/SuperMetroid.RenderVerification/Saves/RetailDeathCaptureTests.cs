using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.RenderVerification.Hud;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Saves;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Compares captured death and reserve-recovery frames with the legacy raster and audio path.</summary>
internal static class RetailDeathCaptureTests
{
    /// <summary>Drives a zero-health fixture through death, blackouts, and game-over handoff.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
        => Run(device, renderer, reserveRecovery: false);

    /// <summary>Drives automatic reserve recovery and checks the return to unlocked gameplay.</summary>
    internal static void RunReserveRecovery(D3D11RenderDevice device, D3D11FrameRenderer renderer)
        => Run(device, renderer, reserveRecovery: true);

    /// <summary>Checks frame state, audio commands, PCM, and rendered pixels through the selected death path.</summary>
    private static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer, bool reserveRecovery)
    {
        var options = new SuperMetroid.Core.Gameplay.GameplayAssists { SkipOpeningCinematic = true };
        var legacy = InstalledFixtures.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), options);
        var captured = InstalledFixtures.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), options);
        var audioAssets = DesktopAccess.LoadAudioClips();
        legacy.BindAudio(audioAssets);
        captured.BindAudio(audioAssets);
        var legacyAudio = new short[AudioSession.PcmSamplesPerFrame];
        var capturedAudio = new short[AudioSession.PcmSamplesPerFrame];
        long sequence = 0;
        void Step(ControllerButton input, bool comparePixels)
        {
            var expected = legacy.Step(input);
            sequence++;
            var actual = captured.StepCaptured(input);
            if (expected.GameState != actual.Frame.GameState ||
                expected.Phase != actual.Frame.Phase || expected.FrameNumber != actual.Frame.FrameNumber ||
                !expected.Audio.Events.SequenceEqual(actual.Frame.Audio.Events))
                throw new InvalidOperationException($"Death capture cadence/commands diverged at {sequence}.");
            legacy.CopyAudioFrame(legacyAudio);
            captured.CopyAudioFrame(capturedAudio);
            if (!legacyAudio.SequenceEqual(capturedAudio))
                throw new InvalidOperationException($"Death PCM diverged at {sequence}.");
            if (comparePixels) PixelComparison.Verify(actual.Snapshot, legacy.PublishedPixels(),
                renderer.RenderForReadback(actual.Snapshot), $"{device.Kind}: death frame {sequence}, {expected.GameState}");
        }
        for (int tick = 0; tick < 2000 && legacy.GameState != SuperMetroidGameState.MainGameplay; tick++)
            Step(tick % 47 == 0 ? ControllerButton.Start : ControllerButton.None, false);
        if (legacy.GameState != SuperMetroidGameState.MainGameplay) throw new InvalidOperationException("Death fixture startup failed.");
        foreach (var game in new[] { legacy, captured })
        {
            game.GameplayRuntimeForVerification.LoadCartridgeRoomForDebug(RoomDefinitions.AlphaPowerBomb, new Position16<RoomPixels>(0, 0));
            game.GameplayRuntimeForVerification.FrameBoundary.BeginAcceptedFrame(0);
            game.GameplayRuntimeForVerification.Player.Samus.Health = 0;
            game.GameplayRuntimeForVerification.Player.Samus.MaxReserveEnergy = ReserveCaptureFixtureDefinitions.Capacity;
            game.GameplayRuntimeForVerification.Player.Samus.ReserveEnergy = reserveRecovery ? ReserveCaptureFixtureDefinitions.Capacity : (ushort)0;
            game.GameplayRuntimeForVerification.Player.Samus.ReserveTankMode = ReserveCaptureFixtureDefinitions.AutomaticMode;
        }
        var states = new HashSet<object>();
        int frames = 0;
        for (; frames < 2000; frames++)
        {
            Step(0, true);
            states.Add(legacy.CurrentStep);
            // The dispatch that enters the game-over menu releases the runtime; both owners must release it together.
            var expectedRuntime = legacy.RuntimeForVerification;
            var actualRuntime = captured.RuntimeForVerification;
            if ((expectedRuntime is null) != (actualRuntime is null))
                throw new InvalidOperationException("Death/recovery runtime release diverged between captured and legacy owners.");
            if (expectedRuntime is not null && actualRuntime is not null)
            {
                var expectedSamus = expectedRuntime.Player.Samus;
                var actualSamus = actualRuntime.Player.Samus;
                if (expectedSamus.Health != actualSamus.Health || expectedSamus.ReserveEnergy != actualSamus.ReserveEnergy ||
                    expectedSamus.Control.InputLocked != actualSamus.Control.InputLocked ||
                    expectedRuntime.Freeze.GameplayTimeFrozen != actualRuntime.Freeze.GameplayTimeFrozen)
                    throw new InvalidOperationException("Death/recovery state diverged between captured and legacy owners.");
            }
            if (reserveRecovery && states.Contains(SuperMetroidGameState.ReserveTankRefill) &&
                legacy.GameState == SuperMetroidGameState.MainGameplay) break;
            if (legacy.GameState == SuperMetroidGameState.GameOverMenu) break;
        }
        if (reserveRecovery)
        {
            var samus = legacy.GameplayRuntimeForVerification.Player.Samus;
            if (frames == 2000 || !states.Contains(SuperMetroidGameState.ReserveTankRefill) ||
                legacy.GameState != SuperMetroidGameState.MainGameplay || samus.Health == 0 ||
                samus.ReserveEnergy != 0 || samus.Control.InputLocked || legacy.GameplayRuntimeForVerification.Freeze.GameplayTimeFrozen)
                throw new InvalidOperationException("Reserve fixture missed recovery, depletion or release.");
            Console.WriteLine($"{device.Kind}: {frames + 1} exact automatic reserve recovery frames, matching PCM, energy/lock/freeze state and gameplay handoff.");
            return;
        }
        if (frames == 2000 || !states.Contains(DeathSequenceStep.BlackingOutSurroundings) ||
            !states.Contains(SamusDeathSequencePhase.SuitExplosion) || !states.Contains(DeathSequenceStep.FinalBlackOut))
            throw new InvalidOperationException("Death fixture missed blackout, explosion or game-over handoff.");
        Console.WriteLine($"{device.Kind}: {frames + 1} exact death frames with matching PCM, blackout/explosion/fade and game-over handoff.");
    }
}
