using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Bosses.Draygon.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Draygon;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Extracts Draygon's four transfer images and eight health-band color records.</summary>
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
            HealthBands = Rgb5Rows.KeyedInTableOrder(DraygonSourceDefinitions.HealthTableOrder, band => band.Key,
                row => Read(DraygonSourceDefinitions.HealthBandColors + row * DraygonColorDefinitions.HealthBandColorCount * sizeof(ushort),
                    DraygonColorDefinitions.HealthBandColorCount)),
        });

        PaletteRgb5[] Read(int source, int count)
        {
            var colors = new PaletteRgb5[count];
            for (int index = 0; index < count; index++)
            {
                int address = source + index * sizeof(ushort);
                int native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
                if (!RgbColor.IsColorWord(checked((ushort)native)))
                    throw new InvalidDataException(
                        $"Draygon color ${address:X6} has an unrepresentable high bit.");
                colors[index] = PaletteRgb5.From(RgbColor.FromWordIgnoringTopBit(native));
            }
            return colors;
        }
    }
}
