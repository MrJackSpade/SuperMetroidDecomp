using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyFrontendRenderCapture()
    {
        byte[] rom = File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc"));
        var legacy = new SuperMetroidGame(new SuperMetroidAddressSpace(rom));
        var captured = new SuperMetroidGame(new SuperMetroidAddressSpace(rom));
        var states = new HashSet<SuperMetroidGameState>();
        int packetCount = 0;
        RenderFrameSnapshot? held = null;
        Rgba32[]? heldPixels = null;
        for (int tick = 0; tick < 500; tick++)
        {
            ushort input = tick % 47 == 0 ? (ushort)SnesButton.Start : (ushort)0;
            FrontendFrame expected = legacy.Step(input);
            CapturedFrontendFrame actual = captured.StepCaptured(input, tick + 1, 7);
            AssertEqual(expected.GameState, actual.Frame.GameState, "captured frontend state cadence");
            AssertEqual(expected.Phase, actual.Frame.Phase, "captured frontend phase cadence");
            AssertEqual(expected.FrameNumber, actual.Frame.FrameNumber, "captured frontend NMI identity");
            AssertSequenceEqual(expected.AudioCommands, actual.Frame.AudioCommands, "captured frontend audio command order");
            states.Add(expected.GameState);
            Rgba32[] actualPixels;
            if (actual.Snapshot is { } packet)
            {
                AssertTrue(!actual.UsedLegacyRaster, "converted frame reports packet output");
                AssertEqual(0, actual.Frame.Pixels.Length, "captured Step returns no raster buffer");
                AssertEqual(tick + 1L, packet.Identity.Sequence, "host sequence is not cartridge frame");
                AssertEqual(7L, packet.Identity.Generation, "host generation preserved");
                AssertEqual(expected.FrameNumber, packet.Identity.SimulationFrame, "snapshot tracks completed simulation call");
                actualPixels = SoftwareFrameSnapshotRenderer.Render(packet);
                packetCount++;
                if (held is null) { held = packet; heldPixels = actualPixels; }
            }
            else
            {
                AssertTrue(actual.UsedLegacyRaster, "unconverted scene explicitly reports raster fallback");
                actualPixels = actual.Frame.Pixels;
            }
            AssertTrue(expected.Pixels.AsSpan().SequenceEqual(actualPixels),
                $"frontend packet pixel parity at tick {tick}, {expected.Phase}");
            if (expected.GameState == SuperMetroidGameState.IntroCinematic)
            {
                AssertTrue(!actual.UsedLegacyRaster, "intro publishes captured display data");
                AssertTrue(states.Contains(SuperMetroidGameState.FileSelectMenus), "capture path covers file menu");
                AssertTrue(states.Contains(SuperMetroidGameState.GameOptionsMenu), "capture path covers options");
                AssertTrue(held is not null && heldPixels is not null && heldPixels.AsSpan().SequenceEqual(
                    SoftwareFrameSnapshotRenderer.Render(held)), "retained title packet survives scene handoffs");
                // Switching back to the compatibility entry point must also work.
                FrontendFrame legacyNext = legacy.Step(0);
                FrontendFrame capturedNext = captured.Step(0);
                AssertTrue(legacyNext.Pixels.AsSpan().SequenceEqual(capturedNext.Pixels), "legacy Step after captured Step");
                Console.WriteLine($"  Frontend capture: {packetCount} packet frames preserve state/audio/pixels through title/file/options and intro handoff.");
                return;
            }
        }
        throw new InvalidOperationException("Frontend capture fixture did not reach the intro.");
    }
}
