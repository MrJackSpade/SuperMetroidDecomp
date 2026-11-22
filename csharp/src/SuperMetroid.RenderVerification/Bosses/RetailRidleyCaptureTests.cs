using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Bosses.Ridley;
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
    /// <summary>The getaway updates the comparison samples: every twelfth, so its zoom, turn and scroll cover their range.</summary>
    private const int SampleSpacing = 12;

    /// <summary>
    /// Compares native HUD/floor scenes across the getaway's scales, tilts and scrolls, then replays saved packets after
    /// replacing live GPU resources.
    /// </summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var runtime = RepositoryInstallation.CreateRuntime();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.CeresRidley, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var retained = new List<RetainedFrame>();
        CeresEscape escape = runtime.Enemies.CeresEscape;
        RoomFloor arena = runtime.Room.LoadedScope.Floor ?? throw new InvalidOperationException("Ridley's arena shows his floor.");
        escape.BeginGetaway();
        for (int update = 0; update < CeresRidleyGetawayCurve.Steps.Count; update++)
        {
            escape.RunGetawayRoomMain(runtime.Player.Samus, arena, update);
            if (update % SampleSpacing != 0)
                continue;
            // The getaway's floor reaches the display at the accepted frame boundary (#1307).
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            var expected = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame()).ToArray();
            var scene = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
            if (scene.Layers[0] is not TurningRoomLayer { LowerBand: not null } mixed ||
                mixed.HudLines != ScreenGeometry.GameplayHudHeightPixels)
                throw new InvalidOperationException("Retail Ridley fixture did not capture mixed Mode-7 floor and HUD.");
            var packet = new DisplayPacket(retained.Count + 1, scene);
            PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                $"{device.Kind}: retail Ridley getaway update {update}, view {escape.ShownFloor}");
            retained.Add(new RetainedFrame(packet, expected));
        }
        // The update after the getaway's last hands on to the self-destruct warning, which leaves the turning floor.
        escape.RunGetawayRoomMain(runtime.Player.Samus, arena, CeresRidleyGetawayCurve.Steps.Count);
        if (escape.ShownFloor is not null)
            throw new InvalidOperationException("The getaway's end leaves the turning floor.");
        runtime.Vram.LoadBytes(0, new byte[SnesVram.ByteCount]);
        foreach (var sample in retained.AsEnumerable().Reverse())
            PixelComparison.Verify(sample.Packet, sample.Expected, renderer.RenderForReadback(sample.Packet),
                $"{device.Kind}: retained Ridley {sample.Packet.FrameNumber}");
        Console.WriteLine($"{device.Kind}: {retained.Count} retail Ridley mixed-mode captures and retained replays match exactly.");
        RetailRidleyEscapeTests.Run(device, renderer);
    }
}
