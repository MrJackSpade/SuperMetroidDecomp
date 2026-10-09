using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Captures Kraid's retail rise, growth, defeat, and exit-room restoration transitions.</summary>
internal static class RetailKraidCaptureTests
{
    /// <summary>Runs the Kraid encounter and compares selected runtime frames with the Direct3D readback.</summary>
    /// <param name="device">The render device used to label and perform pixel readback.</param>
    /// <param name="renderer">The frame renderer used to compare captured gameplay packets.</param>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = RepositoryInstallation.CreateRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomThroughDoorForVerification(SuperMetroid.AssetExtraction.CartridgeDoorHeaderImporter.Load(bus, KraidCaptureDefinitions.IncomingDoor), 0, 256);
        var samus = runtime.Samus!;
        // Match the existing Kraid rise audit's stationary approach position.
        samus.XPosition = 128; samus.YPosition = 456;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
        var boss = runtime.Enemies.Kraid ?? throw new InvalidOperationException("Kraid did not initialize.");
        var body = runtime.Enemies.Slots[0];
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
                var expected = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                var packet = new RenderFrameSnapshot(new(++samples, 1, (ushort)tick), GameplayDisplayCapture.TryCaptureFrame(runtime)!);
                packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
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
            // shape; until then the hitbox words still hold their initial values.
            while (!(body.VariableA is (ushort)KraidAiFunction.MainAttackWithMouthOpen or (ushort)KraidAiFunction.MouthOpenReaction
                && KraidMouthHitboxes.IsDefined(boss.InvulnerableMouthHitbox)) && waiting++ < 1400)
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
                var expected = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                var packet = new RenderFrameSnapshot(new(++samples, 1, (ushort)tick), GameplayDisplayCapture.TryCaptureFrame(runtime)!);
                packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
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
            && boss.InvulnerableMouthHitbox != ushort.MaxValue) && deathWait++ < 1400)
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
                var expected = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                var packet = new RenderFrameSnapshot(new(++samples, 1, (ushort)tick), GameplayDisplayCapture.TryCaptureFrame(runtime)!);
                packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
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
        RetailDoorCaptureTests.VerifyTransition(device, renderer, runtime, bus,
            KraidAuditDefinitions.LeftExitDoor, KraidAuditDefinitions.LeftExitDestination, "defeated Kraid left");
        // The second route starts from the saved defeat bit, as in the existing
        // regression. Run the defeated-room restoration before opening the other door.
        runtime.LoadCartridgeRoomThroughDoorForVerification(SuperMetroid.AssetExtraction.CartridgeDoorHeaderImporter.Load(bus, KraidCaptureDefinitions.IncomingDoor), 0, 256);
        int restoreTicks = 0;
        while (runtime.Enemies.Kraid is { DeathSequenceComplete: false } && restoreTicks++ < 120)
            runtime.StepFrame(0);
        if (runtime.Enemies.Kraid is not { DeathSequenceComplete: true })
            throw new InvalidOperationException("Defeated Kraid reload did not restore its background.");
        RetailDoorCaptureTests.VerifyTransition(device, renderer, runtime, bus,
            KraidAuditDefinitions.RightExitDoor, KraidAuditDefinitions.RightExitDestination, "defeated Kraid right");
    }

    /// <summary>Stages a missile at Kraid's open-mouth hitbox and sends it through the gameplay collision dispatcher.</summary>
    /// <param name="bus">The address space used to resolve the native or compiled mouth hitbox.</param>
    /// <param name="runtime">The running encounter whose enemy collision path processes the shot.</param>
    /// <param name="body">Kraid's body slot, used to position the staged missile relative to the hitbox.</param>
    /// <param name="boss">Kraid's state containing the currently active mouth-hitbox identity.</param>
    private static void StrikeMouth(ISnesAddressSpace bus, SuperMetroidRuntime runtime, RoomEnemySlot body, KraidEnemyState boss)
    {
        // Match the established audit: stage a missile at the open-mouth hitbox, then use
        // the real collision/damage dispatcher to initiate growth. Resolve the shape exactly
        // as collision does: compiled $A7:9788 records or a live bank-$A7 low-half alias.
        var (left, top, bottom) = KraidMouthHitboxes.ResolveCollision(bus, boss.InvulnerableMouthHitbox);
        var shots = RepositoryInstallation.CreateProjectileSystem();
        var shot = shots.Slots[0];
        shot.Type = KraidCaptureDefinitions.AuditMissileType;
        shot.Damage = 100; shot.Direction = (ushort)SamusProjectileDirection.Right;
        shot.XPosition = unchecked((ushort)(body.XPosition + left + 2));
        shot.YPosition = unchecked((ushort)(body.YPosition + (top + bottom) / 2));
        shot.XRadius = 2; shot.YRadius = 2;
        shot.InstructionPointer = KraidCaptureDefinitions.AuditLiveInstruction;
        shot.InstructionTimer = 1;
        // Kraid's collision walk starts at the native projectile count; one staged shot is count one.
        typeof(SamusProjectileSystem).GetProperty(nameof(SamusProjectileSystem.ProjectileCounter))!.SetValue(shots, (ushort)1);
        // The native walk counts one missile in both its mouth and body passes (#1192, #1247).
        if (runtime.Enemies.ResolveKraidProjectileHits(bus, shots, RepositoryInstallation.CreateBombSystem()) == 0)
            throw new InvalidOperationException("Kraid growth fixture failed to land its staged missile.");
    }
}

/// <summary>Native room and projectile words needed to construct the retail Kraid capture scenario.</summary>
internal static class KraidCaptureDefinitions
{
    /// <summary>Retail incoming door $83:91B6 used by the existing Kraid rise audit.</summary>
    internal const ushort IncomingDoor = 0x91b6;
    /// <summary>Live missile type word used by the established Kraid collision audit.</summary>
    internal const ushort AuditMissileType = 0x8100;
    /// <summary>Nonzero instruction sentinel; this staged projectile only enters collision, not animation.</summary>
    internal const ushort AuditLiveInstruction = 0x9000;
}
