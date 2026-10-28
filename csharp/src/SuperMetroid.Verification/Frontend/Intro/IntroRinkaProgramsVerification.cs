using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks compiled intro Rinka instruction bytes and frame catalog against native records.</summary>
internal static class IntroRinkaProgramsVerification
{
    /// <summary>Checks three native frame operands, published names, stock part counts, and catalog bounds.</summary>
    internal static void VerifyIntroRinkaFrameCatalog(ISnesAddressSpace rom)
    {
        string[] names = ["rinka-0", "rinka-1", "rinka-2"];
        var frames = IntroRinkaSpriteDefinitions.Frames;
        AssertEqual(3, frames.Count, "three original Rinka frames");
        for (int index = 0; index < 3; index++)
        {
            int operand = 0x8bcded + 4 * index;
            ushort pointer = LittleEndianBytes.Word(IntroRinkaFrameCatalogRomByte(rom, operand), IntroRinkaFrameCatalogRomByte(rom, operand + 1));
            AssertEqual(pointer, CinematicSpritemapSources.Of(frames[index].Pose), "native Rinka frame operand");
            AssertEqual(names[index], frames[index].Name, "published Rinka asset key");
            AssertEqual(IntroRinkaSpriteDefinitions.StockPartCount,
                LittleEndianBytes.Word(IntroRinkaFrameCatalogRomByte(rom, 0x8c0000 + pointer), IntroRinkaFrameCatalogRomByte(rom, 0x8c0001 + pointer)), "native four-part Rinka record");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => { _ = frames[invalid]; }, "Rinka catalog bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => IntroRinkaSpritePoses.FramePose(invalid), "Rinka pointer bounds");
        }
        AssertTrue(frames.SequenceEqual(frames.ToArray()), "Rinka enumeration order");
    }

    /// <summary>Reads one native frame-record byte solely as a verification oracle.</summary>
    private static byte IntroRinkaFrameCatalogRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
