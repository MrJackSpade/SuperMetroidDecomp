using SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>Checks escape-room explosion sprites, earthquake offsets, and rescue exit behavior.</summary>
internal static class EscapeRoomEffectsVerification
{
    /// <summary>Samples six retail rooms for rendered explosions and shake, then checks the animals' opened passage.</summary>
    internal static void VerifyEscapeRoomEffects(string romPath)
    {
        foreach (ushort room in new ushort[] { 0xde4d, 0xde7a, 0xdea7, 0xdede, 0x92fd, 0x9804 })
        {
            var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
            var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Get(room));
            runtime.Player.IntroducedSamus.Control.LockHandlers();
            int explosionFrames = 0;
            bool visibleExplosion = false, visibleShake = false;
            var offsets = new HashSet<OctantVector>();
            for (int frame = 0; frame < 80; frame++)
            {
                runtime.StepFrame(0);
                if (runtime.Enemies.RoomSpriteObjects.Any(s => s.IsActive)) explosionFrames++;
                var shake = runtime.Enemies.Earthquake.LastRoomShake;
                offsets.Add(new OctantVector(shake.Bg1.X, shake.Bg1.Y));
                if (frame % 10 == 9)
                {
                    var capture = GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame());
                    var layer = (OrdinaryGameplayRenderLayer)capture.Layers[0];
                    var actual = SoftwareLayeredSnapshotRenderer.Render(capture);
                    var noObjects = new OrdinaryGameplayRenderLayer(layer.Registers with
                    { MainScreenLayers = layer.Registers.MainScreenLayers & ~SnesMainScreenLayers.Obj }, layer.Rooms,
                        layer.Bg2LineScrolls);
                    var baseline = SoftwareLayeredSnapshotRenderer.Render(new(capture.Memory, [noObjects], capture.Brightness));
                    foreach (var sprite in runtime.Enemies.RoomSpriteObjects.Where(s => s.IsActive))
                    {
                        int x = sprite.Position.X.Whole - runtime.Room.LoadedCamera.PixelPosition.X, y = sprite.Position.Y.Whole - runtime.Room.LoadedCamera.PixelPosition.Y;
                        if (Math.Abs(sprite.Position.X.Whole - runtime.Player.IntroducedSamus.Position.X.Whole) < 40 && Math.Abs(sprite.Position.Y.Whole - runtime.Player.IntroducedSamus.Position.Y.Whole) < 40) continue;
                        int changed = 0;
                        for (int py = Math.Max(32, y - 8); py < Math.Min(224, y + 8); py++)
                            for (int px = Math.Max(0, x - 8); px < Math.Min(256, x + 8); px++)
                                if (actual[py * 256 + px] != baseline[py * 256 + px]) changed++;
                        visibleExplosion |= changed > 8;
                    }
                    var displayedShake = runtime.Display.DisplayedGameplayPpu.RoomShake;
                    var stationary = new OrdinaryGameplayRenderLayer(noObjects.Registers with
                    {
                        Bg1Scroll = layer.Registers.Bg1Scroll - displayedShake.Bg1
                    }, layer.Rooms, layer.Bg2LineScrolls);
                    var unshaken = SoftwareLayeredSnapshotRenderer.Render(new(capture.Memory, [stationary], capture.Brightness));
                    visibleShake |= baseline.Where((pixel, index) => index >= 32 * 256 && pixel != unshaken[index]).Count() > 500;
                    AssertTrue(baseline.AsSpan(0, 32 * 256).SequenceEqual(unshaken.AsSpan(0, 32 * 256)), "escape shake does not move HUD");
                    if (visibleExplosion && visibleShake)
                    {

                    }
                }
            }
            AssertTrue(explosionFrames > 10, $"escape {room:X4} produces animated explosion sprites");
            AssertTrue(offsets.Count > 1, $"escape {room:X4} alternates actual background scroll offsets");
            AssertTrue(visibleExplosion, $"escape {room:X4} explosion appears in rendered pixels away from Samus");
            AssertTrue(visibleShake, $"escape {room:X4} shake visibly displaces terrain");
            if (room == 0x9804)
            {
                var level = runtime.Room.LoadedLevelData;
                int origin = level.GetBlockIndex(new(15, 10));
                AssertEqual(0x4f, level.GetCollisionBlock(new(15, 10)).Bts.Value, "retail room loader installs rescue wall");
                runtime.Plms.TrySpawnProjectileShotBlock(level, origin, 0x4f, 1, true);
                for (int frame = 0; frame < 32; frame++) runtime.StepFrame(0);
                AssertTrue(runtime.System.HasEvent(EventNumber.CrittersEscaped), "live rescue room publishes escaped event");
                var door = runtime.Plms.GreyDoorSnapshots.Single();
                AssertEqual(SuperMetroid.Core.Rooms.Doors.GreyDoorPhase.Flashing, door.Phase, "rescue unlocks exit door");
                runtime.Plms.ProjectileTriggers.TryNotifyResidentProjectileHit(door.BlockIndex, 1);
                for (int frame = 0; frame < 32; frame++) runtime.StepFrame(0);
                AssertEqual(0, runtime.Plms.GreyDoorSnapshots.Count, "exit door finishes opening");
                var samus = runtime.Player.IntroducedSamus;
                samus.Kinematics.Position = samus.Kinematics.Position.WithWhole(new(48, 112));
                samus.Kinematics.Radius = new(5, 16);
                for (int frame = 0; frame < 32 && !runtime.RoomLoader.HasPendingDoorTransition; frame++)
                    SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = runtime.Plms }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = -(1 << 16) });
                AssertTrue(runtime.RoomLoader.HasPendingDoorTransition, "Samus can cross opened rescue exit and trigger room transition");
                AssertEqual((ushort)184, runtime.RoomLayer3Fx.TargetSurface.Y, "rescue liquid target matches retail FX header");
            }
            Console.WriteLine($"Escape {room:X4}: {explosionFrames} explosion frames, {offsets.Count} shake offsets.");
        }
    }
}
