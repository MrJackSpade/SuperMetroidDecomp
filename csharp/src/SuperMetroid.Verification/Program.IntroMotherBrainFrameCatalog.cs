using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

/// <summary>Represents intro mother brain frame catalog verification.</summary>
internal static class IntroMotherBrainFrameCatalogVerification
{
    /// <summary>Verifies intro mother brain frame catalog.</summary>
    internal static void VerifyIntroMotherBrainFrameCatalog(ISnesAddressSpace rom)
    {
        var frames = IntroMotherBrainSpriteDefinitions.Frames;
        AssertEqual(3, frames.Count, "Mother Brain three original frames");
        for (int frame = 0; frame < 3; frame++)
        {
            int operand = 0x8bcb07 + 4 * frame;
            ushort pointer = (ushort)(SnesLittleEndian.Word(IntroMotherBrainFrameCatalogRomByte(rom, operand), IntroMotherBrainFrameCatalogRomByte(rom, operand + 1)));
            AssertEqual(pointer, frames[frame].Pointer, "original Mother Brain frame operand");
            int count = SnesLittleEndian.Word(IntroMotherBrainFrameCatalogRomByte(rom, 0x8c0000 + pointer), IntroMotherBrainFrameCatalogRomByte(rom, 0x8c0001 + pointer));
            AssertEqual(count, IntroMotherBrainSpriteDefinitions.StockPartCount, "original nine-part frame");
            AssertEqual($"mother-brain-frame-{frame}", frames[frame].Name, "published frame key");
        }
        AssertEqual(frames[1].Pointer, IntroMotherBrainSpriteDefinitions.FrameOne, "second frame alias");
        AssertEqual(frames[2].Pointer, IntroMotherBrainSpriteDefinitions.FrameTwo, "third frame alias");
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = frames[invalid]; }, "frame catalog bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => IntroMotherBrainSpriteDefinitions.FramePointer(invalid), "frame pointer bounds");
        }
        AssertTrue(frames.SequenceEqual(frames.ToArray()), "frame enumeration matches indexing");
    }

    /// <summary>Intro mother brain frame catalog rom byte.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for intro Mother Brain frame catalog")]
    private static byte IntroMotherBrainFrameCatalogRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
