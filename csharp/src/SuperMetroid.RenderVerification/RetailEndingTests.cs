using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Compares captured GPU frames with legacy rendering throughout retail ending sequences.</summary>
internal static class RetailEndingTests
{
    /// <summary>Checks ending composition, repeated-packet stability, and scene cadence for the tested completion times.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        int samples = 0;
        foreach (ushort hours in new ushort[] { 2, 3, 10 })
        {
            var leftBus = new SuperMetroid.AssetExtraction.CartridgeImportAddressSpace(rom); var rightBus = new SuperMetroid.AssetExtraction.CartridgeImportAddressSpace(rom);
            var leftAudio = new CartridgeAudioState(); var rightAudio = new CartridgeAudioState();
            var legacy = RepositoryInstallation.CreateEnding(leftBus, leftAudio, hours, 59);
            var captured = RepositoryInstallation.CreateEnding(rightBus, rightAudio, hours, 59);
            var credits = CreditsPresentation.Load(new MemoryStream(
                SuperMetroid.AssetExtraction.CreditsPresentationExtractor.Extract(leftBus)));
            legacy.BindStaffCredits(credits);
            captured.BindStaffCredits(credits);
            var phases = new HashSet<EndingCreditsPhase>();
            for (int tick = 0; tick < 60000; tick++)
            {
                legacy.Step(); captured.Step();
                if (phases.Add(legacy.Phase) || tick % 97 == 0)
                {
                    var expected = legacy.Render();
                    var packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(
                        new RenderFrameSnapshot(new(++samples, 1, (ushort)tick), captured.CaptureRenderSnapshot())));
                    PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                        $"{device.Kind}: ending hours={hours}, tick={tick}, phase={legacy.Phase}");
                    // Rendering the same immutable packet twice must not advance the scene.
                    PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), "ending repeated GPU rendering");
                }
                leftAudio.AdvanceFrame(leftBus, default); rightAudio.AdvanceFrame(rightBus, default);
                if (legacy.Phase != captured.Phase || legacy.CinematicFrame != captured.CinematicFrame ||
                    legacy.CreditsVerticalScroll != captured.CreditsVerticalScroll)
                    throw new InvalidOperationException("GPU ending capture changed scene cadence.");
                if (legacy.Phase == EndingCreditsPhase.SeeYouNextMission) break;
            }
            if (legacy.Phase != EndingCreditsPhase.SeeYouNextMission ||
                !phases.Contains(EndingCreditsPhase.Credits) || !phases.Contains(EndingCreditsPhase.PostCreditsReward) ||
                !phases.Contains(EndingCreditsPhase.ZebesExplosionAnimation))
                throw new InvalidOperationException("GPU ending fixture missed a composition family or terminal phase.");
        }
        Console.WriteLine($"{device.Kind}: {samples} exact retail ending samples across all three rewards; repeat rendering and cadence passed.");
    }
}
