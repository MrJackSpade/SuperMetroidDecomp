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
                SamusAnimationDelayDefinitions.PointerForPose((byte)pose),
                $"compiled Samus animation pointer for pose ${pose:X2}");
        }
        for (int source = SamusAnimationDelayDefinitions.PointerTableAddress;
             source < SamusAnimationDelayDefinitions.DelayStreamsEndExclusive; source++)
            AssertEqual(bus.ReadByte(source),
                SamusAnimationDelayDefinitions.ReadCompiledByte(source),
                $"compiled Samus animation definition byte ${source:X6}");

        // Poses $FD-$FF intentionally overread the first delay bytes as $0302.
        // The resulting low-bank source is live WRAM, not immutable ROM data.
        var mutableBus = SuperMetroidAddressSpace.LoadRetailRom(sourceRom);
        mutableBus.WriteByte(0x7E0302, 0x5A);
        AssertEqual((byte)0x5A,
            SamusAnimationDelayDefinitions.ReadAnimationByte(mutableBus,
                SamusAnimationDelayDefinitions.PointerForPose(0xFD), 0),
            "invalid-pose animation overread keeps its mutable WRAM alias");
        var flashbackSamus = new SamusState
        {
            Pose = SamusPoseIds.FacingLeftNormalPose,
        };
        flashbackSamus.InitializeAnimation(new FrontendCartridgeReadGuard(mutableBus));
        AssertEqual((int)(0x910000 |
            SamusAnimationDelayDefinitions.PointerForPose(SamusPoseIds.FacingLeftNormalPose)),
            flashbackSamus.AnimationDelayListAddress,
            "Mother Brain flashback pose initializes through compiled animation timing");
        AssertThrows<InvalidDataException>(
            () => SamusAnimationDelayDefinitions.ReadAnimationByte(mutableBus,
                0xB5D1, 0),
            "uncompiled immutable pose-delay addresses fail explicitly");
        Console.WriteLine("Samus pose animation: 256 pointers and every stream byte match retail; mutable overread remains live.");
    }
}
