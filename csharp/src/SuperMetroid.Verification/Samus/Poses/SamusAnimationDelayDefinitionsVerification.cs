using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Samus.Movement.RunningCadenceVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Poses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled Samus pose delay streams and their intentional mutable-WRAM overread alias.</summary>
internal static class SamusAnimationDelayDefinitionsVerification
{
    /// <summary>Checks the entire compiled pose-delay definition domain against retail.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus pose-delay definitions and the invalid-pose $0302 low-WRAM overread")]
    internal static void VerifySamusAnimationDelayDefinitions(ISnesAddressSpace bus)
    {
        for (int pose = 0; pose < 256; pose++)
        {
            int source = SamusAnimationDelayDefinitions.PointerTableAddress + pose * 2;
            ushort nativePointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(source);
            AssertEqual(nativePointer,
                SamusAnimationDelayDefinitions.PointerForPose((SamusPoseId)pose),
                $"compiled Samus animation pointer for pose ${pose:X2}");
        }
        for (int source = SamusAnimationDelayDefinitions.PointerTableAddress;
             source < SamusAnimationDelayDefinitions.DelayStreamsEndExclusive; source++)
            AssertEqual(bus.ReadCartridgeByte(source),
                SamusAnimationDelayDefinitions.ReadCompiledByte(source),
                $"compiled Samus animation definition byte ${source:X6}");

        // Semantic layout: contiguous segments, each ending in one command, and every real
        // pose pointer selecting a segment start rather than a delay inside one.
        ReadOnlySpan<SamusAnimationSegment> segments = SamusAnimationDelayPrograms.Segments;
        AssertEqual(160, segments.Length, "Samus animation segment count");
        int streamsStart = SamusAnimationDelayDefinitions.DelayStreamsAddress & ushort.MaxValue;
        int expectedOffset = 0;
        var segmentStarts = new HashSet<int>();
        foreach (SamusAnimationSegment segment in segments)
        {
            AssertEqual(expectedOffset, segment.Offset, "Samus animation segments are contiguous");
            segmentStarts.Add(streamsStart + segment.Offset);
            expectedOffset += segment.Length;
        }
        AssertEqual(SamusAnimationDelayDefinitions.DelayStreamsEndExclusive - SamusAnimationDelayDefinitions.DelayStreamsAddress,
            expectedOffset, "Samus animation segments end at the running-cadence pointer");
        for (int pose = 0; pose < 0xFD; pose++)
            AssertTrue(segmentStarts.Contains(SamusAnimationDelayDefinitions.PointerForPose((SamusPoseId)pose)),
                $"pose ${pose:X2} animation pointer starts a segment");

        // Poses $FD-$FF intentionally overread the first delay bytes as $0302.
        // The resulting low-bank source is the Mode 7 queue residue, not immutable ROM data.
        var residue = new SuperMetroid.Core.Hardware.Mode7TransferQueueResidue();
        residue.Write(0x7E0302 - SuperMetroid.Core.Hardware.Mode7TransferQueueLayout.QueueStart, 0x5A);
        AssertEqual((byte)0x5A,
            SamusAnimationDelayDefinitions.ReadAnimationByte(residue,
                SamusAnimationDelayDefinitions.PointerForPose((SamusPoseId)0xFD), 0),
            "invalid-pose animation overread keeps its mutable WRAM alias");
        var flashbackSamus = new SamusState
        {
            Pose = SamusPoseId.FacingLeftNormalPose,
        };
        SamusPresentation.InitializeAnimation(flashbackSamus);
        AssertEqual(SamusAnimationDelayDefinitions.ListForPose(SamusPoseId.FacingLeftNormalPose),
            flashbackSamus.AnimationDelayList,
            "Mother Brain flashback pose initializes through compiled animation timing");
        AssertThrows<InvalidDataException>(
            () => SamusAnimationDelayDefinitions.ReadAnimationByte(residue,
                0xB5D1, 0),
            "uncompiled immutable pose-delay addresses fail explicitly");
        Console.WriteLine("Samus pose animation: 256 pointers and every stream byte match retail; mutable overread remains live.");
    }

    /// <summary>Runs pose-delay and running-cadence checks against the retail ROM.</summary>
    [VerificationCommand("--mutable-animation-aliases")]
    internal static int MutableAnimationAliasesCommand()
    {
        string sourceRom = Path.GetFullPath("Super Metroid.smc");
        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        Suite(nameof(VerifySamusAnimationDelayDefinitions), () => VerifySamusAnimationDelayDefinitions(bus));
        Suite(nameof(VerifyRunningCadence), () => VerifyRunningCadence(bus));
        return 0;
    }
}
