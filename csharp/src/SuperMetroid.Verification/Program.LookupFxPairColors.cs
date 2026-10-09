using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    // Independent reviewed fields, not the production rule's reported coverage.
    /// <summary>
    /// Returns the RGB5 component bits independently confirmed as calculated for a native paired-color entry.
    /// </summary>
    /// <param name="id">The room-FX blend selector from the cartridge.</param>
    /// <param name="color">The primary or secondary color index in the pair.</param>
    /// <returns>A bit mask of calculated component fields; unset fields are stored natively.</returns>
    private static int OriginalFxPairCalculatedMask(byte id, int color) => color == 0 && id is (0x42 or 0xe2 or 0xee) ? 0x7fff : id switch
    {
        0x02 => 0x7c1f,
        0x22 or 0x62 => 0x7fe0,
        0x42 => 0x03e0,
        0xe8 => 0x7fe0,
        _ => 0,
    };

    /// <summary>
    /// Confirms the red RGB5 component of each calculated native pair against cartridge, catalog, and CGRAM values.
    /// </summary>
    /// <param name="rom">The address space containing original paired-color words.</param>
    /// <param name="stock">The installed palette-blend catalog under verification.</param>
    private static void VerifyFxPairRed(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxPairComponent(rom, stock, 0);

    /// <summary>
    /// Confirms the green RGB5 component of each calculated native pair against cartridge, catalog, and CGRAM values.
    /// </summary>
    /// <param name="rom">The address space containing original paired-color words.</param>
    /// <param name="stock">The installed palette-blend catalog under verification.</param>
    private static void VerifyFxPairGreen(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxPairComponent(rom, stock, 5);

    /// <summary>
    /// Confirms the blue RGB5 component of each calculated native pair against cartridge, catalog, and CGRAM values.
    /// </summary>
    /// <param name="rom">The address space containing original paired-color words.</param>
    /// <param name="stock">The installed palette-blend catalog under verification.</param>
    private static void VerifyFxPairBlue(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxPairComponent(rom, stock, 10);

    /// <summary>
    /// Checks one selected RGB5 component wherever the independent native mask identifies that component as calculated.
    /// </summary>
    /// <param name="rom">The address space containing the original paired-color values.</param>
    /// <param name="stock">The installed catalog used to resolve and apply each pair.</param>
    /// <param name="shift">The bit offset of the red, green, or blue five-bit field in a CGRAM color word.</param>
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
                0 => RoomFxPaletteBlendDefinitions.CalculatedPairRed(id, color == 0),
                5 => RoomFxPaletteBlendDefinitions.CalculatedPairGreen(id, native & 31, color == 0),
                _ => RoomFxPaletteBlendDefinitions.CalculatedPairBlue(id, native & 31, (native >> 5) & 31, color == 0),
            };
            AssertEqual(expected, calculated!.Value, "Native paired-color component rule");
            AssertEqual(expected, (stock.Resolve(id)[color] >> shift) & 31, "Installed paired-color component");
            var cgram = new SnesCgram();
            stock.Apply(cgram, id);
            AssertEqual(expected, (cgram.Colors[25 + color] >> shift) & 31, "Applied paired-color component");
        }
    }

    /// <summary>
    /// Verifies calculated fields are not redundantly stored and that explicit RGB5 edits round-trip across the full valid range.
    /// </summary>
    /// <param name="rom">The address space providing the native paired-color words used to identify derived fields.</param>
    private static void VerifyFxPairStorageAndEdits(ISnesAddressSpace rom)
    {
        foreach (byte id in OriginalFxBlendIds())
        {
            for (int color = 0; color < 2; color++)
            {
                var owner = new RoomFxPairColor(id, ReadVerificationWord(rom, 0x89aa02 + id + color * 2), color == 0);
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
                AssertEqual((ushort)word, new RoomFxPairColor(id, (ushort)word, isPrimary).CreateColor(), "All RGB5 edits round-trip");
            foreach (bool isPrimary in new[] { false, true })
            foreach (int invalid in new[] { int.MinValue, -1, 32, int.MaxValue })
            {
                AssertThrows<ArgumentOutOfRangeException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairGreen(id, invalid, isPrimary), "Green rule RGB5 bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairBlue(id, invalid, 0, isPrimary), "Blue rule RGB5 bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairBlue(id, 0, invalid, isPrimary), "Blue rule green-input bounds" );
            }
        }
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            byte id = (byte)value;
            if (OriginalFxBlendIds().Contains(id)) continue;
            foreach (bool isPrimary in new[] { false, true })
            {
                AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairRed(id, isPrimary), "Unknown red rule selector rejects");
                AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairGreen(id, 0, isPrimary), "Unknown green rule selector rejects");
                AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendDefinitions.CalculatedPairBlue(id, 0, 0, isPrimary), "Unknown blue rule selector rejects");
            }
        }
    }

    /// <summary>
    /// Masks calculated paired-color fields in cartridge reads to ensure catalog construction derives those values rather than copying them.
    /// </summary>
    /// <param name="source">The underlying cartridge address space whose original bytes are masked selectively.</param>
    private sealed class DerivedBlendSourceGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>
        /// Reads a byte and clears bits belonging to components calculated by the reviewed native blend rules.
        /// </summary>
        /// <param name="address">The cartridge address to read through the masking guard.</param>
        /// <returns>The source byte with calculated paired-color fields cleared when the address belongs to a covered pair.</returns>
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

        /// <summary>
        /// Routes an importer cartridge read through the derived-component mask.
        /// </summary>
        /// <param name="address">The cartridge address requested by the importer.</param>
        /// <returns>The byte after calculated paired-color fields have been masked.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>
        /// Forwards a write to the underlying address space without changing its address or value.
        /// </summary>
        /// <param name="address">The address receiving the write.</param>
        /// <param name="value">The byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
