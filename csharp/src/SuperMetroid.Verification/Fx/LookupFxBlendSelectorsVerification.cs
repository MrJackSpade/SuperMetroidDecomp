using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx;

using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks the eight FX blend selectors, cartridge source addresses, and CGRAM application.</summary>
internal static class LookupFxBlendSelectorsVerification
{
    /// <summary>Preserves the original public selector domain and order from before the palette conversion.</summary>
    internal static byte[] OriginalFxBlendIds() => [0x02, 0x22, 0x42, 0x48, 0x62, 0xe2, 0xe8, 0xee];

    /// <summary>Checks selector ordering, document keys, and rejection of all unknown byte values.</summary>
    internal static void VerifyFxBlendSelectorIdentities()
    {
        byte[] original = OriginalFxBlendIds();
        // #1740: each installed key names the tint, in the selectors' original order, not the colour-table offset.
        string[] tintKeys =
            ["lava", "landing-site-rain", "indigo-maridia-water", "water-and-acid", "fog", "teal-maridia-water", "green-maridia-water", "purple-maridia-water"];
        AssertTrue(original.SequenceEqual(RoomFxPaletteBlendDocumentRules.Ids.Select(id => (byte)SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.PaletteBlends.Word(id))), "Original blend selector order/domain");
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            byte id = (byte)value;
            if (original.Contains(id))
                AssertEqual(tintKeys[Array.IndexOf(original, id)], RoomFxPaletteBlendDocumentRules.Key(FxNativeWords.PaletteBlend(id)), "Document key names the blend's tint");
            else AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendDocumentRules.Key(FxNativeWords.PaletteBlend(id)), "Unknown selector key rejects");
        }
    }

    /// <summary>Checks each indexed source against the native long-load operand and rejects uncatalogued selectors.</summary>
    internal static void VerifyFxBlendSourceAddresses(ISnesAddressSpace rom)
    {
        // Original LDA.l operand at89:AB5F supplies the color-one base.
        int nativeBase = (int)new SnesAddress(FxBlendSourceAddressesRomByte(rom, 0x89ab61), LittleEndianBytes.Word(FxBlendSourceAddressesRomByte(rom, 0x89ab5f), FxBlendSourceAddressesRomByte(rom, 0x89ab60)));
        AssertEqual(0x89aa02, nativeBase, "Pinned native blend load operand");
        foreach (int value in Enumerable.Range(0, 256))
        {
            byte id = (byte)value;
            if (OriginalFxBlendIds().Contains(id))
                AssertEqual(nativeBase + id, RoomFxLayer3TilemapSources.PaletteBlend(FxNativeWords.PaletteBlend(id)).Value, "Original indexed color address");
            else AssertThrows<InvalidDataException>(() => RoomFxLayer3TilemapSources.PaletteBlend(FxNativeWords.PaletteBlend(id)).Value, "Noncatalogued source rejects");
        }
    }

    /// <summary>Reads one retail operand byte for the native blend-source address comparison.</summary>
    private static byte FxBlendSourceAddressesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks stock three-color blends in CGRAM, including selector zero and malformed documents.</summary>
    internal static void VerifyFxBlendPageDispatch(ISnesAddressSpace rom, RoomFxBlendInks stock)
    {
        var document = new RoomFxPaletteBlendDocument
        {
            Version = RoomFxPaletteBlendDefinitions.Version,
            Blends = new(),
        };
        byte[] ids = OriginalFxBlendIds();
        for (int index = 0; index < ids.Length; index++)
            document.Blends.Add(RoomFxPaletteBlendDocumentRules.Key(FxNativeWords.PaletteBlend(ids[index])), Enumerable.Range(0, 3)
                .Select(color => new PaletteRgb5 { Red = index, Green = color, Blue = 31 - index }).ToArray());
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            byte id = (byte)value;
            if (ids.Contains(id))
            {
                IReadOnlyList<RgbColor> inks = stock.Inks(FxNativeWords.PaletteBlend(id));
                AssertEqual(3, inks.Count, "The selection loads all three layer-3 blend inks");
                for (int color = 0; color < 3; color++)
                    AssertEqual(ReadVerificationWord(rom, 0x89aa02 + id + 2 * color), inks[color].ToWord(),
                        "Layer 3's blend inks show the selection's installed native colors");
            }
            else if (id == 0)
            {
                IReadOnlyList<RgbColor> inks = stock.Inks(FxNativeWords.PaletteBlend(id));
                AssertEqual(1, inks.Count, "Zero loads only the third blend ink, leaving the first two as they were");
                AssertEqual(RgbColor.Black, inks[0], "Zero clears the third blend ink to black");
            }
            else AssertThrows<InvalidDataException>(() => { _ = stock.Inks(FxNativeWords.PaletteBlend(id)); }, "Unknown ink selector rejects");
        }
        foreach (byte id in ids)
        {
            string key = RoomFxPaletteBlendDocumentRules.Key(FxNativeWords.PaletteBlend(id));
            var colors = document.Blends[key];
            document.Blends.Remove(key);
            document.Blends.Add("unknown", colors);
            AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendFile.Read(new MemoryStream(RoomFxPaletteBlendFile.Write(document), writable: false)), "Reading rejects each missing required name independently");
            document.Blends.Remove("unknown");
            document.Blends.Add(key, colors);
        }
    }
}
