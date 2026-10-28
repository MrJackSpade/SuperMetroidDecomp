using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Exports Botwoon's eight complete health-band sprite palettes.</summary>
public static class BotwoonColorExtractor
{
    /// <summary>Reads all complete native Botwoon health-band palettes, rejects colors with the high bit set, and serializes their RGB5 components.</summary>
    /// <param name="bus">Supported-cartridge address space containing Botwoon's contiguous health palette table.</param>
    /// <returns>UTF-8 JSON bytes for the versioned Botwoon color catalog.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var bands = new PaletteRgb5[BotwoonHealthPaletteDefinitions.PaletteCount][];
        for (int band = 0; band < bands.Length; band++)
        {
            bands[band] = new PaletteRgb5[BotwoonHealthPaletteDefinitions.ColorsPerPalette];
            for (int color = 0; color < bands[band].Length; color++)
            {
                int address = BotwoonSourceDefinitions.HealthColors +
                    (band * BotwoonHealthPaletteDefinitions.ColorsPerPalette + color) *
                    sizeof(ushort);
                ushort native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
                if (!RgbColor.IsColorWord(native))
                    throw new InvalidDataException(
                        $"Botwoon color ${address:X6} has an unrepresentable high bit.");
                bands[band][color] = PaletteRgb5.From(RgbColor.FromWord(native));
            }
        }
        return BotwoonColorFile.Write(new BotwoonColorDocument
        {
            Version = BotwoonColorFormat.Version,
            Health = bands,
        });
    }
}
