using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Fx.LookupFxPairColorsVerification;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx;

using SuperMetroid.Core.Fx;

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
        AssertTrue(original.SequenceEqual(RoomFxPaletteBlendDocumentRules.Ids.Select(id => (byte)SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.PaletteBlends.Word(id))), "Original blend selector order/domain");
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            byte id = (byte)value;
            if (original.Contains(id))
                AssertEqual($"blend-{id:X2}", RoomFxPaletteBlendDocumentRules.Key(FxNativeWords.PaletteBlend(id)), "Original document key");
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

    /// <summary>Checks stock and edited three-color blends in CGRAM, including selector zero and malformed documents.</summary>
    internal static void VerifyFxBlendPageDispatch(ISnesAddressSpace rom, RoomFxBlendInks stock)
    {
        var document = new RoomFxPaletteBlendDocument { Version = 1, Blends = new() };
        byte[] ids = OriginalFxBlendIds();
        for (int index = 0; index < ids.Length; index++)
            document.Blends.Add($"blend-{ids[index]:X2}", Enumerable.Range(0, 3)
                .Select(color => new PaletteRgb5 { Red = index, Green = color, Blue = 31 - index }).ToArray());
        var edited = RoomFxPaletteBlendFile.Read(new MemoryStream(RoomFxPaletteBlendFile.Write(document)));
        foreach (bool useEdited in new[] { false, true })
        {
            var catalog = useEdited ? edited : stock;
            for (int value = 0; value <= byte.MaxValue; value++)
            {
                byte id = (byte)value;
                int selectionIndex = Array.IndexOf(ids, id);
                if (selectionIndex >= 0)
                {
                    var colors = catalog.Resolve(FxNativeWords.PaletteBlend(id));
                    AssertEqual(3, colors.Length, "Exactly three loaded colors");
                    IReadOnlyList<RgbColor> inks = catalog.Inks(FxNativeWords.PaletteBlend(id));
                    AssertEqual(3, inks.Count, "The selection loads all three layer-3 blend inks");
                    for (int color = 0; color < 3; color++)
                    {
                        // Calculated stock black components have their own native proofs.
                        if (!useEdited && color == 2 && id is not (0x22 or 0x62)) continue;
                        ushort expected = useEdited ? (ushort)(selectionIndex | color << 5 | (31 - selectionIndex) << 10)
                            : ReadVerificationWord(rom, 0x89aa02 + id + 2 * color);
                        int mask = useEdited ? 0x7fff : color < 2 ? 0x7fff & ~OriginalFxPairCalculatedMask(id, color) : 0x001f;
                        AssertEqual(expected & mask, colors[color].ToWord() & mask, "Native or independently edited selector content" );
                        AssertEqual(expected & mask, inks[color].ToWord() & mask, "Layer 3's blend inks show the selected colors");
                    }
                }
                else
                {
                    AssertThrows<InvalidDataException>(() => { _ = catalog.Resolve(FxNativeWords.PaletteBlend(id)); }, "Unknown resolve selector rejects including zero");
                    if (id == 0)
                    {
                        IReadOnlyList<RgbColor> inks = catalog.Inks(FxNativeWords.PaletteBlend(id));
                        AssertEqual(1, inks.Count, "Zero loads only the third blend ink, leaving the first two as they were");
                        AssertEqual(RgbColor.Black, inks[0], "Zero clears the third blend ink to black");
                    }
                    else AssertThrows<InvalidDataException>(() => { _ = catalog.Inks(FxNativeWords.PaletteBlend(id)); }, "Unknown ink selector rejects");
                }
            }
        }
        foreach (byte id in ids)
        {
            string key = $"blend-{id:X2}";
            var colors = document.Blends[key];
            document.Blends.Remove(key);
            document.Blends.Add("unknown", colors);
            AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendFile.Write(document), "Each required name is independently validated");
            document.Blends.Remove("unknown");
            document.Blends.Add(key, colors);
        }
    }
}
