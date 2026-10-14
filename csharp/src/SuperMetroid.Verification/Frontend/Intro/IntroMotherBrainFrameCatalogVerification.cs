using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks the three intro Mother Brain frame identities against cartridge operands and OAM counts.</summary>
internal static class IntroMotherBrainFrameCatalogVerification
{
    /// <summary>Verifies pointers, nine-part frame sizes, aliases, enumeration order, and index bounds.</summary>
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

    /// <summary>Reads a native catalog byte for the frame-pointer and part-count comparisons.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for intro Mother Brain frame catalog")]
    private static byte IntroMotherBrainFrameCatalogRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
