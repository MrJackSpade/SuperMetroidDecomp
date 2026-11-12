using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks the six intro scientist OAM records against their contiguous retail source chain.</summary>
internal static class IntroScientistFrameCatalogVerification
{
    /// <summary>
    /// Verifies native record strides, part counts, names and catalog bounds: the delivered baby's three records from
    /// $8C:8CF3, then the examined baby's three, ending where the text caret's block ($8C:8D68) begins. The next-page arrow's
    /// records before them and the caret's block after them are no scientist-scene baby's (#1654).
    /// </summary>
    internal static void VerifyIntroScientistFrameCatalog(ISnesAddressSpace rom)
    {
        string[] names = ["delivered-baby-1", "delivered-baby-2", "delivered-baby-3",
            "examined-baby-1", "examined-baby-2", "examined-baby-3"];
        var frames = IntroScientistSpriteDefinitions.Frames;
        AssertEqual(names.Length, frames.Count, "scientist catalog contains six source records");
        int pointer = 0x8cf3;
        for (int index = 0; index < frames.Count; index++)
        {
            int count = LittleEndianBytes.Word(IntroScientistFrameCatalogRomByte(rom, 0x8c0000 + pointer), IntroScientistFrameCatalogRomByte(rom, 0x8c0001 + pointer));
            AssertEqual((ushort)pointer, CinematicSpritemapSources.Of(frames[index].Pose), "scientist frame follows original record chain");
            AssertEqual(count, frames[index].StockPartCount, "original scientist part count");
            AssertEqual(names[index], frames[index].Name, "published scientist asset key");
            pointer += 2 + 5 * count;
        }
        AssertEqual((ushort)pointer, CinematicSpritemapSources.Of(IntroCaretSpriteDefinitions.Visible.Pose),
            "the scientist record chain ends where the caret's block begins");
        foreach (int invalid in new[] { int.MinValue, -1, names.Length, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => { _ = frames[invalid]; }, "scientist catalog boundary");
        AssertTrue(frames.SequenceEqual(frames.ToArray()), "scientist catalog enumeration agrees with indexing");
    }

    /// <summary>Reads a source byte for the native scientist record-chain comparison.</summary>
    private static byte IntroScientistFrameCatalogRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
