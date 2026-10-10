using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    // Independent reviewed fields, not the production rule's reported coverage.
    private static int OriginalFxPairCalculatedMask(byte id, int color) => color == 0 && id is (0x42 or 0xe2 or 0xee) ? 0x7fff : id switch
    {
        0x02 => 0x7c1f,
        0x22 or 0x62 => 0x7fe0,
        0x42 => 0x03e0,
        0xe8 => 0x7fe0,
        _ => 0,
    };

    private static void VerifyFxPairRed(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxPairComponent(rom, stock, 0);
    private static void VerifyFxPairGreen(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxPairComponent(rom, stock, 5);
    private static void VerifyFxPairBlue(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxPairComponent(rom, stock, 10);

    private static void VerifyFxPairComponent(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock, int shift)
    {
        foreach (byte id in OriginalFxBlendIds())
        for (int color = 0; color < 2; color++)
        {
            if ((OriginalFxPairCalculatedMask(id, color) & (31 << shift)) == 0) continue;
            ushort native = ReadVerificationWord(rom, 0x89aa02 + id + color * 2);
            int expected = (native >> shift) & 31;
            int? calculated = shift switch
            {
                0 => RoomFxPaletteBlendDefinitions.CalculatedPairRed((RoomFxPaletteBlend)id, color == 0),
                5 => RoomFxPaletteBlendDefinitions.CalculatedPairGreen((RoomFxPaletteBlend)id, native & 31, color == 0),
                _ => RoomFxPaletteBlendDefinitions.CalculatedPairBlue((RoomFxPaletteBlend)id, native & 31, (native >> 5) & 31, color == 0),
            };
            AssertEqual(expected, calculated!.Value, "Native paired-color component rule");
            AssertEqual(expected, (stock.Resolve((RoomFxPaletteBlend)id)[color].ToWord() >> shift) & 31, "Installed paired-color component");
            var cgram = new SnesCgram();
            stock.Apply(cgram, (RoomFxPaletteBlend)id);
            AssertEqual(expected, (cgram.Colors[25 + color].ToWord() >> shift) & 31, "Applied paired-color component");
        }
    }

    private static void VerifyFxPairStorageAndEdits(ISnesAddressSpace rom)
    {
        foreach (byte id in OriginalFxBlendIds())
        {
            for (int color = 0; color < 2; color++)
            {
                var owner = new RoomFxPairColor((RoomFxPaletteBlend)id, Bgr555.FromWord(checked((ushort)(ReadVerificationWord(rom, 0x89aa02 + id + color * 2)))), color == 0);
                foreach (var field in new[] { (Name: "redOverride", Shift: 0), (Name: "greenOverride", Shift: 5), (Name: "blueOverride", Shift: 10) })
                {
                    var info = typeof(RoomFxPairColor).GetField(field.Name, BindingFlags.Instance | BindingFlags.NonPublic)!;
                    bool stored = (OriginalFxPairCalculatedMask(id, color) & (31 << field.Shift)) == 0;
                    AssertEqual(stored, info.GetValue(owner) is not null, "Stock derived component is not stored");
                    _ = owner.CreateColor();
                    AssertEqual(stored, info.GetValue(owner) is not null, "Output creates no component cache");
                }
            }
            // Exact property: every externally valid RGB5 edit round-trips unchanged,
            // including intensities above the stock tint's range and saturated values.
            foreach (bool isPrimary in new[] { false, true })
            for (int word = 0; word < 0x8000; word++)
                AssertEqual((ushort)word, new RoomFxPairColor((RoomFxPaletteBlend)id, Bgr555.FromWord(checked((ushort)((ushort)word))), isPrimary).CreateColor(), "All RGB5 edits round-trip");
            foreach (bool isPrimary in new[] { false, true })
            foreach (int invalid in new[] { int.MinValue, -1, 32, int.MaxValue })
            {
                AssertThrows<ArgumentOutOfRangeException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairGreen((RoomFxPaletteBlend)id, invalid, isPrimary), "Green rule RGB5 bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairBlue((RoomFxPaletteBlend)id, invalid, 0, isPrimary), "Blue rule RGB5 bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairBlue((RoomFxPaletteBlend)id, 0, invalid, isPrimary), "Blue rule green-input bounds" );
            }
        }
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            byte id = (byte)value;
            if (OriginalFxBlendIds().Contains(id)) continue;
            foreach (bool isPrimary in new[] { false, true })
            {
                AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairRed((RoomFxPaletteBlend)id, isPrimary), "Unknown red rule selector rejects");
                AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairGreen((RoomFxPaletteBlend)id, 0, isPrimary), "Unknown green rule selector rejects");
                AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairBlue((RoomFxPaletteBlend)id, 0, 0, isPrimary), "Unknown blue rule selector rejects");
            }
        }
    }

    private sealed class DerivedBlendSourceGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadByte(int address)
        {
            byte original = source.ReadByte(address);
            foreach (byte id in OriginalFxBlendIds())
            {
                int delta = address - (0x89aa02 + id);
                if ((uint)delta < 4)
                    return (byte)(original & ~(OriginalFxPairCalculatedMask(id, delta / 2) >> (8 * (delta & 1))));
                if (delta is 4 or 5 && id is 0x22 or 0x62)
                    return (byte)(original & ~(0x7fe0 >> (8 * (delta & 1))));
            }
            return original;
        }
        public byte ReadCartridgeByte(int address) => ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
