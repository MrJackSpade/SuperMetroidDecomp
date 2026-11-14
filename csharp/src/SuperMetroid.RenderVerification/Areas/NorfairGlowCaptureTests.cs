using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Hud;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Areas;

using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Rooms;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks D3D11 capture of Norfair's animated foreground glow against production rendering.</summary>
internal static class NorfairGlowCaptureTests
{
    /// <summary>Compares 232 retail Cathedral frames and isolates palette-only BG1 pixel changes while holding geometry and unrelated colors fixed.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var runtime = RepositoryInstallation.CreateRuntime(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Cathedral, new Position16<RoomPixels>(0, 0));
        runtime.Player.IntroducedSamus.Control.LockOutPlayer();
        LayeredScene? original = null;
        RgbaPixel[]? initialPixels = null;
        int maximumChanged = 0;
        for (int tick = 0; tick < 232; tick++)
        {
            runtime.StepFrame(0);
            var scene = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
            original ??= scene;
            var packet = new DisplayPacket((ushort)tick, scene);
            PixelComparison.Verify(packet, FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame()),
                renderer.RenderForReadback(packet), $"{device.Kind}: Norfair glow frame {tick}");

            // Freeze geometry, camera and objects, and show only the foreground under this frame's room shading
            // (#1512): any remaining pixel change is the room's glow shade, not a moving enemy, liquid surface, or BG2
            // heat distortion.
            FramePaint memory = scene.Paint;
            var originalLayer = (GameplaySceneLayer)original.Layers[0];
            var registers = originalLayer.View;
            var foreground = new LayeredScene(memory,
                [new GameplaySceneLayer(registers with { ShownLayers = ScreenLayers.Foreground }, originalLayer.Rooms)], original.Brightness);
            var isolated = new DisplayPacket((ushort)tick, foreground);
            var pixels = DisplayRendering.Render(isolated);
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
