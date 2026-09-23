using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports full-body hurt/intro RGB5 artwork, not hurt-counter logic.</summary>
public static class SamusHurtColorExtractor
{
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

    private static PaletteRgb5[] ReadColors(ISnesAddressSpace bus, int address)
    {
        byte[] source = RomDataReader.ReadFixedBank(bus,
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
