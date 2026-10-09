using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Returns the six liquid blend selectors whose calculated third palette color is black.</summary>
    /// <returns>The original black-color FX blend IDs.</returns>
    private static byte[] OriginalBlackBlendIds() => [0x02, 0x42, 0x48, 0xe2, 0xe8, 0xee];

    /// <summary>Checks that stock black blend selectors resolve and apply a zero red component.</summary>
    /// <param name="rom">ROM address space containing the original blend-color data.</param>
    /// <param name="stock">Stock catalog used to resolve and apply the palette colors.</param>
    private static void VerifyFxBlendBlackRed(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxBlendBlackComponent(rom, stock, 0);

    /// <summary>Checks that stock black blend selectors resolve and apply a zero green component.</summary>
    /// <param name="rom">ROM address space containing the original blend-color data.</param>
    /// <param name="stock">Stock catalog used to resolve and apply the palette colors.</param>
    private static void VerifyFxBlendBlackGreen(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxBlendBlackComponent(rom, stock, 5);

    /// <summary>Checks that stock black blend selectors resolve and apply a zero blue component.</summary>
    /// <param name="rom">ROM address space containing the original blend-color data.</param>
    /// <param name="stock">Stock catalog used to resolve and apply the palette colors.</param>
    private static void VerifyFxBlendBlackBlue(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyFxBlendBlackComponent(rom, stock, 10);

    /// <summary>Compares one five-bit component of each calculated black color with ROM, catalog resolution, and CGRAM output.</summary>
    /// <param name="rom">ROM address space containing the native third-color words.</param>
    /// <param name="stock">Catalog whose resolved colors and application output are checked.</param>
    /// <param name="shift">Bit offset of the red, green, or blue component within a packed SNES color word.</param>
    private static void VerifyFxBlendBlackComponent(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock, int shift)
    {
        foreach (byte id in OriginalBlackBlendIds())
        {
            int expected = (ReadVerificationWord(rom, 0x89aa06 + id) >> shift) & 31;
            AssertEqual(expected, (RoomFxPaletteBlendDefinitions.CalculatedThirdColor(id)!.Value >> shift) & 31, "Original third-color component");
            AssertEqual(expected, (stock.Resolve(id)[2] >> shift) & 31, "Resolved third-color component");
            var cgram = new SnesCgram();
            stock.Apply(cgram, id);
            AssertEqual(expected, (cgram.Colors[27] >> shift) & 31, "Applied third-color component");
        }
    }

    /// <summary>Checks black-color calculation boundaries, ensures stock black values are derived rather than stored, and verifies edits do not mutate catalog data.</summary>
    /// <param name="stock">Stock catalog whose override storage and copied color output are inspected.</param>
    private static void VerifyFxBlendBlackStorageAndEdits(RoomFxPaletteBlendCatalog stock)
    {
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            byte id = (byte)value;
            if (OriginalBlackBlendIds().Contains(id)) AssertEqual((ushort)0,
                RoomFxPaletteBlendDefinitions.CalculatedThirdColor(id)!.Value, "Six liquid selections calculate black");
            else if (id is 0x22 or 0x62) AssertTrue(RoomFxPaletteBlendDefinitions.CalculatedThirdColor(id) is null, "Weather third color stays independent");
            else AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendDefinitions.CalculatedThirdColor(id), "Unknown calculated-color selector rejects");
        }
        var overrideField = typeof(RoomFxBlendColors).GetField("thirdOverride", BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (var field in typeof(RoomFxPaletteBlendCatalog).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                     .Where(field => field.FieldType == typeof(RoomFxBlendColors)))
        {
            var owner = (RoomFxBlendColors)field.GetValue(stock)!;
            bool weather = field.Name is "landingSiteRain" or "fog";
            AssertEqual(weather, overrideField.GetValue(owner) is not null, "Stock black values are not stored");
            _ = owner.CreateColors();
            AssertEqual(weather, overrideField.GetValue(owner) is not null, "Output creation does not cache black");
        }
        foreach (byte id in OriginalBlackBlendIds())
        foreach (ushort third in new ushort[] { 0, 1, 31, 32, 1023, 1024, 32767 })
        {
            var owner = new RoomFxBlendColors(id, 0x1234, 0x2345, third);
            var colors = owner.CreateColors();
            AssertEqual(third, colors[2], "Custom third-color components survive");
            colors[2] ^= 1;
            AssertEqual(third, owner.CreateColors()[2], "Output edits do not mutate loaded colors");
        }
    }

    /// <summary>Rejects attempts to extract calculated black third-color words while forwarding other cartridge access.</summary>
    /// <param name="source">Address space that supplies permitted reads and receives writes.</param>
    private sealed class BlackBlendSourceGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Forwards reads except the ROM bytes that would redundantly store a calculated black color.</summary>
        /// <param name="address">Cartridge address requested by the importer or runtime.</param>
        /// <returns>The wrapped address-space byte when the address is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address selects a calculated black third-color byte.</exception>
        public byte ReadByte(int address)
        {
            foreach (byte id in OriginalBlackBlendIds())
                if (address == 0x89aa06 + id || address == 0x89aa07 + id)
                    throw new InvalidOperationException("FX blend extraction read a calculated black color.");
            return source.ReadByte(address);
        }
        /// <summary>Routes importer reads through the calculated-color read check.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The wrapped byte when the address is not a calculated black color.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Forwards writes unchanged because the guard only constrains reads.</summary>
        /// <param name="address">Cartridge address to write.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
