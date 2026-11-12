using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks the nineteen intro egg and confused-baby frame names, pointers, and part counts.</summary>
internal static class IntroDiscoveryFrameCatalogVerification
{
    /// <summary>
    /// Walks native spritemap records and confirms catalog order, asset keys, and index bounds: the egg's sixteen records
    /// from $8C:8D6F, then the confused baby's three from $8C:8FCB. The large asteroids' record after them ($8C:909D) is the
    /// Ceres destruction's, not the baby's (#1645).
    /// </summary>
    internal static void VerifyIntroDiscoveryFrameCatalog(ISnesAddressSpace rom)
    {
        string[] names = ["egg-intact", "egg-crack-1", "egg-crack-2", "egg-crack-3",
            "egg-crack-4", "egg-crack-5", "egg-crack-6", "egg-crack-7", "egg-hatched",
            "egg-remnant-1", "egg-remnant-2", "egg-remnant-3", "egg-remnant-4",
            "egg-remnant-5", "egg-remnant-6", "egg-remnant-7",
            "confused-baby-1", "confused-baby-2", "confused-baby-3"];
        var frames = IntroDiscoveryActorSpriteDefinitions.Frames;
        AssertEqual(names.Length, frames.Count, "discovery catalog original frame count");
        int pointer = 0x8d6f;
        for (int index = 0; index < frames.Count; index++)
        {
            if (index == 16)
            {
                AssertEqual(0x8f7e, pointer, "egg record chain end");
                pointer = 0x8fcb;
            }
            int count = LittleEndianBytes.Word(IntroDiscoveryFrameCatalogRomByte(rom, 0x8c0000 + pointer), IntroDiscoveryFrameCatalogRomByte(rom, 0x8c0001 + pointer));
            AssertEqual((ushort)pointer, CinematicSpritemapSources.Of(frames[index].Pose), "discovery original record pointer");
            AssertEqual(count, frames[index].StockPartCount, "discovery original part count");
            AssertEqual(names[index], frames[index].Name, "discovery published asset key");
            pointer += 2 + 5 * count;
        }
        AssertEqual(0x8fe0, pointer, "confused baby record chain end");
        foreach (int invalid in new[] { int.MinValue, -1, names.Length, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => { _ = frames[invalid]; }, "discovery catalog bounds");
        AssertTrue(frames.SequenceEqual(frames.ToArray()), "discovery enumeration agrees with indexing");
    }

    /// <summary>Reads a byte of a bank-$8C frame record.</summary>
    private static byte IntroDiscoveryFrameCatalogRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
