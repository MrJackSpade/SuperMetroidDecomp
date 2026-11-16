using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Frontend;

/// <summary>Checks captured frontend packets against the legacy raster through title and menu handoffs.</summary>
internal static class FrontendRenderCaptureTestsVerification
{
    /// <summary>Compares frame pixels, state, audio, and retained packet identity through the intro transition.</summary>
    internal static void VerifyFrontendRenderCapture()
    {
        AreaMapPresentation presentation = RetailPresentationFixture();
        var legacy = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore());
        var captured = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore());
        legacy.BindArtwork(legacy.Artwork with
        {
            MapPresentation = presentation,
        });
        captured.BindArtwork(captured.Artwork with
        {
            MapPresentation = presentation,
        });
        AssertEqual(new RgbaPixel(0, 0, 0, 255), captured.GetRetainedDisplay().SolidColor,
            "the initial display is a published black frame");
        var states = new HashSet<SuperMetroidGameState>();
        int packetCount = 0;
        DisplayPacket? held = null;
        byte[] heldDigest = [];
        for (int tick = 0; tick < 500; tick++)
        {
            ControllerButton input = tick % 47 == 0 ? ControllerButton.Start : 0;
            FrontendFrame expected = legacy.Step(input);
            CapturedFrontendFrame actual = captured.StepCaptured(input);
            AssertEqual(expected.GameState, actual.Frame.GameState, "captured frontend state cadence");
            AssertEqual(expected.Phase, actual.Frame.Phase, "captured frontend phase cadence");
            AssertEqual(expected.FrameNumber, actual.Frame.FrameNumber, "captured frontend NMI identity");
            AssertSequenceEqual(expected.Audio.Events, actual.Frame.Audio.Events, "captured frontend audio event order");
            states.Add(expected.GameState);
            DisplayPacket packet = actual.Snapshot;
            {
                long allocationStart = GC.GetAllocatedBytesForCurrentThread();
                var metadata = captured.CurrentFrame;
                long metadataBytes = GC.GetAllocatedBytesForCurrentThread() - allocationStart;
                AssertTrue(metadataBytes < SuperMetroid.Core.Spatial.ScreenGeometry.ScreenWidthPixels * FrontendFrame.Height,
                    "metadata access must not allocate even a one-byte-per-pixel raster");
                AssertEqual(expected.GameState, metadata.GameState, "metadata state");
                AssertEqual(expected.Phase, metadata.Phase, "metadata phase");
                AssertEqual(expected.FrameNumber, metadata.FrameNumber, "metadata frame identity");
                AssertEqual(expected.FrameNumber, packet.FrameNumber, "snapshot carries the completed update's frame number");
                // Republish twice, as a paused host may attach a presenter or resize it. Neither may replay draw effects.
                DisplayPacket retained = captured.GetRetainedDisplay();
                DisplayPacket repeated = captured.GetRetainedDisplay();
                AssertEqual(expected.FrameNumber, retained.FrameNumber, "republish keeps the frame number");
                AssertEqual(expected.FrameNumber, captured.FrameNumber, "republish does not step simulation");
                AssertTrue(ScenePixels.Same(packet, retained),
                    "republished display preserves exact pixels and outer fades");
                AssertTrue(ScenePixels.Same(packet, repeated),
                    "repeated republish preserves exact display");
                packetCount++;
                // The held packet's whole description, every value it reaches through owned or shared state, is
                // digested when captured and again at the intro handoff (#1532).
                if (held is null) { held = packet; heldDigest = GraphDigest(packet); }
            }
            AssertTrue(ScenePixels.Same(legacy.GetRetainedDisplay(), packet),
                $"frontend packet pixel parity at tick {tick}, {expected.Phase}");
            if (expected.GameState == SuperMetroidGameState.IntroCinematic)
            {
                AssertTrue(states.Contains(SuperMetroidGameState.FileSelectMenus), "capture path covers file menu");
                AssertTrue(states.Contains(SuperMetroidGameState.GameOptionsMenu), "capture path covers options");
                AssertTrue(held is not null && heldDigest.AsSpan().SequenceEqual(GraphDigest(held)),
                    "retained title packet survives scene handoffs");
                // Switching back to the compatibility entry point must also work.
                legacy.Step(0);
                captured.Step(0);
                AssertTrue(ScenePixels.Same(legacy.GetRetainedDisplay(), captured.GetRetainedDisplay()), "legacy Step after captured Step");
                Console.WriteLine($"  Frontend capture: {packetCount} packet frames preserve state/audio/pixels through title/file/options and intro handoff.");
                return;
            }
        }
        throw new InvalidOperationException("Frontend capture fixture did not reach the intro.");
    }
}
