using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports Botwoon's eight complete health-band sprite palettes.</summary>
public static class BotwoonColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var bands = new PaletteRgb5[BotwoonHealthPaletteDefinitions.PaletteCount][];
        for (int band = 0; band < bands.Length; band++)
        {
            bands[band] = new PaletteRgb5[BotwoonHealthPaletteDefinitions.ColorsPerPalette];
            for (int color = 0; color < bands[band].Length; color++)
            {
                int address = BotwoonHealthPaletteDefinitions.NativePaletteAddress +
                    (band * BotwoonHealthPaletteDefinitions.ColorsPerPalette + color) *
                    sizeof(ushort);
                ushort native = RomDataReader.ReadWordFixedBank(bus, address);
                if ((native & 0x8000) != 0)
                    throw new InvalidDataException(
                        $"Botwoon color ${address:X6} has an unrepresentable high bit.");
                bands[band][color] = new PaletteRgb5
                {
                    Red = native & 31,
                    Green = native >> 5 & 31,
                    Blue = native >> 10 & 31,
                };
            }
        }
        return BotwoonColorCatalog.Write(new BotwoonColorDocument
        {
            Version = BotwoonColorFormat.Version,
            Health = bands,
        });
    }
}
