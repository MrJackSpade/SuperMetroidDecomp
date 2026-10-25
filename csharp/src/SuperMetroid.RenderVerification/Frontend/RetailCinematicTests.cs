using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.RenderVerification.Harness;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Frontend;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.Core.Audio;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Compares translated intro snapshots with direct retail-frame rendering on a D3D device.</summary>
internal static class RetailCinematicTests
{
    /// <summary>Samples phase transitions and periodic frames through Ceres flight, retaining sampled packets across advancement.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        // Each owner keeps its own music queue, which the Ceres flight waits on; the game runs
        // the queue handler after every dispatch.
        var legacyAudio = new AudioQueues();
        var capturedAudio = new AudioQueues();
        var legacy = RenderRetailFixture.CreateIntro(legacyAudio);
        var captured = RenderRetailFixture.CreateIntro(capturedAudio);
        var phases = new HashSet<IntroCinematicPhase>();
        int count = 0;
        for (int tick = 0; tick < 20000 && !legacy.CeresFlightFinished; tick++)
        {
            // Drawing owns projectile-trail side effects: keep separate live owners
            // and execute each producer once even on frames not sent to the GPU.
            var expected = legacy.Render();
            var scene = captured.CaptureTranslatedRenderSnapshot();
            bool changed = phases.Add(legacy.Phase);
            bool sample = changed || tick % 31 == 0;
            var packet = new DisplayPacket(new(tick + 1, 1, (ushort)tick), scene);
            if (sample)
            {
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: intro {legacy.Phase}, tick {tick}");
                count++;
            }
            ControllerButton input = tick % 47 == 0 ? ControllerButton.A : ControllerButton.None;
            legacy.Step(input);
            legacyAudio.AdvanceFrame(default);
            captured.Step(input);
            capturedAudio.AdvanceFrame(default);
            if (legacy.Phase != captured.Phase || legacy.MotherBrainHitCount != captured.MotherBrainHitCount ||
                legacy.FlashbackSamusX != captured.FlashbackSamusX || legacy.FlashbackSamusY != captured.FlashbackSamusY ||
                legacy.ActiveFlashbackProjectileCount != captured.ActiveFlashbackProjectileCount)
                throw new InvalidOperationException($"Intro display-owned state diverged at tick {tick}.");
            if (sample)
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: retained intro packet after tick {tick}");
        }
        if (!legacy.CeresFlightFinished || !captured.CeresFlightFinished ||
            !phases.Contains(IntroCinematicPhase.MotherBrainFlashback) ||
            !phases.Contains(IntroCinematicPhase.BabyDiscovery) ||
            !phases.Contains(IntroCinematicPhase.BabyMetroidExamination))
            throw new InvalidOperationException("Intro coverage did not reach required phases and completed flight.");
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} intro samples across {phases.Count} phases match before/after advancement.");
    }
}
