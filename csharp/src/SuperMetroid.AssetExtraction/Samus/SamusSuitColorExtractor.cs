using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Exports the three ordinary suit palettes; selection and palette timing stay compiled.</summary>
[CpuBusBoundary("codec: ROM ordinary suit palettes → Samus suit colors")]
public static class SamusSuitColorExtractor
{
    /// <summary>Exports the ordinary Power, Varia, and Gravity suit OBJ palette rows, not suit-up effects or transient palette cycles.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for $9B:9400, $9B:9520, and $9B:9800 respectively.</param>
    /// <returns>A new UTF-8 JSON buffer with three named sixteen-color arrays including color zero; each RGB5 channel is 0..31.</returns>
    /// <remarks>Only lower-fifteen-bit color channels are represented. Equipped-suit priority, CGRAM installation, and palette timing remain runtime mechanics.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> lacks cartridge import access.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        return SamusSuitColorCatalog.Write(new SamusSuitColorDocument
        {
            Version = SamusSuitColorFormat.Version,
            Power = Read(SamusCartridgeTables.PowerSuitPalette),
            Varia = Read(SamusCartridgeTables.VariaSuitPalette),
            Gravity = Read(SamusCartridgeTables.GravitySuitPalette),
        });

        PaletteRgb5[] Read(int address)
        {
            byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), address,
                SamusSuitColorFormat.ColorsPerSuit * sizeof(ushort));
            var result = new PaletteRgb5[SamusSuitColorFormat.ColorsPerSuit];
            for (int index = 0; index < result.Length; index++)
            {
                ushort color = SnesLittleEndian.Word(source[index * 2], source[index * 2 + 1]);
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
