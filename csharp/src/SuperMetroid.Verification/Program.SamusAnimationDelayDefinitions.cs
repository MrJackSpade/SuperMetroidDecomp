using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the entire compiled pose-delay definition domain against retail.</summary>
    private static void VerifySamusAnimationDelayDefinitions(ISnesAddressSpace bus,
        string sourceRom)
    {
        for (int pose = 0; pose < 256; pose++)
        {
            int source = SamusAnimationDelayDefinitions.PointerTableAddress + pose * 2;
            ushort nativePointer = (ushort)(bus.ReadByte(source) |
                bus.ReadByte(source + 1) << 8);
            AssertEqual(nativePointer,
                SamusAnimationDelayDefinitions.PointerForPose((SamusPoseId)pose),
                $"compiled Samus animation pointer for pose ${pose:X2}");
        }
        for (int source = SamusAnimationDelayDefinitions.PointerTableAddress;
             source < SamusAnimationDelayDefinitions.DelayStreamsEndExclusive; source++)
            AssertEqual(bus.ReadByte(source),
                SamusAnimationDelayDefinitions.ReadCompiledByte(source),
                $"compiled Samus animation definition byte ${source:X6}");

        // Semantic layout: contiguous segments, each ending in one command, and every real
        // pose pointer selecting a segment start rather than a delay inside one.
        ReadOnlySpan<SamusAnimationSegment> segments = SamusAnimationDelayPrograms.Segments;
        AssertEqual(160, segments.Length, "Samus animation segment count");
        int expectedAddress = SamusAnimationDelayDefinitions.DelayStreamsAddress & ushort.MaxValue;
        var segmentStarts = new HashSet<int>();
        foreach (SamusAnimationSegment segment in segments)
        {
            AssertEqual(expectedAddress, (int)segment.Address, "Samus animation segments are contiguous");
            segmentStarts.Add(segment.Address);
            expectedAddress += segment.Length;
        }
        AssertEqual(SamusAnimationDelayDefinitions.DelayStreamsEndExclusive & ushort.MaxValue, expectedAddress,
            "Samus animation segments end at the running-cadence pointer");
        for (int pose = 0; pose < 0xFD; pose++)
            AssertTrue(segmentStarts.Contains(SamusAnimationDelayDefinitions.PointerForPose((SamusPoseId)pose)),
                $"pose ${pose:X2} animation pointer starts a segment");

        // Poses $FD-$FF intentionally overread the first delay bytes as $0302.
        // The resulting low-bank source is live WRAM, not immutable ROM data.
        var mutableBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        mutableBus.WriteByte(0x7E0302, 0x5A);
        AssertEqual((byte)0x5A,
            SamusAnimationDelayDefinitions.ReadAnimationByte(mutableBus,
                SamusAnimationDelayDefinitions.PointerForPose((SamusPoseId)0xFD), 0),
            "invalid-pose animation overread keeps its mutable WRAM alias");
        var flashbackSamus = new SamusState
        {
            Pose = SamusPoseId.FacingLeftNormalPose,
        };
        flashbackSamus.InitializeAnimation(new FrontendCartridgeReadGuard(mutableBus));
        AssertEqual((int)(0x910000 |
            SamusAnimationDelayDefinitions.PointerForPose(SamusPoseId.FacingLeftNormalPose)),
            flashbackSamus.AnimationDelayListAddress,
            "Mother Brain flashback pose initializes through compiled animation timing");
        AssertThrows<InvalidDataException>(
            () => SamusAnimationDelayDefinitions.ReadAnimationByte(mutableBus,
                0xB5D1, 0),
            "uncompiled immutable pose-delay addresses fail explicitly");
        Console.WriteLine("Samus pose animation: 256 pointers and every stream byte match retail; mutable overread remains live.");
    }
}
