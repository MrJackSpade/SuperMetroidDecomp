using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Hud;
using SuperMetroid.RenderVerification.Areas;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.RenderVerification.Bosses;

using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks D3D11 captures of the retail Ceres Ridley Mode 7 scene and retained packet replay.</summary>
internal static class RetailRidleyCaptureTests
{
    /// <summary>Compares native HUD/floor scenes across scale, tilt, and scroll, then replays saved packets after replacing live GPU resources.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var runtime = RepositoryInstallation.CreateRuntime();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.CeresRidley, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var ridley = runtime.Enemies.RidleyEncounter.CeresRidley;
        var retained = new List<RetainedFrame>();
        foreach (ushort scale in new ushort[] { 128, 240, 256 })
        foreach (short tilt in new short[] { -33, 0, 33 })
        foreach (short scroll in new short[] { -19, 0, 19 })
        {
            (ridley ?? throw new InvalidOperationException("Verification requires ridley.")).GetawayActive = true;
            ridley.FloorMatrixA = new SignedEightEight(scale); ridley.FloorMatrixD = new SignedEightEight(scale);
            ridley.FloorMatrixB = SignedEightEight.FromSignedRaw(tilt);
            ridley.FloorMatrixC = SignedEightEight.FromSignedRaw(unchecked((short)-tilt));
            ridley.FloorCenter = new Position16<FloorPixels>(128, 120);
            ridley.FloorOffset = new Offset16<FloorPixels>(7, unchecked(scroll));
            // Room main's M7A..M7Y shadows reach the PPU at the accepted NMI (#1307).
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            var expected = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame()).ToArray();
            var scene = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
            if (scene.Layers[0] is not TurningRoomLayer { Floor: not null } mixed ||
                mixed.HudScanlines != ScreenGeometry.GameplayHudHeightPixels)
                throw new InvalidOperationException("Retail Ridley fixture did not capture mixed Mode-7 floor and HUD.");
            var packet = new DisplayPacket(new(retained.Count + 1, 1, 0), scene);
            PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                $"{device.Kind}: retail Ridley scale={scale}, tilt={tilt}, scroll={scroll}");
            retained.Add(new RetainedFrame(packet, expected));
        }
        (ridley ?? throw new InvalidOperationException("Verification requires ridley.")).GetawayActive = false;
        runtime.Vram.LoadBytes(0, new byte[SnesVram.ByteCount]);
        for (int color = 0; color < SnesCgram.ColorCount; color++) runtime.Colours.AsPaletteColors().SetColor(color, new RgbColor(0, 0, 0));
        foreach (var sample in retained.AsEnumerable().Reverse())
            PixelComparison.Verify(sample.Packet, sample.Expected, renderer.RenderForReadback(sample.Packet),
                $"{device.Kind}: retained Ridley {sample.Packet.Identity.Sequence}");
        Console.WriteLine($"{device.Kind}: {retained.Count} retail Ridley mixed-mode captures and retained replays match exactly.");
        RetailRidleyEscapeTests.Run(device, renderer);
    }
}
