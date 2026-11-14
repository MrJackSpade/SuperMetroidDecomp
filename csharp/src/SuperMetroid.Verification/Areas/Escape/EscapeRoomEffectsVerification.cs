using SuperMetroid.AssetExtraction;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Assets;
using SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms;
using SuperMetroid.TestSupport.FrameRendering;
namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>Checks escape-room explosion sprites, earthquake offsets, and rescue exit behavior.</summary>
internal static class EscapeRoomEffectsVerification
{
    /// <summary>The escape rooms whose explosions and shaking are checked, each as a suite of its own.</summary>
    private static readonly ushort[] EscapeRooms = [0xde4d, 0xde7a, 0xdea7, 0xdede, 0x92fd, 0x9804];

    /// <summary>The escape-room effects checks, one suite for each escape room.</summary>
    internal static FullRunSuitesVerification.RegisteredSuite[] EscapeRoomEffectsSuites() =>
        [.. EscapeRooms.Select(room => new FullRunSuitesVerification.RegisteredSuite($"{nameof(VerifyEscapeRoomEffects)}{room:X4}", () => VerifyEscapeRoomEffects(room)))];

    /// <summary>
    /// Samples retail escape room <paramref name="room"/> for rendered explosions and shake; in the animals' room, then
    /// checks their opened passage.
    /// </summary>
    internal static void VerifyEscapeRoomEffects(ushort room)
    {
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(StoryEvent.ZebesTimebombSet);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Get(room));
        runtime.Player.IntroducedSamus.Control.LockOutPlayer();
        int explosionFrames = 0;
        bool visibleExplosion = false, visibleShake = false;
        var offsets = new HashSet<OctantVector>();
        for (int frame = 0; frame < 80; frame++)
        {
            runtime.StepFrame(0);
            if (runtime.Enemies.RoomSpriteObjects.Any(s => s.IsActive)) explosionFrames++;
            var shake = runtime.Enemies.Earthquake.LastRoomShake;
            offsets.Add(new OctantVector(shake.Foreground.X, shake.Foreground.Y));
            if (frame % 10 == 9)
            {
                var capture = GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame());
                var layer = (GameplaySceneLayer)capture.Layers[0];
                var actual = DisplayRendering.Render(capture);
                var noObjects = new GameplaySceneLayer(layer.View with
                { ShownLayers = layer.View.ShownLayers & ~ScreenLayers.Sprites }, layer.Rooms,
                    layer.BackgroundLineScrolls);
                // The HUD is its own layer after the room (#1466); both renders keep it.
                var baseline = DisplayRendering.Render(new LayeredScene(capture.Paint, [noObjects, capture.Layers[1]], capture.Brightness));
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
                var displayedShake = runtime.Display.DisplayedGameplayScrolls.RoomShake;
                var stationary = new GameplaySceneLayer(noObjects.View with
                {
                    ForegroundScroll = layer.View.ForegroundScroll - displayedShake.Foreground
                }, layer.Rooms, layer.BackgroundLineScrolls);
                var unshaken = DisplayRendering.Render(new LayeredScene(capture.Paint, [stationary, capture.Layers[1]], capture.Brightness));
                visibleShake |= baseline.Where((pixel, index) => index >= 32 * 256 && pixel != unshaken[index]).Count() > 500;
                AssertTrue(baseline.AsSpan(0, 32 * 256).SequenceEqual(unshaken.AsSpan(0, 32 * 256)), "escape shake does not move HUD");
            }
        }
        AssertTrue(explosionFrames > 10, $"escape {room:X4} produces animated explosion sprites");
        AssertTrue(offsets.Count > 1, $"escape {room:X4} alternates actual background scroll offsets");
        AssertTrue(visibleExplosion, $"escape {room:X4} explosion appears in rendered pixels away from Samus");
        AssertTrue(visibleShake, $"escape {room:X4} shake visibly displaces terrain");
        if (room == 0x9804)
        {
            var level = runtime.Room.LoadedLevelData;
            var origin = new Position16<RoomBlocks>(15, 10);
            AssertEqual(0x4f, RoomBlockBehaviorCodec.Encode(level.GetCollisionBlock(new(15, 10)).Behavior), "retail room loader installs rescue wall");
            runtime.RoomObjects.TrySpawnProjectileShotBlock(level, origin, 0x4f, SamusProjectileType.Beam(SamusBeamCombination.Wave, charged: false), true);
            for (int frame = 0; frame < 32; frame++) runtime.StepFrame(0);
            AssertTrue(runtime.System.HasEvent(StoryEvent.CrittersEscaped), "live rescue room publishes escaped event");
            var door = runtime.RoomObjects.GreyDoorSnapshots.Single();
            AssertEqual(SuperMetroid.Core.Rooms.Doors.GreyDoorPhase.Flashing, door.Phase, "rescue unlocks exit door");
            runtime.RoomObjects.ProjectileTriggers.TryNotifyResidentProjectileHit(door.Block, SamusProjectileType.Beam(SamusBeamCombination.Wave, charged: false));
            for (int frame = 0; frame < 32; frame++) runtime.StepFrame(0);
            AssertEqual(0, runtime.RoomObjects.GreyDoorSnapshots.Count, "exit door finishes opening");
            var samus = runtime.Player.IntroducedSamus;
            samus.Kinematics.Position = samus.Kinematics.Position.WithWhole(new(48, 112));
            samus.Kinematics.Radius = new(5, 16);
            for (int frame = 0; frame < 32 && !runtime.RoomLoader.HasPendingDoorTransition; frame++)
                SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = runtime.RoomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = SixteenSixteenWords.ToDisplacement(-(1 << 16)) });
            AssertTrue(runtime.RoomLoader.HasPendingDoorTransition, "Samus can cross opened rescue exit and trigger room transition");
            AssertEqual(184, runtime.RoomFxOverlay.NamedTargetSurface, "rescue liquid target matches retail FX header");
        }
        Console.WriteLine($"Escape {room:X4}: {explosionFrames} explosion frames, {offsets.Count} shake offsets.");
    }
}
