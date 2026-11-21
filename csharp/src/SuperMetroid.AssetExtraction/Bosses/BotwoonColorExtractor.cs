using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Botwoon.Artwork;

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
        return BotwoonColorFile.Write(new BotwoonColorDocument
        {
            Version = BotwoonColorFormat.Version,
            Health = Rgb5Rows.KeyedInTableOrder(BotwoonSourceDefinitions.HealthTableOrder, band => band.Key, Row),
        });

        PaletteRgb5[] Row(int row)
        {
            var colours = new PaletteRgb5[BotwoonColorFormat.ColorsPerPalette];
            for (int color = 0; color < colours.Length; color++)
            {
                int address = BotwoonSourceDefinitions.HealthColors +
                    (row * BotwoonColorFormat.ColorsPerPalette + color) *
                    sizeof(ushort);
                int native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
                if (!RgbColor.IsColorWord(checked((ushort)native)))
                    throw new InvalidDataException(
                        $"Botwoon color ${address:X6} has an unrepresentable high bit.");
                colours[color] = PaletteRgb5.From(RgbColor.FromWord(checked((ushort)native)));
            }
            return colours;
        }
    }
}
