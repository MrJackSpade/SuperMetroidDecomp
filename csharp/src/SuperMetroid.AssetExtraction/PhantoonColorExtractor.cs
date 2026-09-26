using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts Phantoon's health, fade-out, and ship-power RGB5 targets.</summary>
public static class PhantoonColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var bands = new PaletteRgb5[PhantoonColorRomData.HealthBandCount][];
        for (int band = 0; band < bands.Length; band++)
            bands[band] = Read(PhantoonColorRomData.HealthBandsSource +
                band * PhantoonColorRomData.HealthBandColorCount * sizeof(ushort),
                PhantoonColorRomData.HealthBandColorCount);
        return PhantoonColorCatalog.Write(new PhantoonColorDocument
        {
            Version = PhantoonColorFormat.Version,
            HealthBands = bands,
            FadeOut = Read(PhantoonColorRomData.FadeOutSource,
                PhantoonColorRomData.FadeOutCount),
            PowerOn = Read(PhantoonColorRomData.PowerOnSource,
                PhantoonColorRomData.PowerOnCount),
        });

        PaletteRgb5[] Read(int source, int count)
        {
            var colors = new PaletteRgb5[count];
            for (int color = 0; color < count; color++)
            {
                int address = source + color * sizeof(ushort);
                ushort native = RomDataReader.ReadWordFixedBank(bus, address);
                if ((native & 0x8000) != 0)
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
