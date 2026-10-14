using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Rendering.Direct3D11;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Abilities;

/// <summary>Production capture and legacy composition agree through combined effect updates.</summary>
internal static class RuntimeOverlayTests
{
    /// <summary>Combines Power Bomb, message, and suit layers, then replays retained packets after updates.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = RepositoryInstallation.CreateRuntime(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.NmiHandler.RunNmi(0, true);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.NmiHandler.RunNmi(0, true);
        long sequence = 0;
        Check("Ceres mixed-mode initial publication");
        runtime.LoadCartridgeRoomForDebug(OverlayFixtureRooms.AlphaPowerBomb, new Position16<RoomPixels>(0, 0));
        runtime.NmiHandler.RunNmi(0, true);
        Check("ordinary room without overlays");

        // Deliberately combine owners rather than claim this is a natural pickup
        // sequence. This isolates the production composition order in one room.
        runtime.BombProjectiles.PowerBombExplosion.Arm();
        runtime.BombProjectiles.PowerBombExplosion.Spawn(new(128, 120));
        for (int tick = 0; tick < 50; tick++) runtime.BombProjectiles.PowerBombExplosion.StepFrame(bus);
        runtime.MessageBox.Begin(bus, GameplayMessageIds.MapDataAccessCompleted, 0);
        for (int tick = 0; tick < 13; tick++) runtime.MessageBox.Step(0);
        runtime.SuitPickup.Begin(bus, runtime.Player.IntroducedSamus, new(0, 0), SamusSuitPickupKind.Varia);
        runtime.SuitPickup.Step(bus, runtime.Player.IntroducedSamus, runtime.Cgram);
        var initial = GameplayDisplayCapture.CaptureFrame(runtime);
        if (initial.Layers.Length != 4 || initial.Layers[0] is not OrdinaryGameplayRenderLayer
            || initial.Layers[1] is not ScanlineColorAddRenderLayer
            || initial.Layers[2] is not MessageBoxRenderLayer
            || initial.Layers[3] is not ScanlineColorAddRenderLayer)
            throw new InvalidOperationException("Fixture missed base/Power Bomb/message/suit composition order.");

        for (int tick = 0; tick < 90; tick++)
        {
            var retained = Check($"combined overlay tick {tick}");
            runtime.BombProjectiles.PowerBombExplosion.StepFrame(bus);
            runtime.MessageBox.Step(0);
            runtime.SuitPickup.Step(bus, runtime.Player.IntroducedSamus, runtime.Cgram);
            // Force GPU resources to contain the next frame before replaying the old
            // packet. A cached output or borrowed live palette cannot satisfy this.
            Check($"updated overlay tick {tick}");
            PixelComparison.Verify(retained.Packet, retained.Pixels,
                renderer.RenderForReadback(retained.Packet), $"{device.Kind}: retained overlay tick {tick}");
        }
        Console.WriteLine($"{device.Kind}: {sequence} runtime frames and 90 retained overlay replays match exactly.");

        (RenderFrameSnapshot Packet, Rgba32[] Pixels) Check(string context)
        {
            Rgba32[] expected = SuperMetroidRuntimeFrameRenderer.Render(runtime).ToArray();
            var packet = new RenderFrameSnapshot(new(++sequence, 1, 0), GameplayDisplayCapture.CaptureFrame(runtime));
            packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotWriter.Serialize(packet));
            PixelComparison.Verify(packet, expected, SoftwareFrameSnapshotRenderer.Render(packet), $"software: {context}");
            PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), $"{device.Kind}: {context}");
            return (packet, expected);
        }
    }
}
