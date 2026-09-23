using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports authored visor RGB5 colors without exposing their phase selectors.</summary>
public static class SamusVisorColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte[] source = RomDataReader.ReadFixedBank(bus,
            SamusVisorColorFormat.SourceAddress,
            SamusVisorColorFormat.ColorCount * sizeof(ushort));
        var colors = new PaletteRgb5[SamusVisorColorFormat.ColorCount];
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
        return SamusVisorColorCatalog.Write(new SamusVisorColorDocument
        {
            Version = SamusVisorColorFormat.Version,
            Colors = colors,
        });
    }
}
