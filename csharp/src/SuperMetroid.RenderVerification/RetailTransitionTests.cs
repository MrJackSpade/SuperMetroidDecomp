using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class RetailTransitionTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var destruction = new CeresDestructionCinematicState(bus);
        var phases = new HashSet<CeresDestructionPhase>();
        bool mode1 = false, mode7 = false;
        int cinematicCount = 0, menuCount = 0;
        for (int tick = 0; tick < 2000 && !destruction.Finished; tick++)
        {
            bool changed = phases.Add(destruction.Phase);
            if (changed || tick % 13 == 0)
            {
                var expected = destruction.Render();
                var scene = destruction.CaptureRenderSnapshot();
                mode1 |= scene.Layers.ToArray().Any(layer => layer is Bg4BppRenderLayer);
                mode7 |= scene.Layers.ToArray().Any(layer => layer is Mode7RenderLayer);
                var packet = new RenderFrameSnapshot(new(++cinematicCount, 1, (ushort)tick), scene);
                Check(packet, expected, $"Ceres/Zebes {destruction.Phase}, tick {tick}");
                destruction.Step();
                Check(packet, expected, $"retained Ceres/Zebes tick {tick}");
            }
            else destruction.Step();
        }
        if (!destruction.Finished || !mode1 || !mode7)
            throw new InvalidOperationException("Ceres/Zebes coverage did not finish with both modes.");
        foreach (bool chooseNo in new[] { false, true })
        {
            var audio = new CartridgeAudioState();
            var menu = new GameOverMenuState(bus, audio);
            bool toggled = false, released = false;
            for (int tick = 0; tick < 300 && !menu.ContinueRequested && !menu.TitleRequested; tick++)
            {
                var expected = menu.Render();
                var packet = new RenderFrameSnapshot(new(++menuCount, 2, (ushort)tick), menu.CaptureRenderSnapshot());
                Check(packet, expected, $"game-over {menu.Phase}, no={chooseNo}, tick {tick}");
                ushort input = 0;
                if (menu.Phase == GameOverMenuPhase.Main)
                {
                    if (chooseNo && !toggled) { input = (ushort)SnesButton.Down; toggled = true; }
                    else if (!released) released = true;
                    else input = (ushort)SnesButton.A;
                }
                menu.Step(input); audio.AdvanceFrame(bus, default);
                Check(packet, expected, $"retained game-over tick {tick}");
            }
            if (menu.TitleRequested != chooseNo || menu.ContinueRequested == chooseNo)
                throw new InvalidOperationException("Game-over fixture did not reach selected outcome.");
        }
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {cinematicCount} Ceres/Zebes samples across {phases.Count} phases and {menuCount} game-over frames match.");

        void Check(RenderFrameSnapshot packet, Rgba32[] expected, string context) =>
            PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), $"{device.Kind}: {context}");
    }
}
