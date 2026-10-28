using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.RenderVerification.Harness;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Rooms;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks D3D11 rendering across Ceres destruction, Zebes transition, and game-over menu outcomes.</summary>
internal static class RetailTransitionTests
{
    /// <summary>Compares live and retained cinematic packets across Mode 1 and Mode 7, then checks Yes and No game-over paths.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var destruction = RenderRetailFixture.CreateDestruction();
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
                mode1 |= scene.Layers.Any(layer => layer is ScreenTilesLayer);
                mode7 |= scene.Layers.Any(layer => layer is TurningFloorLayer);
                var packet = new DisplayPacket(new(++cinematicCount, 1, (ushort)tick), scene);
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
            var audio = new AudioQueues();
            var menu = new GameOverMenuState(audio, RenderRetailFixture.Maps, MenuSheetFixtures.For(RenderRetailFixture.Maps));
            bool toggled = false, released = false;
            for (int tick = 0; tick < 300 && !menu.ContinueRequested && !menu.TitleRequested; tick++)
            {
                var expected = menu.Render();
                var packet = new DisplayPacket(new(++menuCount, 2, (ushort)tick), menu.CaptureRenderSnapshot());
                Check(packet, expected, $"game-over {menu.Phase}, no={chooseNo}, tick {tick}");
                ControllerButton input = 0;
                if (menu.Phase == GameOverMenuPhase.Main)
                {
                    if (chooseNo && !toggled) { input = ControllerButton.Down; toggled = true; }
                    else if (!released) released = true;
                    else input = ControllerButton.A;
                }
                menu.Step(input); audio.AdvanceFrame(default);
                Check(packet, expected, $"retained game-over tick {tick}");
            }
            if (menu.TitleRequested != chooseNo || menu.ContinueRequested == chooseNo)
                throw new InvalidOperationException("Game-over fixture did not reach selected outcome.");
        }
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {cinematicCount} Ceres/Zebes samples across {phases.Count} phases and {menuCount} game-over frames match.");

        void Check(DisplayPacket packet, RgbaPixel[] expected, string context) =>
            PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), $"{device.Kind}: {context}");
    }
}
