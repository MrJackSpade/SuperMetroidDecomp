using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Compares compiled intro egg-effect scripts and frame identities with the retail cartridge.</summary>
internal static class IntroEggEffectProgramsVerification
{
    /// <summary>Checks eleven published frame names, native operands, OAM part counts, and catalog bounds.</summary>
    internal static void VerifyIntroEggEffectFrameCatalog(ISnesAddressSpace rom)
    {
        string[] names = ["fragment-0", "fragment-1", "fragment-2", "fragment-3", "fragment-4", "fragment-5",
            "slime-moving", "slime-impact-0", "slime-impact-1", "slime-impact-2", "slime-impact-3"];
        var frames = IntroEggEffectSpriteDefinitions.Frames;
        AssertEqual(11, frames.Count, "eleven egg-effect frames");
        for (int index = 0; index < 11; index++)
        {
            int operand = index < 7 ? 0x8bcd3b + 8 * index : 0x8bcd73 + 4 * (index - 7);
            ushort pointer = LittleEndianBytes.Word(IntroEggEffectFrameCatalogRomByte(rom, operand), IntroEggEffectFrameCatalogRomByte(rom, operand + 1));
            AssertEqual(pointer, CinematicSpritemapSources.Of(frames[index].Pose), "original egg-effect frame operand");
            AssertEqual(names[index], frames[index].Name, "published egg-effect name");
            AssertEqual(IntroEggEffectSpriteDefinitions.StockPartCount,
                LittleEndianBytes.Word(IntroEggEffectFrameCatalogRomByte(rom, 0x8c0000 + pointer), IntroEggEffectFrameCatalogRomByte(rom, 0x8c0001 + pointer)), "original one-part egg-effect record");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 11, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => { _ = frames[invalid]; }, "egg-effect catalog boundary");
            AssertThrows<ArgumentOutOfRangeException>(() => IntroEggEffectSpritePoses.FramePose(invalid), "egg-effect pointer boundary");
        }
        AssertTrue(frames.SequenceEqual(frames.ToArray()), "egg-effect enumeration order");
    }

    /// <summary>Reads one original frame-operand or spritemap byte for the catalog comparison.</summary>
    private static byte IntroEggEffectFrameCatalogRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
