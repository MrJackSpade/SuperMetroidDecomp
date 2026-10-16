using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Frontend;

/// <summary>Checks captured frontend packets against the legacy raster through title and menu handoffs.</summary>
internal static class FrontendRenderCaptureTestsVerification
{
    /// <summary>Compares frame pixels, state, audio, and retained packet identity through the intro transition.</summary>
    internal static void VerifyFrontendRenderCapture()
    {
        byte[] rom = File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc"));
        AreaMapPresentationCatalog presentation = RetailPresentationFixture();
        var legacy = CreateRetailGameFixture(new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom), new SuperMetroid.Core.Saves.SaveSlotStore());
        var captured = CreateRetailGameFixture(new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom), new SuperMetroid.Core.Saves.SaveSlotStore());
        legacy.BindArtwork(legacy.Artwork with
        {
            MapPresentation = presentation,
        });
        captured.BindArtwork(captured.Artwork with
        {
            MapPresentation = presentation,
        });
        AssertEqual(new Rgba32(0, 0, 0, 255), captured.GetRetainedDisplay(1, 1).SolidColor,
            "the initial display is a published black frame");
        var states = new HashSet<SuperMetroidGameState>();
        int packetCount = 0;
        RenderFrameSnapshot? held = null;
        Rgba32[]? heldPixels = null;
        for (int tick = 0; tick < 500; tick++)
        {
            SnesButton input = tick % 47 == 0 ? SnesButton.Start : (ushort)0;
            FrontendFrame expected = legacy.Step(input);
            CapturedFrontendFrame actual = captured.StepCaptured(input, tick + 1, 7);
            AssertEqual(expected.GameState, actual.Frame.GameState, "captured frontend state cadence");
            AssertEqual(expected.Phase, actual.Frame.Phase, "captured frontend phase cadence");
            AssertEqual(expected.FrameNumber, actual.Frame.FrameNumber, "captured frontend NMI identity");
            AssertSequenceEqual(legacy.AudioEventsForVerification, captured.AudioEventsForVerification, "captured frontend audio event order");
            states.Add(expected.GameState);
            Rgba32[] actualPixels;
            RenderFrameSnapshot packet = actual.Snapshot;
            {
                long allocationStart = GC.GetAllocatedBytesForCurrentThread();
                var metadata = captured.CurrentFrame;
                long metadataBytes = GC.GetAllocatedBytesForCurrentThread() - allocationStart;
                AssertTrue(metadataBytes < FrontendFrame.Width * FrontendFrame.Height,
                    "metadata access must not allocate even a one-byte-per-pixel raster");
                AssertEqual(expected.GameState, metadata.GameState, "metadata state");
                AssertEqual(expected.Phase, metadata.Phase, "metadata phase");
                AssertEqual(expected.FrameNumber, metadata.FrameNumber, "metadata frame identity");
                AssertEqual(tick + 1L, packet.Identity.Sequence, "host sequence is not cartridge frame");
                AssertEqual(7L, packet.Identity.Generation, "host generation preserved");
                AssertEqual(expected.FrameNumber, packet.Identity.SimulationFrame, "snapshot tracks completed simulation call");
                actualPixels = SoftwareFrameSnapshotRenderer.Render(packet);
                // Republish twice, as a paused host may attach/resize or load a new
                // presentation generation. Neither operation may replay draw effects.
                RenderFrameSnapshot retained = captured.GetRetainedDisplay(10000 + tick, 8);
                RenderFrameSnapshot repeated = captured.GetRetainedDisplay(20000 + tick, 9);
                AssertEqual(10000L + tick, retained.Identity.Sequence, "retained display gets host sequence");
                AssertEqual(8L, retained.Identity.Generation, "retained display gets new generation");
                AssertEqual(expected.FrameNumber, retained.Identity.SimulationFrame, "republish retains simulation identity");
                AssertEqual(expected.FrameNumber, captured.FrameNumber, "republish does not step simulation");
                AssertEqual(7L, packet.Identity.Generation, "republish does not mutate in-flight identity");
                AssertTrue(actualPixels.SequenceEqual(RenderForComparison(retained)),
                    "republished display preserves exact pixels and outer fades");
                AssertTrue(actualPixels.SequenceEqual(RenderForComparison(repeated)),
                    "repeated republish preserves exact display");
                packetCount++;
                if (held is null) { held = packet; heldPixels = actualPixels; }
            }
            AssertTrue(legacy.PublishedPixels().SequenceEqual(actualPixels),
                $"frontend packet pixel parity at tick {tick}, {expected.Phase}");
            if (expected.GameState == SuperMetroidGameState.IntroCinematic)
            {
                AssertTrue(states.Contains(SuperMetroidGameState.FileSelectMenus), "capture path covers file menu");
                AssertTrue(states.Contains(SuperMetroidGameState.GameOptionsMenu), "capture path covers options");
                AssertTrue(held is not null && heldPixels is not null && heldPixels.SequenceEqual(
                    SoftwareFrameSnapshotRenderer.Render(held)), "retained title packet survives scene handoffs");
                // Switching back to the compatibility entry point must also work.
                FrontendFrame legacyNext = legacy.Step(0);
                FrontendFrame capturedNext = captured.Step(0);
                AssertTrue(legacy.PublishedPixels().SequenceEqual(captured.PublishedPixels()), "legacy Step after captured Step");
                Console.WriteLine($"  Frontend capture: {packetCount} packet frames preserve state/audio/pixels through title/file/options and intro handoff.");
                return;
            }
        }
        throw new InvalidOperationException("Frontend capture fixture did not reach the intro.");
    }
}
