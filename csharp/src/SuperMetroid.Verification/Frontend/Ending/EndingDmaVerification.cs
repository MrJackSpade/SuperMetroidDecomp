using SuperMetroid.Core.Frontend;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Frontend.Ending.EndingSnapshotTestsVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Ending.EndingCloudPlacementVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingExplosionSlotOrderVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingRewardGestureVerification;
using static SuperMetroid.Verification.Rendering.ObjSubscreenVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Verification.Frontend.Ending;
using SuperMetroid.Verification.Harness;

/// <summary>Checks ending Mode 7 and reward artwork DMA against retail VRAM lane placement.</summary>
internal static class EndingDmaVerification
{
    /// <summary>Confirms escape artwork uses split VRAM lanes and reward artwork uses contiguous bytes.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for ending escape and reward VRAM DMA")]
    internal static void VerifyEndingDma()
    {
        Suite(nameof(VerifyObjSubscreenAddition), () => VerifyObjSubscreenAddition());
        Suite(nameof(VerifyEndingRewardGesture), () => VerifyEndingRewardGesture());
        Suite(nameof(VerifyEndingCloudPlacement), () => VerifyEndingCloudPlacement());
        Suite(nameof(VerifyEndingExplosionSlotOrder), () => VerifyEndingExplosionSlotOrder());
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var audio = new AudioQueues();
        var ending = CreateRetailEndingFixture(audio, 0, 0);
        // The setup's NMI waits each end a dispatcher call; the call after the last runs the scene setup.
        for (int call = 0; call <= EndingSequenceDefinitions.SetupFrameWaits; call++)
            ending.Step();
        AssertEqual(EndingCreditsPhase.WaitForEscapeMusic, ending.Stage.Phase, "ending setup completes after its NMI waits");
        var projection = EndingCreditsPresentation.CaptureRenderSnapshot(ending).LayersOf<SuperMetroid.Core.Rendering.Mode7RenderLayer>().Single().Transform;
        AssertEqual((short)0, projection.Scroll.X, "native atmospheric scroll X");
        AssertEqual((short)0, projection.Scroll.Y, "native atmospheric scroll Y");
        AssertEqual((short)45, projection.MatrixA, "native initial angle 32, scale 64 cosine");
        AssertEqual((short)45, projection.MatrixB, "native initial angle 32, scale 64 sine");
        byte[] vram = ending.ShownVram();
        byte[] interleaved = RomDataReader.Decompress(bus, 0x99d17e, 0x8000);
        byte[] characters = RomDataReader.Decompress(bus, 0x98bcd6, 0x8000);
        // $8B:D4FC/D51C use mode-1 DMA to both ports; D55C then overwrites
        // only the high lane. These checks are independent of either C# renderer.
        for (int word = 0; word < 0x4000; word++)
        {
            AssertEqual(interleaved[(word * 2) % 0x4000], vram[word * 2], "ending native low-lane map DMA");
            AssertEqual(characters[word], vram[word * 2 + 1], "ending native high-lane character DMA");
        }
        Console.WriteLine("Ending escape DMA matches native low/high VRAM lanes.");
        for (int frame = 0; frame < 20000 && ending.Stage.Phase != EndingCreditsPhase.Credits; frame++)
        {
            ending.Step();
            audio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.Credits, ending.Stage.Phase, "ending reaches credits setup");
        byte[] reward = RomDataReader.Decompress(bus, 0x97b957, 0x8000);
        AssertTrue(reward.AsSpan(0, 0x4000).SequenceEqual(ending.ShownVram()[..0x4000]),
            "native post-credits reward DMA writes contiguous bytes, not the Mode-7 high lane");
    }

    /// <summary>Runs ending DMA and render-snapshot checks together.</summary>
    [VerificationCommand("--ending-dma")]
    internal static int EndingDmaCommand()
    {
        Suite(nameof(VerifyEndingDma), () => VerifyEndingDma());
        Suite(nameof(VerifyEndingRenderSnapshots), () => VerifyEndingRenderSnapshots());
        return 0;
    }
}
