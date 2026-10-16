using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Rendering.RenderPacketCodecTestsVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Runtime.Attract;

/// <summary>Checks captured attract-demo frames against legacy rendering, audio, and state transitions.</summary>
internal static class AttractCaptureTestsVerification
{
    /// <summary>Compares demo playback, held final image, cancellation, and return-to-title packets with the live frontend.</summary>
    internal static void VerifyAttractCapture()
    {
        byte[] rom = File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc"));
        AreaMapPresentationCatalog presentation = RetailPresentationFixture();
        // Scene setup is compiled now. Bound only the live verification instance after
        // its real first scene has loaded; ROM table patches no longer affect admission.
        int samples = 0;
        foreach (bool cancel in new[] { false, true })
        {
            var legacy = CreateRetailGameFixture(new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom));
            var captured = CreateRetailGameFixture(new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom));
            legacy.BindArtwork(legacy.Artwork with
            {
                MapPresentation = presentation,
            });
            captured.BindArtwork(captured.Artwork with
            {
                MapPresentation = presentation,
            });
            var states = new HashSet<SuperMetroidGameState>();
            bool sawHold = false, returned = false, bounded = false;
            Rgba32[]? held = null;
            for (int tick = 0; tick < 8000; tick++)
            {
                if (!bounded && legacy.GameState == SuperMetroidGameState.TransitionToDemoB)
                {
                    foreach (var game in new[] { legacy, captured })
                    {
                        game.Session.demoFramesRemaining = 16;
                        game.Session.demoScene = 6;
                    }
                    bounded = true;
                }
                SnesButton input = cancel && legacy.AttractDemoHoldFramesRemaining == 80 ? SnesButton.Start : (ushort)0;
                FrontendFrame expected = legacy.Step(input);
                CapturedFrontendFrame actual = captured.StepCaptured(input, tick + 1, 1);
                states.Add(expected.GameState);
                AssertEqual(expected.GameState, actual.Frame.GameState, "demo capture state");
                AssertSequenceEqual(legacy.AudioEventsForVerification, captured.AudioEventsForVerification, "demo capture audio event order");
                bool demo = expected.GameState is SuperMetroidGameState.TransitionToDemoA or SuperMetroidGameState.TransitionToDemoB
                    or SuperMetroidGameState.PlayingDemo or SuperMetroidGameState.TransitionFromDemoA or SuperMetroidGameState.TransitionFromDemoB;
                if (demo || tick % 31 == 0)
                {
                    AssertTrue(legacy.PublishedPixels().SequenceEqual(RenderForComparison(RoundTripRenderPacket(actual.Snapshot))),
                        $"demo frame {tick} pixel parity");
                    samples++;
                }
                if (legacy.AttractDemoHoldFramesRemaining > 0 && expected.GameState == SuperMetroidGameState.PlayingDemo)
                {
                    sawHold = true;
                    held ??= legacy.PublishedPixels();
                    AssertTrue(held.SequenceEqual(legacy.PublishedPixels()), "demo final-image hold stays pixel-identical");
                }
                if (states.Contains(SuperMetroidGameState.TransitionFromDemoB) && expected.GameState == SuperMetroidGameState.OpeningCinematic)
                {
                    AssertTrue(legacy.PublishedPixels().SequenceEqual(RenderForComparison(actual.Snapshot)), "demo return title frame");
                    returned = true; break;
                }
            }
            AssertTrue(sawHold && returned && states.Contains(SuperMetroidGameState.TransitionToDemoB)
                && states.Contains(SuperMetroidGameState.TransitionFromDemoA), "demo fixture exercises both black transitions and returns to title");
        }
        Console.WriteLine($"  Attract capture: {samples} sampled frames cover load, playback, retained hold, completion and cancellation.");
    }
}
