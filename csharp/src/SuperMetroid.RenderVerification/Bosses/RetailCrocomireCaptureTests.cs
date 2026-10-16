using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Bosses;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.RepositoryInstallation;

/// <summary>Checks GPU capture through Crocomire's ordinary fight and full death sequence.</summary>
internal static class RetailCrocomireCaptureTests
{
    /// <summary>Compares rendered frames across native death phases and checks when melting HDMA becomes active.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = RepositoryInstallation.CreateRuntime(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(CrocomireCaptureFixture.Room, new Position16<RoomPixels>(CrocomireCaptureFixture.CameraX, 0));
        var samus = runtime.Player.IntroducedSamus;
        samus.Position = samus.Position.WithWhole(new(CrocomireCaptureFixture.InitialSamusX, 120));
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.PoseMetadata.RefreshCollisionRadii(); samus.Presentation.InitializeAnimation(bus);
        var boss = runtime.Enemies.CrocomireBehavior.Crocomire ?? throw new InvalidOperationException("Crocomire did not initialize.");
        var death = runtime.Enemies.CrocomireBehavior.CrocomireDeath;
        // Before either melting object is spawned, BG2 must use the actor's ordinary
        // scroll register. Comparing two renderers alone missed this shared defect.
        int latchedScrollChanges = 0;
        for (int tick = 0; tick < 60; tick++)
        {
            // Crocomire AI writes the BG2HOFS/BG2VOFS shadows during the main loop; the
            // next accepted NMI uploads them with the rest of that frame's latch (#1307).
            Position16<Bg2Pixels> previousScroll = runtime.Enemies.CrocomireBehavior.CrocomireBg2Scroll;
            runtime.StepFrame(0);
            GameplayDisplayFrame published = runtime.PublishDisplayFrame();
            if (published.Latched.Bosses.CrocomireBg2Scroll != previousScroll)
                throw new InvalidOperationException($"Crocomire BG2 scroll was not published by the following NMI at tick {tick}.");
            if (previousScroll != runtime.Enemies.CrocomireBehavior.CrocomireBg2Scroll) latchedScrollChanges++;
            var scene = GameplayDisplayCapture.Capture(published);
            var baseLayer = (OrdinaryGameplayRenderLayer)scene.Layers[0];
            if (!baseLayer.Bg2LineScrolls.Vertical.IsEmpty || (death ?? throw new InvalidOperationException("Verification requires death.")).MeltingHdmaActive)
                throw new InvalidOperationException("Crocomire fight incorrectly enabled melting HDMA.");
            var packet = new RenderFrameSnapshot(new(tick + 1, 1, (ushort)tick), scene);
            PixelComparison.Verify(packet, FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame()),
                renderer.RenderForReadback(packet), $"{device.Kind}: Crocomire fight {tick}");
        }
        // Reuse the cartridge audit's bridge threshold, then let the complete
        // runtime execute every death state, PLM, DMA and scanline publication.
        boss.Body.Position = boss.Body.Position.WithWholeX(CrocomireCaptureFixture.BridgeThreshold);
        var phases = new HashSet<CrocomireDeathPhase>();
        bool distorted = false, complete = false;
        int samples = 0;
        for (int tick = 0; tick < 20000; tick++)
        {
            if (boss.DeathSequenceIndex == CrocomireDeathPhase.WaitForSamusAtWall)
                samus.Position = samus.Position.WithWholeX(CrocomireCaptureFixture.ReturnSamusX);
            runtime.StepFrame(0);
            bool shouldMelt = boss.DeathSequenceIndex is CrocomireDeathPhase.DissolveFirstImage or CrocomireDeathPhase.DissolveSecondImage;
            if ((death ?? throw new InvalidOperationException("Verification requires death.")).MeltingHdmaActive != shouldMelt)
                throw new InvalidOperationException($"Crocomire HDMA lifecycle disagrees in phase {(int)boss.DeathSequenceIndex:X2}.");
            bool changed = phases.Add(boss.DeathSequenceIndex);
            bool nonuniform = death.Bg2ScrollByScanline.Any(y => y != death.Bg2ScrollByScanline[0]);
            bool firstDistortion = nonuniform && !distorted;
            distorted |= nonuniform;
            complete = boss.DeathSequenceIndex == CrocomireDeathPhase.InertCorpse;
            if (changed || firstDistortion || tick % 16 == 0 || complete)
            {
                var expected = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame());
                var packet = new RenderFrameSnapshot(new(++samples, 1, (ushort)tick), GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));
                packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotWriter.Serialize(packet));
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: Crocomire tick={tick}, phase={(int)boss.DeathSequenceIndex:X2}");
            }
            if (complete) break;
        }
        if (!complete || !distorted || !phases.Contains(CrocomireDeathPhase.DissolveFirstImage) ||
            !phases.Contains(CrocomireDeathPhase.DissolveSecondImage) || !phases.Contains(CrocomireDeathPhase.BreakSpikeWall))
            throw new InvalidOperationException($"Crocomire fixture incomplete: phase={(int)boss.DeathSequenceIndex:X2}, distortion={distorted}.");
        Console.WriteLine($"{device.Kind}: {samples} exact Crocomire death samples across {phases.Count} phases, both dissolves, scanline distortion and skeleton completion; {latchedScrollChanges} fight frames displayed the preceding main loop's BG2 scroll.");
    }
}
