using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Extracts Draygon's four transfer images and eight health-band color records.</summary>
[CpuBusBoundary("codec: ROM Draygon transfer images and health-band colors → DraygonColorCatalog")]
public static class DraygonColorExtractor
{
    /// <summary>Imports Draygon's intro, background, sprite, and white-flash palettes and eight ordered health-band color records.</summary>
    /// <param name="bus">Non-null cartridge import address space containing the native Draygon palette sources.</param>
    /// <returns>New UTF-8 JSON bytes containing the four palette roles and health bands as RGB5 channels in the range 0..31.</returns>
    /// <remarks>The document supplies colors; the runtime retains health-band selection and palette-transfer timing.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A native color uses bit 15, which the RGB5 document cannot represent.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var bands = new PaletteRgb5[DraygonColorDefinitions.HealthBandCount][];
        for (int band = 0; band < bands.Length; band++)
            bands[band] = Read(DraygonSourceDefinitions.HealthBandColors +
                band * DraygonColorDefinitions.HealthBandColorCount * sizeof(ushort),
                DraygonColorDefinitions.HealthBandColorCount);
        return DraygonColorFile.Write(new DraygonColorDocument
        {
            Version = DraygonColorFormat.Version,
            Intro = Read(DraygonSourceDefinitions.IntroColors, DraygonColorDefinitions.IntroCount),
            Background = Read(DraygonSourceDefinitions.BackgroundColors,
                DraygonColorDefinitions.BackgroundCount),
            Sprite = Read(DraygonSourceDefinitions.SpriteColors,
                DraygonColorDefinitions.SpriteCount),
            WhiteFlash = Read(DraygonSourceDefinitions.WhiteFlashColors,
                DraygonColorDefinitions.WhiteFlashCount),
            HealthBands = bands,
        });

        PaletteRgb5[] Read(int source, int count)
        {
            var colors = new PaletteRgb5[count];
            for (int index = 0; index < count; index++)
            {
                int address = source + index * sizeof(ushort);
                ushort native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
                if (!Bgr555.IsColorWord(native))
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
