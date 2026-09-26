using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts Draygon's four transfer images and eight health-band color records.</summary>
public static class DraygonColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var bands = new PaletteRgb5[DraygonColorRomData.HealthBandCount][];
        for (int band = 0; band < bands.Length; band++)
            bands[band] = Read(DraygonColorRomData.HealthBandsSource +
                band * DraygonColorRomData.HealthBandColorCount * sizeof(ushort),
                DraygonColorRomData.HealthBandColorCount);
        return DraygonColorCatalog.Write(new DraygonColorDocument
        {
            Version = DraygonColorFormat.Version,
            Intro = Read(DraygonColorRomData.IntroSource, DraygonColorRomData.IntroCount),
            Background = Read(DraygonColorRomData.BackgroundSource,
                DraygonColorRomData.BackgroundCount),
            Sprite = Read(DraygonColorRomData.SpriteSource,
                DraygonColorRomData.SpriteCount),
            WhiteFlash = Read(DraygonColorRomData.WhiteFlashSource,
                DraygonColorRomData.WhiteFlashCount),
            HealthBands = bands,
        });

        PaletteRgb5[] Read(int source, int count)
        {
            var colors = new PaletteRgb5[count];
            for (int index = 0; index < count; index++)
            {
                int address = source + index * sizeof(ushort);
                ushort native = RomDataReader.ReadWordFixedBank(bus, address);
                if ((native & 0x8000) != 0)
                    throw new InvalidDataException(
                        $"Draygon color ${address:X6} has an unrepresentable high bit.");
                colors[index] = new PaletteRgb5
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
