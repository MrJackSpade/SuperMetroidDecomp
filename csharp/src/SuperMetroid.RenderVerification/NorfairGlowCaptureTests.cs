using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Rendering.Direct3D11;

internal static class NorfairGlowCaptureTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(NorfairGlowFixture.Cathedral, 0, 0);
        runtime.Samus!.InputLocked = true;
        LayeredRenderSnapshot? original = null;
        SuperMetroid.Core.Assets.Rgba32[]? initialPixels = null;
        int maximumChanged = 0;
        for (int tick = 0; tick < 232; tick++)
        {
            runtime.StepFrame(0);
            var scene = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
            original ??= scene;
            var packet = new RenderFrameSnapshot(new(tick + 1, 1, (ushort)tick), scene);
            PixelComparison.Verify(packet, SuperMetroidRuntimeFrameRenderer.Render(runtime),
                renderer.RenderForReadback(packet), $"{device.Kind}: Norfair glow frame {tick}");

            // Freeze geometry, camera, objects, and every unrelated palette entry.
            // Any remaining pixel change is specifically the room's foreground glow,
            // not a moving enemy, liquid surface, or BG2 heat distortion.
            ushort[] colors = original.Memory.Cgram.ToArray();
            foreach (int index in NorfairGlowFixture.ColorIndices)
                colors[index] = scene.Memory.Cgram[index];
            var memory = new PpuMemorySnapshot(original.Memory.Vram, colors, original.Memory.Oam);
            var registers = ((OrdinaryGameplayRenderLayer)original.Layers[0]).Registers;
            var foreground = new LayeredRenderSnapshot(memory,
                [new OrdinaryGameplayRenderLayer(registers with { MainScreenLayers = SnesMainScreenLayers.Bg1 })],
                original.ObjectSelection, original.Brightness);
            var isolated = new RenderFrameSnapshot(new(tick + 1, 2, (ushort)tick), foreground);
            var pixels = SoftwareFrameSnapshotRenderer.Render(isolated);
            PixelComparison.Verify(isolated, pixels, renderer.RenderForReadback(isolated),
                $"{device.Kind}: isolated Norfair foreground {tick}");
            initialPixels ??= pixels;
            int changed = 0;
            for (int pixel = 256 * 32; pixel < pixels.Length; pixel++)
                if (pixels[pixel] != initialPixels[pixel]) changed++;
            maximumChanged = Math.Max(maximumChanged, changed);
        }
        if (maximumChanged == 0)
            throw new InvalidOperationException("Norfair palette animation never changes a visible BG1 foreground pixel.");
        Console.WriteLine($"{device.Kind}: 232 Norfair runtime/GPU frames agree; isolated glow changes {maximumChanged} foreground pixels.");
    }
}

internal static class NorfairGlowFixture
{
    /// <summary>$8F:A788 Cathedral, a heated Norfair room with glowing foreground rock.</summary>
    internal const ushort Cathedral = 0xa788;
    /// <summary>CGRAM word destinations written by $8D:F08E/F1D1/F2D9/F3E1, including inline skips.</summary>
    internal static ReadOnlySpan<int> ColorIndices =>
        [53, 54, 55, 60, 61, 65, 66, 67, 76, 77, 81, 82, 83, 92, 93, 97, 98, 99, 108, 109];
}
