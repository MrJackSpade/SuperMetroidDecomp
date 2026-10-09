using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports full-body hurt/intro RGB5 artwork, not hurt-counter logic.</summary>
public static class SamusHurtColorExtractor
{
    /// <summary>Exports Samus's full sixteen-color hurt-flash and cinematic-intro palette images from $9B:A380 and $9B:A3A0.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for the two complete OBJ palette rows.</param>
    /// <returns>A new UTF-8 JSON buffer with separate Hurt and Intro arrays, including color zero, expressed as RGB5 channels 0..31.</returns>
    /// <remarks>Bit 15 is omitted from channel output. Hurt-counter selection, invulnerability, and cinematic flash cadence remain compiled rather than becoming editable fields.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> lacks cartridge import access.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return SamusHurtColorCatalog.Write(new SamusHurtColorDocument
        {
            Version = SamusHurtColorFormat.Version,
            Hurt = ReadColors(bus, SamusHurtColorFormat.HurtSourceAddress),
            Intro = ReadColors(bus, SamusHurtColorFormat.IntroSourceAddress),
        });
    }

    /// <summary>Decodes one complete native Samus hurt-flash palette row into RGB5 components.</summary>
    /// <param name="bus">Import address space containing the color words.</param>
    /// <param name="address">SNES CPU address of the row's first color.</param>
    /// <returns>The fixed-size palette row in native order.</returns>
    private static PaletteRgb5[] ReadColors(ISnesAddressSpace bus, int address)
    {
        byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            address, SamusHurtColorFormat.ColorsPerPalette * sizeof(ushort));
        var colors = new PaletteRgb5[SamusHurtColorFormat.ColorsPerPalette];
        for (int index = 0; index < colors.Length; index++)
        {
            ushort word = unchecked((ushort)(source[index * 2] | source[index * 2 + 1] << 8));
            colors[index] = new PaletteRgb5
            {
                Red = word & 31,
                Green = (word >> 5) & 31,
                Blue = (word >> 10) & 31,
            };
        }
        return colors;
    }
}
