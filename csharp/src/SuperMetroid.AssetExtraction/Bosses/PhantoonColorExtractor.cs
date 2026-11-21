using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Extracts Phantoon's health, fade-out, and ship-power RGB5 targets.</summary>
public static partial class PhantoonColorExtractor
{
    /// <summary>Reads all Phantoon health bands, fade-out colors, and ship-power targets, rejects high-bit colors, and serializes their RGB5 components.</summary>
    /// <param name="bus">Supported-cartridge address space containing the complete Phantoon color tables.</param>
    /// <returns>UTF-8 JSON bytes for the versioned Phantoon color catalog.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        return PhantoonColorFile.Write(new PhantoonColorDocument
        {
            Version = PhantoonColorFormat.Version,
            HealthBands = PhantoonHealthBandDefinitions.HighestFirst.ToDictionary(band => band.Key,
                band => Read(PhantoonSourceDefinitions.HealthBandColors +
                    PhantoonSourceDefinitions.HealthBandPalettes[band] * PhantoonColorFormat.HealthBandColorCount * sizeof(ushort),
                    PhantoonColorFormat.HealthBandColorCount), StringComparer.Ordinal),
            FadeOut = Read(PhantoonSourceDefinitions.FadeOutColors,
                PhantoonColorFormat.FadeOutCount),
            PowerOn = PhantoonColorFormat.PoweredPartKeys.ToDictionary(part => part.Value,
                part => Read(PhantoonSourceDefinitions.PowerOnColors +
                    PhantoonSourceDefinitions.PowerOnGroups[part.Key] * PhantoonColorFormat.PoweredPartColourCount * sizeof(ushort),
                    PhantoonColorFormat.PoweredPartColourCount), StringComparer.Ordinal),
        });

        PaletteRgb5[] Read(int source, int count)
        {
            var colors = new PaletteRgb5[count];
            for (int color = 0; color < count; color++)
            {
                int address = source + color * sizeof(ushort);
                int native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
                if (!RgbColor.IsColorWord(checked((ushort)native)))
                    throw new InvalidDataException(
                        $"Phantoon color ${address:X6} has an unrepresentable high bit.");
                colors[color] = PaletteRgb5.From(RgbColor.FromWordIgnoringTopBit(native));
            }
            return colors;
        }
    }
}
