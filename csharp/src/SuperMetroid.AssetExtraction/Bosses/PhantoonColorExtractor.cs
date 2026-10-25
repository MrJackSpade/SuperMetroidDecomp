using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Extracts Phantoon's health, fade-out, and ship-power RGB5 targets.</summary>
[CpuBusBoundary("codec: ROM Phantoon health, fade-out and ship-power colors → PhantoonColorCatalog")]
public static class PhantoonColorExtractor
{
    /// <summary>Reads all Phantoon health bands, fade-out colors, and ship-power targets, rejects high-bit colors, and serializes their RGB5 components.</summary>
    /// <param name="bus">Supported-cartridge address space containing the complete Phantoon color tables.</param>
    /// <returns>UTF-8 JSON bytes for the versioned Phantoon color catalog.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var bands = new PaletteRgb5[PhantoonColorDefinitions.HealthBandCount][];
        for (int band = 0; band < bands.Length; band++)
            bands[band] = Read(PhantoonSourceDefinitions.HealthBandColors +
                band * PhantoonColorDefinitions.HealthBandColorCount * sizeof(ushort),
                PhantoonColorDefinitions.HealthBandColorCount);
        return PhantoonColorFile.Write(new PhantoonColorDocument
        {
            Version = PhantoonColorFormat.Version,
            HealthBands = bands,
            FadeOut = Read(PhantoonSourceDefinitions.FadeOutColors,
                PhantoonColorDefinitions.FadeOutCount),
            PowerOn = Read(PhantoonSourceDefinitions.PowerOnColors,
                PhantoonColorDefinitions.PowerOnCount),
        });

        PaletteRgb5[] Read(int source, int count)
        {
            var colors = new PaletteRgb5[count];
            for (int color = 0; color < count; color++)
            {
                int address = source + color * sizeof(ushort);
                ushort native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
                if (!Bgr555.IsColorWord(native))
                    throw new InvalidDataException(
                        $"Phantoon color ${address:X6} has an unrepresentable high bit.");
                colors[color] = new PaletteRgb5
                {
                    Red = native & 31,
                    Green = native >> 5 & 31,
                    Blue = native >> 10 & 31,
                };
            }
            return colors;
        }
    }
}
