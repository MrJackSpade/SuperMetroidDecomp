using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Frontend;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Compares the gunship escape event, fade, and ending handoff through captured rendering and audio.</summary>
internal static class RetailEndingHandoffTests
{
    /// <summary>Runs paired retail games through the native takeoff boundary and sixty ending frames.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        var options = new SuperMetroid.Core.Gameplay.GameplayAssists { SkipOpeningCinematic = true };
        var audioAssets = DesktopAccess.LoadAudioClips();
        var legacy = InstalledFixtures.CreateGameWithAudio(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), SuperMetroid.Core.Audio.GameAudioOutput.MixedFrom(audioAssets), options);
        var captured = InstalledFixtures.CreateGameWithAudio(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), SuperMetroid.Core.Audio.GameAudioOutput.MixedFrom(audioAssets), options);
        var leftAudio = new short[AudioSession.PcmSamplesPerFrame];
        var rightAudio = new short[AudioSession.PcmSamplesPerFrame];
        long sequence = 0;
        void Step(ControllerButton input, bool pixels)
        {
            var expected = legacy.Step(input);
            sequence++;
            var actual = captured.StepCaptured(input);
            legacy.CopyAudioFrame(leftAudio);
            captured.CopyAudioFrame(rightAudio);
            if (expected.GameState != actual.Frame.GameState ||
                expected.Phase != actual.Frame.Phase || expected.FrameNumber != actual.Frame.FrameNumber ||
                !expected.Audio.Events.SequenceEqual(actual.Frame.Audio.Events) ||
                !leftAudio.SequenceEqual(rightAudio))
                throw new InvalidOperationException("Ending handoff changed frontend cadence/audio.");
            if (pixels) PixelComparison.Verify(actual.Snapshot, legacy.PublishedPixels(),
                renderer.RenderForReadback(actual.Snapshot), $"{device.Kind}: ending handoff {sequence}, {expected.Phase}");
        }
        for (int tick = 0; tick < 2000 && legacy.GameState != SuperMetroidGameState.MainGameplay; tick++)
            Step(tick % 47 == 0 ? ControllerButton.Start : ControllerButton.None, false);
        if (legacy.GameState != SuperMetroidGameState.MainGameplay) throw new InvalidOperationException("Ending fixture startup failed.");
        foreach (var game in new[] { legacy, captured })
        {
            var runtime = game.GameplayRuntimeForVerification;
            runtime.LoadCartridgeRoomForDebug(RoomDefinitions.LandingSite);
            var top = runtime.Enemies.Places.InUpdateOrder.First(place =>
                place.Holds(EnemyDefinitionId.ShipTop));
            // Stage the existing accelerating integrator near its native completion
            // threshold. It—not the fixture—publishes the frontend escape event.
            top.RequireSpeciesState<GunshipState>().Phase = GunshipPhase.AcceleratingLiftoff;
            // The staged hull starts outside this fixture's camera. Explicitly keep
            // its AI active; this is handoff coverage, not full departure validation.
            top.Processing = top.Processing.With(EnemyProcessing.OffScreen);
            runtime.Player.Samus.Control.LockOutPlayer();
            runtime.Player.Samus.Position = runtime.Player.Samus.Position.WithWholeY(EndingHandoffFixtureDefinitions.TakeoffBoundaryY);
            runtime.FrameBoundary.BeginAcceptedFrame(0);
        }
        bool sawFade = false;
        int endingFrames = 0, frames = 0;
        for (; frames < 256; frames++)
        {
            Step(0, true);
            sawFade |= legacy.IsAt(EndingStep.LeavingZebes);
            if (legacy.IsAt(EndingStep.Credits) && ++endingFrames == 60) break;
        }
        if (!sawFade || endingFrames != 60) throw new InvalidOperationException($"Ending fixture missed native escape fade/credits handoff: state={legacy.GameState}, fade={sawFade}, ending={endingFrames}, health={legacy.GameplayRuntimeForVerification.Player.Samus.Health}, event={string.Join(",", legacy.GameplayRuntimeForVerification.Enemies.LastEnemyFrame.All<GunshipEvent>())}.");
        Console.WriteLine($"{device.Kind}: {frames + 1} exact gunship-event/fade/ending frames with matching PCM and frontend state.");
    }
}
