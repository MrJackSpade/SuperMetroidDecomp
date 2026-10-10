using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    // Original public selector domain/order before conversion at11df17c7.
    private static byte[] OriginalFxBlendIds() => [0x02, 0x22, 0x42, 0x48, 0x62, 0xe2, 0xe8, 0xee];

    private static void VerifyFxBlendSelectorIdentities()
    {
        byte[] original = OriginalFxBlendIds();
        AssertTrue(original.SequenceEqual(RoomFxPaletteBlendDefinitions.Ids.Select(id => (byte)id)), "Original blend selector order/domain");
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            byte id = (byte)value;
            if (original.Contains(id))
                AssertEqual($"blend-{id:X2}", RoomFxPaletteBlendDefinitions.Key((RoomFxPaletteBlend)id), "Original document key");
            else AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendDefinitions.Key((RoomFxPaletteBlend)id), "Unknown selector key rejects");
        }
    }

    private static void VerifyFxBlendSourceAddresses(ISnesAddressSpace rom)
    {
        // Original LDA.l operand at89:AB5F supplies the color-one base.
        int nativeBase = rom.ReadByte(0x89ab5f) | rom.ReadByte(0x89ab60) << 8 | rom.ReadByte(0x89ab61) << 16;
        AssertEqual(0x89aa02, nativeBase, "Pinned native blend load operand");
        foreach (int value in Enumerable.Range(0, 256))
        {
            byte id = (byte)value;
            if (OriginalFxBlendIds().Contains(id))
                AssertEqual(nativeBase + id, RoomFxPaletteBlendDefinitions.SourceAddress((RoomFxPaletteBlend)id), "Original indexed color address");
            else AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendDefinitions.SourceAddress((RoomFxPaletteBlend)id), "Noncatalogued source rejects");
        }
    }

    private static void VerifyFxBlendPageDispatch(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock)
    {
        var document = new RoomFxPaletteBlendDocument { Version = 1, Blends = new() };
        byte[] ids = OriginalFxBlendIds();
        for (int index = 0; index < ids.Length; index++)
            document.Blends.Add($"blend-{ids[index]:X2}", Enumerable.Range(0, 3)
                .Select(color => new PaletteRgb5 { Red = index, Green = color, Blue = 31 - index }).ToArray());
        var edited = RoomFxPaletteBlendCatalog.Load(new MemoryStream(RoomFxPaletteBlendCatalog.Write(document)));
        foreach (bool useEdited in new[] { false, true })
        {
            var catalog = useEdited ? edited : stock;
            for (int value = 0; value <= byte.MaxValue; value++)
            {
                byte id = (byte)value;
                int selectionIndex = Array.IndexOf(ids, id);
                var cgram = new SnesCgram();
                cgram.SetColor(25, Bgr555.FromWord(0x1234)); cgram.SetColor(26, Bgr555.FromWord(0x2345)); cgram.SetColor(27, Bgr555.FromWord(0x3456));
                if (selectionIndex >= 0)
                {
                    var colors = catalog.Resolve((RoomFxPaletteBlend)id);
                    AssertEqual(3, colors.Length, "Exactly three loaded colors");
                    catalog.Apply(cgram, (RoomFxPaletteBlend)id);
                    for (int color = 0; color < 3; color++)
                    {
                        // Calculated stock black components have their own native proofs.
                        if (!useEdited && color == 2 && id is not (0x22 or 0x62)) continue;
                        ushort expected = useEdited ? (ushort)(selectionIndex | color << 5 | (31 - selectionIndex) << 10)
                            : ReadVerificationWord(rom, 0x89aa02 + id + 2 * color);
                        int mask = useEdited ? 0x7fff : color < 2 ? 0x7fff & ~OriginalFxPairCalculatedMask(id, color) : 0x001f;
                        AssertEqual(expected & mask, colors[color].ToWord() & mask, "Native or independently edited selector content" );
                        AssertEqual(expected & mask, cgram.Colors[25 + color].ToWord() & mask, "Apply uses selected colors");
                    }
                }
                else
                {
                    AssertThrows<InvalidDataException>(() => { _ = catalog.Resolve((RoomFxPaletteBlend)id); }, "Unknown resolve selector rejects including zero");
                    if (id == 0)
                    {
                        catalog.Apply(cgram, (RoomFxPaletteBlend)id);
                        AssertEqual((ushort)0x1234, cgram.Colors[25], "Zero preserves color25");
                        AssertEqual((ushort)0x2345, cgram.Colors[26], "Zero preserves color26");
                        AssertEqual((ushort)0, cgram.Colors[27], "Zero clears color27 only");
                    }
                    else AssertThrows<InvalidDataException>(() => catalog.Apply(cgram, (RoomFxPaletteBlend)id), "Unknown apply selector rejects");
                }
            }
        }
        foreach (byte id in ids)
        {
            string key = $"blend-{id:X2}";
            var colors = document.Blends[key];
            document.Blends.Remove(key);
            document.Blends.Add("unknown", colors);
            AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendCatalog.Write(document), "Each required name is independently validated");
            document.Blends.Remove("unknown");
            document.Blends.Add(key, colors);
        }
    }
}
