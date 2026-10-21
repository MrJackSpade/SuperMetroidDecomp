using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks Mother Brain intro explosion instructions and frame catalog against native records.</summary>
internal static class IntroMotherBrainExplosionProgramsVerification
{
    /// <summary>Checks twelve named explosion frames, their native pointers and part counts, and lookup bounds.</summary>
    internal static void VerifyIntroMotherBrainExplosionFrameCatalog(ISnesAddressSpace rom)
    {
        string[] names = ["small-explosion-0", "small-explosion-1", "small-explosion-2", "small-explosion-3", "small-explosion-4", "small-explosion-5",
            "big-explosion-0", "big-explosion-1", "big-explosion-2", "big-explosion-3", "big-explosion-4", "big-explosion-5"];
        var frames = IntroMotherBrainExplosionSpriteDefinitions.Frames;
        AssertEqual(12, frames.Count, "original explosion frame count");
        for (int index = 0; index < frames.Count; index++)
        {
            int operand = (index < 6 ? 0x8bcdcd : 0x8bcdad) + 4 * (index % 6);
            ushort pointer = SnesLittleEndian.Word(IntroMotherBrainExplosionFrameCatalogRomByte(rom, operand), IntroMotherBrainExplosionFrameCatalogRomByte(rom, operand + 1));
            int count = SnesLittleEndian.Word(IntroMotherBrainExplosionFrameCatalogRomByte(rom, 0x8c0000 + pointer), IntroMotherBrainExplosionFrameCatalogRomByte(rom, 0x8c0001 + pointer));
            AssertEqual(pointer, CinematicSpritemapAddresses.Of(frames[index].Pose), "original explosion frame operand");
            AssertEqual(count, frames[index].StockPartCount, "original explosion part count");
            AssertEqual(names[index], frames[index].Name, "stable explosion asset name");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 12, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => { _ = frames[invalid]; }, "explosion catalog bounds");
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
        foreach (bool big in new[] { false, true })
            AssertThrows<ArgumentOutOfRangeException>(() => IntroMotherBrainExplosionSpriteDefinitions.FramePose(big, invalid), "explosion pointer bounds");
        AssertTrue(frames.SequenceEqual(frames.ToArray()), "explosion enumeration preserves indexed order");
    }

    /// <summary>Reads a native explosion frame operand or spritemap header for catalog comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for intro Mother Brain explosion frame catalog")]
    private static byte IntroMotherBrainExplosionFrameCatalogRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
