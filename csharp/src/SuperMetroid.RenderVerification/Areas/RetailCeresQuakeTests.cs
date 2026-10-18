using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Areas;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.RepositoryInstallation;

/// <summary>Compares Ceres escape-room quake snapshots with direct frame rendering.</summary>
internal static class RetailCeresQuakeTests
{
    /// <summary>Advances two retail rooms through 240 frames each, requiring changing quake displacement.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        foreach (ushort room in new[] { RoomHeaderPointers.CeresDeadScientistRoom, RoomHeaderPointers.CeresFinalHallway })
        {
            var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = RepositoryInstallation.CreateRuntime(playerInvincibilityEnabled: true);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.FrameBoundary.BeginAcceptedFrame(0);
            runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(room, new Position16<RoomPixels>(0, 0));
            runtime.Enemies.RidleyEncounter.CeresStatus = CeresQuakeFixture.EscapeStatus;
            var offsets = new HashSet<RoomShakeFrameResult>();
            int shaking = 0;
            for (int tick = 0; tick < 240; tick++)
            {
                // Native door actors produce quake requests and the shared room
                // scheduler advances them. Do not inject screen offsets into capture.
                runtime.StepFrame(0);
                // HandleRoomShaking publishes the default (zero) delta on frames it does not
                // shake, so a non-default delta is a frame that displaced the backgrounds.
                var shake = runtime.Enemies.Earthquake.LastRoomShake;
                if (shake != default) { shaking++; offsets.Add(shake); }
                var expected = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame());
                var packet = new RenderFrameSnapshot(new(tick + 1, 1, (ushort)tick), GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));
                packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotWriter.Serialize(packet));
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: Ceres quake room={room:X4}, tick={tick}, shake={shake}");
            }
            if (shaking == 0 || offsets.Count < 2)
                throw new InvalidOperationException($"Ceres quake fixture {room:X4} did not exercise changing displacement.");
            Console.WriteLine($"{device.Kind}: Ceres room {room:X4}, 240 exact frames, {shaking} shaking frames and {offsets.Count} displacement states.");
        }
    }
}
