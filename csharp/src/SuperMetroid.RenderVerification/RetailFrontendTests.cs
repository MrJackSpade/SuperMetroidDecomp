using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class RetailFrontendTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        // Required fixture, not an optional skip. SRAM is private to these address
        // spaces; no player save files are loaded or modified.
        byte[] rom = File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc"));
        var title = new TitleSequenceState(new SuperMetroidAddressSpace(rom));
        var phases = new HashSet<TitleSequencePhase>();
        int titleCount = 0;
        for (int tick = 0; tick < 2500 && !title.FileSelectRequested; tick++)
        {
            bool newPhase = phases.Add(title.Phase);
            if (newPhase || tick % 17 == 0 || title.Phase == TitleSequencePhase.TitleScreenFadeOut)
            {
                var expected = title.Render();
                var packet = new RenderFrameSnapshot(new(tick + 1, 1, (ushort)tick), title.CaptureRenderSnapshot());
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: retail title {title.Phase}, tick {tick}");
                titleCount++;
                title.Step(title.Phase == TitleSequencePhase.TitleScreen ? (ushort)SnesButton.Start : (ushort)0);
            }
            else title.Step(0);
        }
        if (!title.FileSelectRequested || !phases.Contains(TitleSequencePhase.SceneThreeZoom))
            throw new InvalidOperationException("Retail title did not cover natural zoom and reach file select.");

        var legacy = new SuperMetroidGame(new SuperMetroidAddressSpace(rom));
        var captured = new SuperMetroidGame(new SuperMetroidAddressSpace(rom));
        var states = new HashSet<SuperMetroidGameState>();
        for (int tick = 0; tick < 500; tick++)
        {
            ushort input = tick % 47 == 0 ? (ushort)SnesButton.Start : (ushort)0;
            var expected = legacy.Step(input);
            var actual = captured.StepCaptured(input, tick + 1, 2);
            if (expected.GameState != actual.Frame.GameState || expected.Phase != actual.Frame.Phase ||
                expected.FrameNumber != actual.Frame.FrameNumber ||
                !expected.AudioCommands.SequenceEqual(actual.Frame.AudioCommands))
                throw new InvalidOperationException($"Frontend state/audio diverged at tick {tick}.");
            states.Add(expected.GameState);
            var packet = actual.Snapshot ?? throw new InvalidOperationException($"Legacy raster fallback at {expected.Phase}.");
            if (actual.Frame.Pixels.Length != 0) throw new InvalidOperationException("Capture emitted CPU raster.");
            PixelComparison.Verify(packet, expected.Pixels, renderer.RenderForReadback(packet),
                $"{device.Kind}: retail frontend {expected.Phase}, tick {tick}");
            if (expected.GameState == SuperMetroidGameState.IntroCinematic)
            {
                if (!states.Contains(SuperMetroidGameState.FileSelectMenus) || !states.Contains(SuperMetroidGameState.GameOptionsMenu))
                    throw new InvalidOperationException("Retail frontend did not visit file/options menus.");
                Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {titleCount} natural title and {tick + 1} frontend frames match legacy pixels; frontend state/audio commands agree.");
                return;
            }
        }
        throw new InvalidOperationException("Retail frontend did not reach intro handoff.");
    }
}
