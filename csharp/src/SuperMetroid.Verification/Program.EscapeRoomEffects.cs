using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    private static void VerifyEscapeRoomEffects(string romPath)
    {
        foreach (ushort room in new ushort[] { 0xde4d, 0xde7a, 0xdea7, 0xdede, 0x92fd, 0x9804 })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
            var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
            runtime.LoadCartridgeRoomForDebug(room);
            runtime.Samus!.InputLocked = true;
            int explosionFrames = 0;
            bool visibleExplosion = false, visibleShake = false;
            var offsets = new HashSet<(short, short)>();
            for (int frame = 0; frame < 80; frame++)
            {
                runtime.StepFrame(0);
                if (runtime.Enemies.RoomSpriteObjects.Any(s => s.IsActive && s.SpritemapPointer != 0)) explosionFrames++;
                var shake = runtime.Enemies.LastRoomShake;
                offsets.Add((shake.Bg1X, shake.Bg1Y));
                if (frame % 10 == 9)
                {
                    var capture = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
                    var layer = (OrdinaryGameplayRenderLayer)capture.Layers[0];
                    var actual = SoftwareLayeredSnapshotRenderer.Render(capture);
                    var noObjects = new OrdinaryGameplayRenderLayer(layer.Registers with
                    { MainScreenLayers = layer.Registers.MainScreenLayers & ~SnesMainScreenLayers.Obj },
                        layer.HorizontalScrolls, layer.VerticalScrolls);
                    var baseline = SoftwareLayeredSnapshotRenderer.Render(new(capture.Memory, [noObjects], capture.ObjectSelection, capture.Brightness));
                    foreach (var sprite in runtime.Enemies.RoomSpriteObjects.Where(s => s.IsActive))
                    {
                        int x = sprite.XPosition - runtime.Camera!.XPosition, y = sprite.YPosition - runtime.Camera.YPosition;
                        if (Math.Abs(sprite.XPosition - runtime.Samus.XPosition) < 40 && Math.Abs(sprite.YPosition - runtime.Samus.YPosition) < 40) continue;
                        int changed = 0;
                        for (int py = Math.Max(32, y - 8); py < Math.Min(224, y + 8); py++)
                            for (int px = Math.Max(0, x - 8); px < Math.Min(256, x + 8); px++)
                                if (actual[py * 256 + px] != baseline[py * 256 + px]) changed++;
                        visibleExplosion |= changed > 8;
                    }
                    var displayedShake = runtime.DisplayedGameplayPpu.RoomShake;
                    var stationary = new OrdinaryGameplayRenderLayer(noObjects.Registers with
                    {
                        Bg1X = unchecked((ushort)(layer.Registers.Bg1X - displayedShake.Bg1X)),
                        Bg1Y = unchecked((ushort)(layer.Registers.Bg1Y - displayedShake.Bg1Y))
                    }, layer.HorizontalScrolls, layer.VerticalScrolls);
                    var unshaken = SoftwareLayeredSnapshotRenderer.Render(new(capture.Memory, [stationary], capture.ObjectSelection, capture.Brightness));
                    visibleShake |= baseline.Where((pixel, index) => index >= 32 * 256 && pixel != unshaken[index]).Count() > 500;
                    AssertTrue(baseline.AsSpan(0, 32 * 256).SequenceEqual(unshaken.AsSpan(0, 32 * 256)), "escape shake does not move HUD");
                    if (visibleExplosion && visibleShake)
                    {
                        Directory.CreateDirectory("csharp/test-temp/escape-effects");
                        PngWriter.WriteRgba($"csharp/test-temp/escape-effects/{room:X4}.png", 256, 224, actual);
                    }
                }
            }
            AssertTrue(explosionFrames > 10, $"escape {room:X4} produces animated explosion sprites");
            AssertTrue(offsets.Count > 1, $"escape {room:X4} alternates actual background scroll offsets");
            AssertTrue(visibleExplosion, $"escape {room:X4} explosion appears in rendered pixels away from Samus");
            AssertTrue(visibleShake, $"escape {room:X4} shake visibly displaces terrain");
            if (room == 0x9804)
            {
                var level = runtime.LevelData!;
                int origin = level.GetBlockIndex(15, 10);
                AssertEqual((byte)0x4f, level.GetCollisionBlock(15, 10).Behavior, "retail room loader installs rescue wall");
                runtime.Plms.TrySpawnProjectileShotBlock(level, origin, (byte)0x4f, 1, true);
                for (int frame = 0; frame < 32; frame++) runtime.StepFrame(0);
                AssertTrue(runtime.System.HasEvent(EventNumber.CrittersEscaped), "live rescue room publishes escaped event");
                var door = runtime.Plms.GreyDoors.Single();
                AssertEqual(SuperMetroid.Core.Rooms.GreyDoorPhase.Flashing, door.Phase, "rescue unlocks exit door");
                runtime.Plms.TryNotifyResidentProjectileHit(door.BlockIndex, 1);
                for (int frame = 0; frame < 32; frame++) runtime.StepFrame(0);
                AssertEqual(0, runtime.Plms.GreyDoors.Count, "exit door finishes opening");
                var samus = runtime.Samus!;
                samus.Kinematics.XPosition = 48;
                samus.Kinematics.YPosition = 112;
                samus.Kinematics.XRadius = 5;
                samus.Kinematics.YRadius = 16;
                for (int frame = 0; frame < 32 && !runtime.HasPendingDoorTransition; frame++)
                    SamusBlockCollision.MoveHorizontal(bus, level, samus.Kinematics, -(1 << 16), plms: runtime.Plms);
                AssertTrue(runtime.HasPendingDoorTransition, "Samus can cross opened rescue exit and trigger room transition");
                AssertEqual((ushort)184, runtime.RoomLayer3Fx.TargetYPosition, "rescue liquid target matches retail FX header");
            }
            Console.WriteLine($"Escape {room:X4}: {explosionFrames} explosion frames, {offsets.Count} shake offsets.");
        }
    }
}
