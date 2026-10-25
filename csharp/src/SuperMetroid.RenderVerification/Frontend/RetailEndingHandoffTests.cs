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
        var options = new SuperMetroidGameOptions { SkipOpeningCinematic = true };
        var legacy = InstalledFixtures.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore(), options);
        var captured = InstalledFixtures.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore(), options);
        var audioAssets = DesktopAccess.LoadAudioClips();
        legacy.BindAudio(audioAssets);
        captured.BindAudio(audioAssets);
        var leftAudio = new short[AudioSession.PcmSamplesPerFrame];
        var rightAudio = new short[AudioSession.PcmSamplesPerFrame];
        long sequence = 0;
        void Step(ControllerButton input, bool pixels)
        {
            var expected = legacy.Step(input);
            var actual = captured.StepCaptured(input, ++sequence, 1);
            legacy.CopyAudioFrame(leftAudio);
            captured.CopyAudioFrame(rightAudio);
            if (expected.GameState != actual.Frame.GameState ||
                expected.Phase != actual.Frame.Phase || expected.FrameNumber != actual.Frame.FrameNumber ||
                !legacy.AudioEventsForVerification.SequenceEqual(captured.AudioEventsForVerification) ||
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
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite);
            var top = runtime.Enemies.SlotTable.Slots.First(slot =>
                slot.DefinitionId == EnemyDefinitionId.ShipTop);
            // Stage the existing accelerating integrator near its native completion
            // threshold. It—not the fixture—publishes the frontend escape event.
            GunshipPartState.Of(top).Function = GunshipFunction.AcceleratingLiftoff;
            // The staged hull starts outside this fixture's camera. Explicitly keep
            // its AI active; this is handoff coverage, not full departure validation.
            top.Properties = top.Properties.With(EnemyProperties.ProcessOffScreen);
            runtime.Player.IntroducedSamus.Control.LockHandlers();
            runtime.Player.IntroducedSamus.Position = runtime.Player.IntroducedSamus.Position.WithWholeY(EndingHandoffFixtureDefinitions.TakeoffBoundaryY);
            runtime.FrameBoundary.BeginAcceptedFrame(0);
        }
        bool sawFade = false;
        int endingFrames = 0, frames = 0;
        for (; frames < 256; frames++)
        {
            Step(0, true);
            sawFade |= legacy.GameState == SuperMetroidGameState.SamusEscapesFromZebes;
            if (legacy.GameState == SuperMetroidGameState.EndingAndCredits && ++endingFrames == 60) break;
        }
        if (!sawFade || endingFrames != 60) throw new InvalidOperationException($"Ending fixture missed native escape fade/credits handoff: state={legacy.GameState}, fade={sawFade}, ending={endingFrames}, health={legacy.GameplayRuntimeForVerification.Player.IntroducedSamus.Health}, event={legacy.GameplayRuntimeForVerification.Enemies.LastEnemyFrame.Gunship}.");
        Console.WriteLine($"{device.Kind}: {frames + 1} exact gunship-event/fade/ending frames with matching PCM and frontend state.");
    }
}
