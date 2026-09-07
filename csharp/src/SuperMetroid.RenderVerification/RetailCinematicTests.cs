using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class RetailCinematicTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        byte[] rom = File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc"));
        var legacy = new IntroCinematicState(new SuperMetroidAddressSpace(rom));
        var captured = new IntroCinematicState(new SuperMetroidAddressSpace(rom));
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
            var packet = new RenderFrameSnapshot(new(tick + 1, 1, (ushort)tick), scene);
            if (sample)
            {
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: intro {legacy.Phase}, tick {tick}");
                count++;
            }
            ushort input = tick % 47 == 0 ? (ushort)SnesButton.A : (ushort)0;
            legacy.Step(input); captured.Step(input);
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
