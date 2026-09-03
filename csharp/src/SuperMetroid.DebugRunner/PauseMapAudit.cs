using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using System.Reflection;

internal static class PauseMapAudit
{
    public static int Run(string romPath, string outputPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var samus = new SamusState
        {
            XPosition = 0x0080,
            YPosition = 0x0080,
        };
        var system = new Bank80SystemState();
        system.SetAreaMapAcquired(0);
        var pause = new PauseMenuState(
            bus,
            samus,
            system,
            areaIndex: AreaId.Crateria,
            roomMapX: 17,
            roomMapY: 9);
        Rgba32[] pixels = pause.Render();
        PngWriter.WriteRgba(outputPath, FrontendFrame.Width, FrontendFrame.Height, pixels);
        var vram = (SnesVram)typeof(PauseMenuState)
            .GetField("vram", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(pause)!;
        var cgram = (SnesCgram)typeof(PauseMenuState)
            .GetField("cgram", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(pause)!;
        Rgba32[] bg1 = SnesBgTilemapRenderer.Render4BppViewport(
            vram, cgram, 0x3000, 0, pause.MapHorizontalScroll, pause.MapVerticalScroll,
            256, 224, 64, 32);
        Rgba32[] bg2 = SnesBgTilemapRenderer.Render4BppViewport(
            vram, cgram, 0x3800, 0, 0, 0, 256, 224, 32, 32);
        PngWriter.WriteRgba(Path.ChangeExtension(outputPath, ".bg1.png"), 256, 224, bg1);
        PngWriter.WriteRgba(Path.ChangeExtension(outputPath, ".bg2.png"), 256, 224, bg2);
        Console.WriteLine($"Wrote pause-map audit frame to {Path.GetFullPath(outputPath)}.");
        return 0;
    }
}
