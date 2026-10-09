using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Checks retail Ceres quake displacement across captured and rendered frames.</summary>
internal static class RetailCeresQuakeTests
{
    /// <summary>Replays both Ceres escape rooms and verifies exact rendering over 240 frames each.</summary>
    /// <param name="device">Graphics device used to render readback frames.</param>
    /// <param name="renderer">Frame renderer used for the captured gameplay snapshots.</param>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        foreach (ushort room in new[] { RoomHeaderPointers.CeresDeadScientistRoom, RoomHeaderPointers.CeresFinalHallway })
        {
            var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = RepositoryInstallation.CreateRuntime(bus, playerInvincibilityEnabled: true);
            runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.RunNmi(0, true);
            runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(room, 0, 0);
            runtime.Enemies.CeresStatus = CeresQuakeFixture.EscapeStatus;
            var offsets = new HashSet<RoomShakeFrameResult>();
            int shaking = 0;
            for (int tick = 0; tick < 240; tick++)
            {
                // Native door actors produce quake requests and the shared room
                // scheduler advances them. Do not inject screen offsets into capture.
                runtime.StepFrame(0);
                // HandleRoomShaking publishes the default (zero) delta on frames it does not
                // shake, so a non-default delta is a frame that displaced the backgrounds.
                var shake = runtime.Enemies.LastRoomShake;
                if (shake != default) { shaking++; offsets.Add(shake); }
                var expected = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                var packet = new RenderFrameSnapshot(new(tick + 1, 1, (ushort)tick), GameplayDisplayCapture.TryCaptureFrame(runtime)!);
                packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: Ceres quake room={room:X4}, tick={tick}, shake={shake}");
            }
            if (shaking == 0 || offsets.Count < 2)
                throw new InvalidOperationException($"Ceres quake fixture {room:X4} did not exercise changing displacement.");
            Console.WriteLine($"{device.Kind}: Ceres room {room:X4}, 240 exact frames, {shaking} shaking frames and {offsets.Count} displacement states.");
        }
    }
}

/// <summary>Native state values needed to activate Ceres escape quake behavior in the retail fixtures.</summary>
internal static class CeresQuakeFixture
{
    /// <summary>Native Ceres status word after Ridley's departure enables door earthquake producers.</summary>
    internal const ushort EscapeStatus = 2;
}
