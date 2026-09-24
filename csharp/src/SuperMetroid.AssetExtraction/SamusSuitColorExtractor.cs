using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the three ordinary suit palettes; selection and palette timing stay compiled.</summary>
public static class SamusSuitColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return SamusSuitColorCatalog.Write(new SamusSuitColorDocument
        {
            Version = SamusSuitColorFormat.Version,
            Power = Read(SamusRenderingRomData.Body.PowerSuitPalette),
            Varia = Read(SamusRenderingRomData.Body.VariaSuitPalette),
            Gravity = Read(SamusRenderingRomData.Body.GravitySuitPalette),
        });

        PaletteRgb5[] Read(int address)
        {
            byte[] source = RomDataReader.ReadFixedBank(bus, address,
                SamusSuitColorFormat.ColorsPerSuit * sizeof(ushort));
            var result = new PaletteRgb5[SamusSuitColorFormat.ColorsPerSuit];
            for (int index = 0; index < result.Length; index++)
            {
                ushort color = (ushort)(source[index * 2] | source[index * 2 + 1] << 8);
                result[index] = new PaletteRgb5
                {
                    Red = color & 31,
                    Green = color >> 5 & 31,
                    Blue = color >> 10 & 31,
                };
            }
            return result;
        }
    }
}
