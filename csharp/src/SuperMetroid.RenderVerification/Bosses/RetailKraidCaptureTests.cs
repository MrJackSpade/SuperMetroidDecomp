using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
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
namespace SuperMetroid.RenderVerification.Bosses;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RenderFixtures;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.RepositoryInstallation;

/// <summary>Compares GPU captures through Kraid's rise, growth, death, and defeated-room exits.</summary>
internal static class RetailKraidCaptureTests
{
    /// <summary>Checks exact sampled frames across boss phases and both post-defeat door transitions.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = RepositoryInstallation.CreateRuntime(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomThroughDoorForVerification(SuperMetroid.AssetExtraction.Rooms.CartridgeDoorHeaderImporter.Load(bus, KraidCaptureDefinitions.IncomingDoor), new Position16<RoomPixels>(0, 256));
        var samus = runtime.Player.IntroducedSamus;
        // Match the existing Kraid rise audit's stationary approach position.
        samus.Position = samus.Position.WithWhole(new(128, 456));
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.PoseMetadata.RefreshCollisionRadii(); samus.Presentation.InitializeAnimation();
        var boss = runtime.Enemies.KraidBehavior.Kraid ?? throw new InvalidOperationException("Kraid did not initialize.");
        var body = runtime.Enemies.SlotTable.Slots[0];
        var phases = new HashSet<ushort>();
        int samples = 0;
        bool completed = false, ownedBackground = false;
        for (int tick = 0; tick < 1200; tick++)
        {
            bool changed = phases.Add(body.VariableA);
            ownedBackground |= boss.OwnsBg2Tilemap;
            bool mainLoop = body.VariableA == (ushort)KraidAiFunction.MainloopThinking;
            if (changed || tick % 8 == 0 || mainLoop)
            {
                var expected = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame());
                var packet = new RenderFrameSnapshot(new(++samples, 1, (ushort)tick), GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));
                packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotWriter.Serialize(packet));
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: Kraid rise tick={tick}, AI={body.VariableA:X4}");
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
            while (!(body.VariableA is (ushort)KraidAiFunction.MainAttackWithMouthOpen or (ushort)KraidAiFunction.MouthOpenReaction
                && boss.DisplayedHeadFrame is { InvulnerableShape: not null }) && waiting++ < 1400)
                runtime.StepFrame(0);
            if (waiting >= 1400) throw new InvalidOperationException("Kraid never opened its mouth.");
            StrikeMouth(bus, runtime, body, boss);
            runtime.StepFrame(0);
        }
        int growthSamples = 0;
        var growthPhases = new HashSet<ushort>();
        completed = false;
        for (int tick = 0; tick < 1800; tick++)
        {
            bool changed = growthPhases.Add(body.VariableA);
            bool finished = body.VariableA == (ushort)KraidAiFunction.SecondPhaseThinking;
            if (changed || tick % 4 == 0 || finished)
            {
                var expected = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame());
                var packet = new RenderFrameSnapshot(new(++samples, 1, (ushort)tick), GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));
                packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotWriter.Serialize(packet));
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), $"{device.Kind}: Kraid growth tick={tick}, AI={body.VariableA:X4}");
                growthSamples++;
            }
            if (finished) { completed = true; break; }
            runtime.StepFrame(0);
        }
        if (!completed || !boss.CameraReleasedForSecondPhase || !boss.Bg2PriorityBitsSet)
            throw new InvalidOperationException("Kraid growth missed camera/priority/second-phase handoff.");
        Console.WriteLine($"{device.Kind}: Kraid growth: {growthSamples} exact samples across {growthPhases.Count} AI states, camera and BG2 priority handoff verified.");
        int deathWait = 0;
        while (!(body.VariableA is (ushort)KraidAiFunction.MainAttackWithMouthOpen or (ushort)KraidAiFunction.MouthOpenReaction
            && boss.DisplayedHeadFrame is not { InvulnerableShape: null }) && deathWait++ < 1400)
            runtime.StepFrame(0);
        if (deathWait >= 1400) throw new InvalidOperationException("Kraid death fixture never reached an open mouth.");
        // Shorten combat only, as in the established runtime audit. The lethal hit
        // still runs collision and initializes the real death coroutine.
        body.Health = 100;
        StrikeMouth(bus, runtime, body, boss);
        if (body.Health != 0 || body.VariableA != (ushort)KraidAiFunction.DeathInitialize)
            throw new InvalidOperationException("Kraid lethal hit did not start death.");
        int deathSamples = 0;
        var deathPhases = new HashSet<ushort>();
        for (int tick = 0; tick < 1200; tick++)
        {
            bool changed = deathPhases.Add(body.VariableA);
            if (changed || tick % 4 == 0 || boss.DeathSequenceComplete)
            {
                var expected = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame());
                var packet = new RenderFrameSnapshot(new(++samples, 1, (ushort)tick), GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));
                packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotWriter.Serialize(packet));
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), $"{device.Kind}: Kraid death tick={tick}, AI={body.VariableA:X4}");
                deathSamples++;
            }
            if (boss.DeathSequenceComplete) break;
            runtime.StepFrame(0);
        }
        if (!boss.DeathSequenceComplete || boss.DeathBg3TransferCount != 4 ||
            !deathPhases.Contains((ushort)KraidAiFunction.DeathSink) ||
            !deathPhases.Contains((ushort)KraidAiFunction.DeathFadeInBackground))
            throw new InvalidOperationException("Kraid death missed sinking, background fade or four BG3 restores.");
        Console.WriteLine($"{device.Kind}: Kraid death: {deathSamples} exact samples across {deathPhases.Count} AI states; sinking, fade and BG3 restore verified.");
        RetailDoorCaptureTests.VerifyTransition(device, renderer, runtime, new RetailDoorCase { Door = KraidAuditDefinitions.LeftExitDoor, Destination = KraidAuditDefinitions.LeftExitDestination }, "defeated Kraid left");
        // The second route starts from the saved defeat bit, as in the existing
        // regression. Run the defeated-room restoration before opening the other door.
        runtime.LoadCartridgeRoomThroughDoorForVerification(SuperMetroid.AssetExtraction.Rooms.CartridgeDoorHeaderImporter.Load(bus, KraidCaptureDefinitions.IncomingDoor), new Position16<RoomPixels>(0, 256));
        int restoreTicks = 0;
        while (runtime.Enemies.KraidBehavior.Kraid is { DeathSequenceComplete: false } && restoreTicks++ < 120)
            runtime.StepFrame(0);
        if (runtime.Enemies.KraidBehavior.Kraid is not { DeathSequenceComplete: true })
            throw new InvalidOperationException("Defeated Kraid reload did not restore its background.");
        RetailDoorCaptureTests.VerifyTransition(device, renderer, runtime, new RetailDoorCase { Door = KraidAuditDefinitions.RightExitDoor, Destination = KraidAuditDefinitions.RightExitDestination }, "defeated Kraid right");
    }

    /// <summary>Stages an audit missile at Kraid's active mouth hitbox and uses production collision.</summary>
    private static void StrikeMouth(ISnesAddressSpace bus, SuperMetroidRuntime runtime, RoomEnemySlot body, KraidEnemyState boss)
    {
        // Match the established audit: stage a missile at the open-mouth hitbox, then use
        // the real collision/damage dispatcher to initiate growth. Resolve the shape exactly
        // as collision does: compiled $A7:9788 records or a live bank-$A7 low-half alias.
        var (left, top, bottom) = KraidMouthHitboxDefinitions.ResolveCollision(
            boss.DisplayedHeadFrame?.InvulnerableShape ?? throw new InvalidOperationException("Kraid shows no inner mouth shape to strike."));
        var shots = InstalledFixtures.CreateProjectileSystem();
        var shot = shots.Slots[0];
        shot.Type = KraidCaptureDefinitions.AuditMissileType;
        shot.Damage = 100; shot.Direction = (ushort)SamusProjectileDirection.Right;
        shot.Position = shot.Position.WithWhole(new(unchecked((ushort)(body.Position.X.Whole + left + 2)), unchecked((ushort)(body.Position.Y.Whole + (top + bottom) / 2))));
        shot.Radius = new(2, 2);
        // Any live missile program marks the slot active; this staged shot only enters collision, not animation.
        shot.Cursor = AnimationProgramCursor<SamusProjectileProgram>.Start(SamusProjectileProgram.MissileRight);
        shot.InstructionTimer = NativeCountdown.Start(1);
        // Kraid's collision walk starts at the native projectile count; one staged shot is count one.
        shots.SlotTable.ProjectileCounter = (ushort)1;
        // The native walk counts one missile in both its mouth and body passes (#1192, #1247).
        if (runtime.Enemies.ResolveKraidProjectileHits(bus, shots, InstalledFixtures.CreateBombSystem()) == 0)
            throw new InvalidOperationException("Kraid growth fixture failed to land its staged missile.");
    }
}
