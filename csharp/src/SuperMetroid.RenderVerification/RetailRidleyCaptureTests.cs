using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Rendering.Direct3D11;

internal static class RetailRidleyCaptureTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var runtime = new SuperMetroidRuntime(SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.CeresRidleyRoom, 0, 0);
        runtime.RunNmi(0, true);
        var ridley = runtime.Enemies.CeresRidley!;
        var retained = new List<(RenderFrameSnapshot Packet, Rgba32[] Expected)>();
        // Retail PPU memory with controlled escape registers, not a simulated battle.
        // Vary signed rotation/scale and scroll while retaining the native floor/HUD.
        foreach (ushort scale in new ushort[] { 128, 240, 256 })
        foreach (short tilt in new short[] { -33, 0, 33 })
        foreach (short scroll in new short[] { -19, 0, 19 })
        {
            ridley.Mode7Active = true;
            ridley.Mode7MatrixA = scale; ridley.Mode7MatrixD = scale;
            ridley.Mode7MatrixB = unchecked((ushort)tilt);
            ridley.Mode7MatrixC = unchecked((ushort)-tilt);
            ridley.Mode7CenterX = 128; ridley.Mode7CenterY = 120;
            ridley.Mode7HorizontalOffset = 7;
            ridley.Mode7VerticalOffset = unchecked((ushort)scroll);
            var expected = SuperMetroidRuntimeFrameRenderer.Render(runtime);
            var scene = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
            if (scene.Layers[0] is not Mode7GameplayRenderLayer { Floor: not null } mixed ||
                mixed.HudScanlines != SnesPpuLayout.GameplayHudHeightPixels)
                throw new InvalidOperationException("Retail Ridley fixture did not capture mixed Mode-7 floor and HUD.");
            var packet = new RenderFrameSnapshot(new(retained.Count + 1, 1, 0), scene);
            packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
            PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                $"{device.Kind}: retail Ridley scale={scale}, tilt={tilt}, scroll={scroll}");
            retained.Add((packet, expected));
        }
        ridley.Mode7Active = false;
        runtime.Vram.LoadBytes(0, new byte[SnesPpuLayout.VramByteCount]);
        for (int color = 0; color < SnesPpuLayout.CgramColorCount; color++) runtime.Cgram.SetColor(color, 0);
        // Replay in reverse order so every check also overwrites GPU resources from
        // another packet. No borrowed room memory or previous output may satisfy it.
        foreach (var sample in retained.AsEnumerable().Reverse())
            PixelComparison.Verify(sample.Packet, sample.Expected, renderer.RenderForReadback(sample.Packet),
                $"{device.Kind}: retained Ridley {sample.Packet.Identity.Sequence}");
        Console.WriteLine($"{device.Kind}: {retained.Count} retail Ridley mixed-mode captures and retained replays match exactly.");
        RetailRidleyEscapeTests.Run(device, renderer);
    }
}
