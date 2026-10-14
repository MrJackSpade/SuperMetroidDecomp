using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using System.Reflection;
using static GraphDigestVerification;
using static RenderPacketCodecTestsVerification;
using static RetailGameFixtureVerification;
using static RetailPresentationFixtureVerification;
using static VerificationHelpersVerification;

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
            var legacy = CreateRetailGameFixture(new SuperMetroid.AssetExtraction.CartridgeImportAddressSpace(rom));
            var captured = CreateRetailGameFixture(new SuperMetroid.AssetExtraction.CartridgeImportAddressSpace(rom));
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
                        game.demoFramesRemaining = 16;
                        game.demoScene = 6;
                    }
                    bounded = true;
                }
                ushort input = cancel && legacy.AttractDemoHoldFramesRemaining == 80 ? (ushort)SnesButton.Start : (ushort)0;
                FrontendFrame expected = legacy.Step(input);
                CapturedFrontendFrame actual = captured.StepCaptured(input, tick + 1, 1);
                states.Add(expected.GameState);
                AssertTrue(!actual.UsedLegacyRaster, $"demo capture never uses legacy pixels: tick {tick}, {expected.GameState}");
                AssertEqual(expected.GameState, actual.Frame.GameState, "demo capture state");
                AssertSequenceEqual(expected.AudioEvents, actual.Frame.AudioEvents, "demo capture audio event order");
                bool demo = expected.GameState is SuperMetroidGameState.TransitionToDemoA or SuperMetroidGameState.TransitionToDemoB
                    or SuperMetroidGameState.PlayingDemo or SuperMetroidGameState.TransitionFromDemoA or SuperMetroidGameState.TransitionFromDemoB;
                if (demo || tick % 31 == 0)
                {
                    AssertTrue(expected.Pixels.SequenceEqual(RenderForComparison(RoundTripRenderPacket((actual.Snapshot ?? throw new InvalidOperationException("Verification requires actual.Snapshot."))))),
                        $"demo frame {tick} pixel parity");
                    samples++;
                }
                if (legacy.AttractDemoHoldFramesRemaining > 0 && expected.GameState == SuperMetroidGameState.PlayingDemo)
                {
                    sawHold = true;
                    held ??= expected.Pixels.ToArray();
                    AssertTrue(held.SequenceEqual(expected.Pixels), "demo final-image hold stays pixel-identical");
                }
                if (states.Contains(SuperMetroidGameState.TransitionFromDemoB) && expected.GameState == SuperMetroidGameState.OpeningCinematic)
                {
                    AssertTrue(expected.Pixels.SequenceEqual(RenderForComparison((actual.Snapshot ?? throw new InvalidOperationException("Verification requires actual.Snapshot.")))), "demo return title frame");
                    returned = true; break;
                }
            }
            AssertTrue(sawHold && returned && states.Contains(SuperMetroidGameState.TransitionToDemoB)
                && states.Contains(SuperMetroidGameState.TransitionFromDemoA), "demo fixture exercises both black transitions and returns to title");
        }
        Console.WriteLine($"  Attract capture: {samples} sampled frames cover load, playback, retained hold, completion and cancellation.");
    }
}
