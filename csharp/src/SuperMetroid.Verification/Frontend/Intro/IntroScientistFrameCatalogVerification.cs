using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks the ten intro scientist OAM records against their contiguous retail source chain.</summary>
internal static class IntroScientistFrameCatalogVerification
{
    /// <summary>Verifies native record strides, part counts, names, the final caret alias, and catalog bounds.</summary>
    internal static void VerifyIntroScientistFrameCatalog(ISnesAddressSpace rom)
    {
        string[] names = ["examined-loop-1", "examined-loop-2", "examined-loop-3",
            "delivered-baby-1", "delivered-baby-2", "delivered-baby-3",
            "examined-baby-1", "examined-baby-2", "examined-baby-3", "examined-baby-hold"];
        var frames = IntroScientistSpriteDefinitions.Frames;
        AssertEqual(10, frames.Count, "scientist catalog contains ten source records");
        int pointer = 0x8ccf;
        for (int index = 0; index < 10; index++)
        {
            int count = LittleEndianBytes.Word(IntroScientistFrameCatalogRomByte(rom, 0x8c0000 + pointer), IntroScientistFrameCatalogRomByte(rom, 0x8c0001 + pointer));
            AssertEqual((ushort)pointer, CinematicSpritemapSources.Of(frames[index].Pose), "scientist frame follows original record chain");
            AssertEqual(count, frames[index].StockPartCount, "original scientist part count");
            AssertEqual(names[index], frames[index].Name, "published scientist asset key");
            pointer += 2 + 5 * count;
        }
        AssertEqual(0x8d6f, pointer, "original record chain ends at catalog boundary");
        AssertEqual(IntroCaretSpriteDefinitions.Visible.Pose, frames[9].Pose, "last scientist record aliases the shared caret");
        foreach (int invalid in new[] { int.MinValue, -1, 10, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => { _ = frames[invalid]; }, "scientist catalog boundary");
            AssertThrows<ArgumentOutOfRangeException>(() => IntroScientistSpriteDefinitions.FramePose(invalid), "scientist pointer boundary");
        }
        AssertTrue(frames.SequenceEqual(frames.ToArray()), "scientist catalog enumeration agrees with indexing");
    }

    /// <summary>Reads a source byte for the native scientist record-chain comparison.</summary>
    private static byte IntroScientistFrameCatalogRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
