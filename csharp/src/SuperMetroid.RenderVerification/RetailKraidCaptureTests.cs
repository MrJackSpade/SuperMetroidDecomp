using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using SuperMetroid.Rendering.Direct3D11;

internal static class RetailKraidCaptureTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomThroughDoorForVerification(CartridgeDoorHeader.Load(bus, KraidCaptureDefinitions.IncomingDoor), 0, 256);
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
            while (!(body.VariableA is (ushort)KraidAiFunction.MainAttackWithMouthOpen or (ushort)KraidAiFunction.MouthOpenReaction
                && boss.InvulnerableMouthHitbox != ushort.MaxValue) && waiting++ < 1400)
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
    }

    private static void StrikeMouth(ISnesAddressSpace bus, SuperMetroidRuntime runtime, RoomEnemySlot body, KraidEnemyState boss)
    {
        // Match the established audit: stage a missile at the ROM-authored open-mouth
        // hitbox, then use the real collision/damage dispatcher to initiate growth.
        int address = KraidBackgroundRomData.EnemyBankBase | boss.InvulnerableMouthHitbox;
        short left = unchecked((short)RomDataReader.ReadWordFixedBank(bus, address));
        short top = unchecked((short)RomDataReader.ReadWordFixedBank(bus, address + 2));
        short bottom = unchecked((short)RomDataReader.ReadWordFixedBank(bus, address + 6));
        var shots = new SamusProjectileSystem();
        var shot = shots.Slots[0];
        shot.Type = KraidCaptureDefinitions.AuditMissileType;
        shot.Damage = 100; shot.Direction = (ushort)SamusProjectileDirection.Right;
        shot.XPosition = unchecked((ushort)(body.XPosition + left + 2));
        shot.YPosition = unchecked((ushort)(body.YPosition + (top + bottom) / 2));
        shot.XRadius = 2; shot.YRadius = 2;
        shot.InstructionPointer = KraidCaptureDefinitions.AuditLiveInstruction;
        shot.InstructionTimer = 1;
        if (runtime.Enemies.ResolveKraidProjectileHits(bus, shots, new SamusBombProjectileSystem()) != 1)
            throw new InvalidOperationException("Kraid growth fixture failed to land its staged missile.");
    }
}

internal static class KraidCaptureDefinitions
{
    /// <summary>Retail incoming door $83:91B6 used by the existing Kraid rise audit.</summary>
    internal const ushort IncomingDoor = 0x91b6;
    /// <summary>Live missile type word used by the established Kraid collision audit.</summary>
    internal const ushort AuditMissileType = 0x8100;
    /// <summary>Nonzero instruction sentinel; this staged projectile only enters collision, not animation.</summary>
    internal const ushort AuditLiveInstruction = 0x9000;
}
