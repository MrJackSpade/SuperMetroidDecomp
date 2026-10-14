using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Rendering.Direct3D11;
using SuperMetroid.Core.Spatial;

/// <summary>Checks D3D11 captures of the retail Ceres Ridley Mode 7 scene and retained packet replay.</summary>
internal static class RetailRidleyCaptureTests
{
    /// <summary>Compares native HUD/floor scenes across scale, tilt, and scroll, then replays saved packets after replacing live GPU resources.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native PPU geometry, native VRAM words")]
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var runtime = RepositoryInstallation.CreateRuntime(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.NmiHandler.RunNmi(0, true);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.CeresRidleyRoom, new Position16<RoomPixels>(0, 0));
        runtime.NmiHandler.RunNmi(0, true);
        var ridley = runtime.Enemies.RidleyEncounter.CeresRidley;
        var retained = new List<(RenderFrameSnapshot Packet, Rgba32[] Expected)>();
        foreach (ushort scale in new ushort[] { 128, 240, 256 })
        foreach (short tilt in new short[] { -33, 0, 33 })
        foreach (short scroll in new short[] { -19, 0, 19 })
        {
            (ridley ?? throw new InvalidOperationException("Verification requires ridley.")).Mode7Active = true;
            ridley.Mode7MatrixA = scale; ridley.Mode7MatrixD = scale;
            ridley.Mode7MatrixB = unchecked((ushort)tilt);
            ridley.Mode7MatrixC = unchecked((ushort)-tilt);
            ridley.Mode7Center = new Position16<Mode7Pixels>(128, 120);
            ridley.Mode7Offset = new Offset16<Mode7Pixels>(7, unchecked((short)scroll));
            // Room main's M7A..M7Y shadows reach the PPU at the accepted NMI (#1307).
            runtime.NmiHandler.RunNmi(0, true);
            var expected = SuperMetroidRuntimeFrameRenderer.Render(runtime.PublishDisplayFrame()).ToArray();
            var scene = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
            if (scene.Layers[0] is not Mode7GameplayRenderLayer { Floor: not null } mixed ||
                mixed.HudScanlines != SnesPpuLayout.GameplayHudHeightPixels)
                throw new InvalidOperationException("Retail Ridley fixture did not capture mixed Mode-7 floor and HUD.");
            var packet = new RenderFrameSnapshot(new(retained.Count + 1, 1, 0), scene);
            packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotWriter.Serialize(packet));
            PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                $"{device.Kind}: retail Ridley scale={scale}, tilt={tilt}, scroll={scroll}");
            retained.Add((packet, expected));
        }
        (ridley ?? throw new InvalidOperationException("Verification requires ridley.")).Mode7Active = false;
        runtime.Vram.LoadBytes(0, new byte[SnesPpuLayout.VramByteCount]);
        for (int color = 0; color < SnesPpuLayout.CgramColorCount; color++) runtime.Cgram.SetColor(color, new Bgr555(0, 0, 0));
        foreach (var sample in retained.AsEnumerable().Reverse())
            PixelComparison.Verify(sample.Packet, sample.Expected, renderer.RenderForReadback(sample.Packet),
                $"{device.Kind}: retained Ridley {sample.Packet.Identity.Sequence}");
        Console.WriteLine($"{device.Kind}: {retained.Count} retail Ridley mixed-mode captures and retained replays match exactly.");
        RetailRidleyEscapeTests.Run(device, renderer);
    }
}
