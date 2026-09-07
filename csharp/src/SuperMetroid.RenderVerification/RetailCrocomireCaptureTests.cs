using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Rendering.Direct3D11;

internal static class RetailCrocomireCaptureTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(CrocomireCaptureFixture.Room, CrocomireCaptureFixture.CameraX, 0);
        var samus = runtime.Samus!;
        samus.XPosition = CrocomireCaptureFixture.InitialSamusX; samus.YPosition = 120;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
        var boss = runtime.Enemies.Crocomire ?? throw new InvalidOperationException("Crocomire did not initialize.");
        var death = runtime.Enemies.CrocomireDeath!;
        // Reuse the cartridge audit's bridge threshold, then let the complete
        // runtime execute every death state, PLM, DMA and scanline publication.
        boss.Body.XPosition = CrocomireCaptureFixture.BridgeThreshold;
        var phases = new HashSet<ushort>();
        bool distorted = false, complete = false;
        int samples = 0;
        for (int tick = 0; tick < 20000; tick++)
        {
            if (boss.DeathSequenceIndex == CrocomireDeathPhases.WaitForSamusAtWall)
                samus.XPosition = CrocomireCaptureFixture.ReturnSamusX;
            runtime.StepFrame(0);
            bool changed = phases.Add(boss.DeathSequenceIndex);
            bool nonuniform = death.Bg2ScrollByScanline.Any(y => y != death.Bg2ScrollByScanline[0]);
            bool firstDistortion = nonuniform && !distorted;
            distorted |= nonuniform;
            complete = boss.DeathSequenceIndex == CrocomireDeathPhases.InertCorpse;
            if (changed || firstDistortion || tick % 16 == 0 || complete)
            {
                var expected = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                var packet = new RenderFrameSnapshot(new(++samples, 1, (ushort)tick), GameplayDisplayCapture.TryCaptureFrame(runtime)!);
                packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: Crocomire tick={tick}, phase={boss.DeathSequenceIndex:X2}");
            }
            if (complete) break;
        }
        if (!complete || !distorted || !phases.Contains(CrocomireDeathPhases.DissolveFirstImage) ||
            !phases.Contains(CrocomireDeathPhases.DissolveSecondImage) || !phases.Contains(CrocomireDeathPhases.BreakSpikeWall))
            throw new InvalidOperationException($"Crocomire fixture incomplete: phase={boss.DeathSequenceIndex:X2}, distortion={distorted}.");
        Console.WriteLine($"{device.Kind}: {samples} exact Crocomire death samples across {phases.Count} phases, both dissolves, scanline distortion and skeleton completion.");
    }
}

internal static class CrocomireCaptureFixture
{
    /// <summary>Retail bank-$8F Crocomire room header, shared with the production death audit.</summary>
    internal const ushort Room = 0xa98d;
    /// <summary>Right arena viewport used by the retail Crocomire audit.</summary>
    internal const ushort CameraX = 0x0400;
    /// <summary>Stationary audit actor's initial world X.</summary>
    internal const ushort InitialSamusX = 0x0440;
    /// <summary>$A4:8D5E bridge threshold that starts the native death dispatcher.</summary>
    internal const ushort BridgeThreshold = 0x0640;
    /// <summary>Left-side actor position satisfying the native wall-return gate.</summary>
    internal const ushort ReturnSamusX = 0x0270;
}
