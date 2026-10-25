using SuperMetroid.AssetExtraction.Frontend.Cinematics;
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
            ushort pointer = SnesLittleEndian.Word(IntroMotherBrainFrameCatalogRomByte(rom, operand), IntroMotherBrainFrameCatalogRomByte(rom, operand + 1));
            AssertEqual(pointer, CinematicSpritemapSources.Of(frames[frame].Pose), "original Mother Brain frame operand");
            int count = SnesLittleEndian.Word(IntroMotherBrainFrameCatalogRomByte(rom, 0x8c0000 + pointer), IntroMotherBrainFrameCatalogRomByte(rom, 0x8c0001 + pointer));
            AssertEqual(count, IntroMotherBrainSpriteDefinitions.StockPartCount, "original nine-part frame");
            AssertEqual($"mother-brain-frame-{frame}", frames[frame].Name, "published frame key");
        }
        AssertEqual(frames[1].Pose, IntroMotherBrainSpriteDefinitions.FramePose(1), "second frame pose");
        AssertEqual(frames[2].Pose, IntroMotherBrainSpriteDefinitions.FramePose(2), "third frame pose");
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => { _ = frames[invalid]; }, "frame catalog bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => IntroMotherBrainSpriteDefinitions.FramePose(invalid), "frame pointer bounds");
        }
        AssertTrue(frames.SequenceEqual(frames.ToArray()), "frame enumeration matches indexing");
    }

    /// <summary>Reads a native catalog byte for the frame-pointer and part-count comparisons.</summary>
    private static byte IntroMotherBrainFrameCatalogRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
