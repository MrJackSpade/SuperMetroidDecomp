using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Frontend;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.RenderVerification.Rooms;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.Core.Enemies.BossBodies;
namespace SuperMetroid.RenderVerification.Bosses;

using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RenderFixtures;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.Core.Progression;

/// <summary>Compares GPU captures through Kraid's rise, growth, death, and defeated-room exits.</summary>
internal static class RetailKraidCaptureTests
{
    /// <summary>Checks exact sampled frames across boss phases and both post-defeat door transitions.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = RepositoryInstallation.CreateRuntime(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomThroughDoorForVerification(SuperMetroid.AssetExtraction.Rooms.CartridgeDoorHeaderImporter.Load(bus, KraidCaptureDefinitions.IncomingDoor), new Position16<RoomPixels>(0, 256));
        var samus = runtime.Player.IntroducedSamus;
        // Match the existing Kraid rise audit's stationary approach position.
        samus.Position = samus.Position.WithWhole(new(128, 456));
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus); SamusPresentation.InitializeAnimation(samus);
        // The first sample is captured before any update: latch the loaded room for display first.
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var boss = runtime.Enemies.KraidBehavior.Access.Kraid ?? throw new InvalidOperationException("Kraid did not initialize.");
        var body = runtime.Enemies.SlotTable.Slots[0];
        KraidPhase BodyPhase() =>
            (body.SpeciesState<KraidPartState>() ?? throw new InvalidOperationException("Kraid's body has no part state.")).Phase
                ?? throw new InvalidOperationException("Kraid's body runs no phase.");
        // The death has played out once the background fade hands the body to either finished entry.
        static bool DeathFinished(KraidEnemyState kraid) =>
            kraid.BodyState.Phase is KraidPhase.DeathFinishedWasAlive or KraidPhase.DeathFinishedWasDead;
        var phases = new HashSet<KraidPhase>();
        int samples = 0;
        bool completed = false, ownedBackground = false;
        for (int tick = 0; tick < 1200; tick++)
        {
            bool changed = phases.Add(BodyPhase());
            ownedBackground |= boss.ShowsBodyLayer;
            bool mainLoop = BodyPhase() == KraidPhase.MainloopThinking;
            if (changed || tick % 8 == 0 || mainLoop)
            {
                var expected = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame());
                samples++;
                var packet = new DisplayPacket((ushort)tick, GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: Kraid rise tick={tick}, AI={BodyPhase()}");
            }
            if (mainLoop) { completed = true; break; }
            runtime.StepFrame(0);
        }
        if (!completed || !ownedBackground || phases.Count < 2)
            throw new InvalidOperationException("Kraid capture missed rise, BG2 ownership or first-phase handoff.");
        Console.WriteLine($"{device.Kind}: Kraid rise reaches first-phase main loop; {samples} exact samples across {phases.Count} AI states.");
        for (int hit = 0; hit < 2; hit++)
        {
            int waiting = 0;
            // The attack enters with a roar timer before its first head frame selects a mouth
            // shape; until then no frame is displayed, or the closed mouth's has no shape.
            while (!(BodyPhase() is KraidPhase.MainAttackWithMouthOpen or KraidPhase.MouthOpenReaction
                && boss.DisplayedHeadFrame is { InvulnerableShape: not null }) && waiting++ < 1400)
                runtime.StepFrame(0);
            if (waiting >= 1400) throw new InvalidOperationException("Kraid never opened its mouth.");
            StrikeMouth(runtime, body, boss);
            runtime.StepFrame(0);
        }
        int growthSamples = 0;
        var growthPhases = new HashSet<KraidPhase>();
        completed = false;
        for (int tick = 0; tick < 1800; tick++)
        {
            bool changed = growthPhases.Add(BodyPhase());
            bool finished = BodyPhase() == KraidPhase.SecondPhaseThinking;
            if (changed || tick % 4 == 0 || finished)
            {
                var expected = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame());
                samples++;
                var packet = new DisplayPacket((ushort)tick, GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), $"{device.Kind}: Kraid growth tick={tick}, AI={BodyPhase()}");
                growthSamples++;
            }
            if (finished) { completed = true; break; }
            runtime.StepFrame(0);
        }
        // Reaching second-phase thinking passes the growth step that releases the first-screen camera.
        if (!completed || boss.BodyAppearance != KraidBodyAppearance.InFrontOfSprites)
            throw new InvalidOperationException("Kraid growth missed camera/priority/second-phase handoff.");
        Console.WriteLine($"{device.Kind}: Kraid growth: {growthSamples} exact samples across {growthPhases.Count} AI states, camera and BG2 priority handoff verified.");
        int deathWait = 0;
        while (!(BodyPhase() is KraidPhase.MainAttackWithMouthOpen or KraidPhase.MouthOpenReaction
            && boss.DisplayedHeadFrame is not { InvulnerableShape: null }) && deathWait++ < 1400)
            runtime.StepFrame(0);
        if (deathWait >= 1400) throw new InvalidOperationException("Kraid death fixture never reached an open mouth.");
        // Shorten combat only, as in the established runtime audit. The lethal hit
        // still runs collision and initializes the real death coroutine.
        body.Health = 100;
        StrikeMouth(runtime, body, boss);
        if (body.Health != 0 || BodyPhase() != KraidPhase.DeathInitialize)
            throw new InvalidOperationException("Kraid lethal hit did not start death.");
        int deathSamples = 0;
        var deathPhases = new HashSet<KraidPhase>();
        for (int tick = 0; tick < 1200; tick++)
        {
            bool changed = deathPhases.Add(BodyPhase());
            if (changed || tick % 4 == 0 || DeathFinished(boss))
            {
                var expected = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame());
                samples++;
                var packet = new DisplayPacket((ushort)tick, GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), $"{device.Kind}: Kraid death tick={tick}, AI={BodyPhase()}");
                deathSamples++;
            }
            if (DeathFinished(boss)) break;
            runtime.StepFrame(0);
        }
        if (!DeathFinished(boss) || !deathPhases.Contains(KraidPhase.DeathWaitBeforeFadeIn4) ||
            !deathPhases.Contains(KraidPhase.DeathSink) ||
            !deathPhases.Contains(KraidPhase.DeathFadeInBackground))
            throw new InvalidOperationException("Kraid death missed sinking, the wait before the fade-in or the background fade.");
        RequireBlankBg2(runtime, $"{device.Kind}: defeated Kraid");
        Console.WriteLine($"{device.Kind}: Kraid death: {deathSamples} exact samples across {deathPhases.Count} AI states; sinking, wait, fade and blank BG2 verified.");
        RetailDoorCaptureTests.VerifyTransition(device, renderer, runtime, new RetailDoorCase { Door = KraidAuditDefinitions.LeftExitDoor, Destination = KraidAuditDefinitions.LeftExitDestination }, "defeated Kraid left", RequireSteadyHud(runtime, $"{device.Kind}: defeated Kraid left"));
        // The second route starts from the saved defeat bit, as in the existing
        // regression. Run the defeated-room restoration before opening the other door.
        runtime.LoadCartridgeRoomThroughDoorForVerification(SuperMetroid.AssetExtraction.Rooms.CartridgeDoorHeaderImporter.Load(bus, KraidCaptureDefinitions.IncomingDoor), new Position16<RoomPixels>(0, 256));
        int restoreTicks = 0;
        while (runtime.Enemies.KraidBehavior.Access.Kraid is { } reloaded && !DeathFinished(reloaded) && restoreTicks++ < 120)
            runtime.StepFrame(0);
        if (runtime.Enemies.KraidBehavior.Access.Kraid is not { } restored || !DeathFinished(restored))
            throw new InvalidOperationException("Defeated Kraid reload did not restore its background.");
        RetailDoorCaptureTests.VerifyTransition(device, renderer, runtime, new RetailDoorCase { Door = KraidAuditDefinitions.RightExitDoor, Destination = KraidAuditDefinitions.RightExitDestination }, "defeated Kraid right", RequireSteadyHud(runtime, $"{device.Kind}: defeated Kraid right"));
    }

    /// <summary>
    /// Requires every BG2 tile to be Kraid's blank tile. Once he has sunk, <c>$A7:C715</c> and <c>$A7:C751</c> fill
    /// BG2's 32x32 map with <c>$0338</c>, so nothing of his body or the room's BG2 remains (#1427). BG2 draws his body
    /// from its pictures (#1392, #1403), so the frame must still show his body, and every word of his latched body must
    /// be the blank tile.
    /// </summary>
    private static void RequireBlankBg2(SuperMetroidRuntime runtime, string context)
    {
        LayeredScene scene = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        var gameplay = (GameplaySceneLayer)scene.Layers[0];
        if (gameplay.Rooms.BossBody is null)
            throw new InvalidOperationException($"{context}: BG2 shows the room's own background where Kraid's tilemap was expected.");
        var shown = new SortedSet<ushort>(BossBodyWords.Of(runtime.Display.Latched.Bosses.Body
            ?? throw new InvalidOperationException($"{context}: BG2 shows a boss body Kraid has not latched."), BossBodyOwner.Kraid,
            InstalledFixtures.BossArtwork));
        if (shown.Any(word => word != KraidWorkingMapDefinitions.BlankTile.PackedWord))
            throw new InvalidOperationException($"{context}: BG2 shows {string.Join(", ", shown.Select(word => $"${word:X4}"))}; only the blank tile ${KraidWorkingMapDefinitions.BlankTile.PackedWord:X4} remains on the cartridge.");
    }

    /// <summary>
    /// Requires every door frame's HUD rows to match the frame before the door: leaving Kraid's room must never draw the
    /// destination's tiles through his HUD character base (#1428).
    /// </summary>
    private static Action<DisplayPacket> RequireSteadyHud(SuperMetroidRuntime runtime, string context)
    {
        const int hudPixels = ScreenGeometry.ScreenWidthPixels * ScreenGeometry.GameplayHudHeightPixels;
        RgbaPixel[] before = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame()).AsSpan(0, hudPixels).ToArray();
        int frame = 0;
        return packet =>
        {
            frame++;
            if (!DisplayRendering.Render(packet).AsSpan(0, hudPixels).SequenceEqual(before))
                throw new InvalidOperationException($"{context}: door frame {frame} changed the HUD.");
        };
    }

    /// <summary>Stages an audit missile at Kraid's active mouth hitbox and uses production collision.</summary>
    private static void StrikeMouth(SuperMetroidRuntime runtime, RoomEnemySlot body, KraidEnemyState boss)
    {
        // Match the established audit: stage a missile at the open-mouth hitbox, then use
        // the real collision/damage dispatcher to initiate growth. Resolve the shape exactly
        // as collision does: compiled $A7:9788 records or a live bank-$A7 low-half alias.
        var (left, top, bottom) = KraidMouthHitboxes.ResolveCollision(
            boss.DisplayedHeadFrame?.InvulnerableShape ?? throw new InvalidOperationException("Kraid shows no inner mouth shape to strike."));
        var shots = InstalledFixtures.CreateProjectileSystem();
        var shot = shots.Slots[0];
        shot.Type = SamusProjectileType.Of(SamusProjectileFamily.Missile);
        shot.Damage = 100; shot.Direction = SamusProjectileDirection.Right;
        shot.Position = shot.Position.WithWhole(new(unchecked((ushort)(body.Position.X.Whole + left + 2)), unchecked((ushort)(body.Position.Y.Whole + (top + bottom) / 2))));
        shot.Radius = new(2, 2);
        // Any live missile program marks the slot active; this staged shot only enters collision, not animation.
        shot.Playback = SamusProjectilePlayback.Start(SamusProjectileSegment.MissileRight);
        shot.FrameHold = WrappingCountdown.Start(1);
        // Kraid's collision walk starts at the native projectile count; one staged shot is count one.
        shots.SlotTable.ProjectileCounter = 1;
        // The native walk counts one missile in both its mouth and body passes (#1192, #1247).
        if (runtime.Enemies.ResolveKraidProjectileHits(shots, InstalledFixtures.CreateBombSystem()) == 0)
            throw new InvalidOperationException("Kraid growth fixture failed to land its staged missile.");
    }
}
