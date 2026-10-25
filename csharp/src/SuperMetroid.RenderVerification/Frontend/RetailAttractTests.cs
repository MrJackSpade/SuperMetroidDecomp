using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Frontend;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks D3D11 capture of the stock attract-demo sequence and its return to the title.</summary>
internal static class RetailAttractTests
{
    /// <summary>Compares captured pixels, state, phase, and audio through demo completion or cancellation, including each scene's frozen final image.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        // Attract scenes are compiled stock definitions, so the fixture tours the stock first
        // demo set; a cartridge edit can no longer shorten it.
        int samples = 0;
        foreach (bool cancel in new[] { false, true })
        {
            var legacy = InstalledFixtures.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore());
            var captured = InstalledFixtures.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore());
            var states = new HashSet<SuperMetroidGameState>();
            bool sawHold = false, returned = false;
            Rgba32[]? held = null;
            for (int tick = 0; tick < 8000; tick++)
            {
                SnesButton input = cancel && legacy.AttractDemoHoldFramesRemaining == 80 ? SnesButton.Start : SnesButton.None;
                var expected = legacy.Step(input);
                var actual = captured.StepCaptured(input, tick + 1, 1);
                states.Add(expected.GameState);
                if (expected.GameState != actual.Frame.GameState ||
                    expected.Phase != actual.Frame.Phase || !legacy.AudioEventsForVerification.SequenceEqual(captured.AudioEventsForVerification))
                    throw new InvalidOperationException($"Attract state/audio/capture divergence at {tick}.");
                bool demo = expected.GameState is SuperMetroidGameState.TransitionToDemoA or SuperMetroidGameState.TransitionToDemoB
                    or SuperMetroidGameState.PlayingDemo or SuperMetroidGameState.TransitionFromDemoA or SuperMetroidGameState.TransitionFromDemoB;
                if (demo || tick % 31 == 0)
                {
                    var packet = actual.Snapshot;
                    PixelComparison.Verify(packet, legacy.PublishedPixels(), renderer.RenderForReadback(packet), $"{device.Kind}: attract tick={tick}, cancel={cancel}");
                    samples++;
                }
                if (legacy.AttractDemoHoldFramesRemaining > 0 && expected.GameState == SuperMetroidGameState.PlayingDemo)
                {
                    sawHold = true;
                    // Each scene's ninety-frame final-image hold freezes that scene's own image.
                    if (legacy.AttractDemoHoldFramesRemaining == AttractDemoDefinitions.FinalImageHoldFrames)
                        held = legacy.PublishedPixels();
                    held ??= legacy.PublishedPixels();
                    if (!held.SequenceEqual(legacy.PublishedPixels())) throw new InvalidOperationException("Demo held frame changed.");
                }
                if (states.Contains(SuperMetroidGameState.TransitionFromDemoB) && expected.GameState == SuperMetroidGameState.OpeningCinematic)
                {
                    PixelComparison.Verify(actual.Snapshot, legacy.PublishedPixels(), renderer.RenderForReadback(actual.Snapshot), "attract return to title");
                    returned = true; break;
                }
            }
            if (!sawHold || !returned || !states.Contains(SuperMetroidGameState.TransitionToDemoB) || !states.Contains(SuperMetroidGameState.TransitionFromDemoA))
                throw new InvalidOperationException("Attract fixture missed hold, transitions or return to title.");
        }
        Console.WriteLine($"{device.Kind}: {samples} exact attract samples; stock demo scenes, per-scene holds, completion/cancel and return to title passed.");
    }
}
