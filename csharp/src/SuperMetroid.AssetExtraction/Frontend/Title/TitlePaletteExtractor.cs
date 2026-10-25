using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>Converts the title's complete cartridge CGRAM image to editable RGB5 JSON.</summary>
internal static class TitlePaletteExtractor
{
    /// <summary>Exports the full title CGRAM image, both ambient palette-FX timelines, and copyright skip colors to the versioned presentation catalog.</summary>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var source = new SnesCgram();
        CartridgePaletteImporter.LoadToCgram(source, bus, TitleSourceDefinitions.Palette);
        PaletteRgb5[] colors = new CgramPalettes(source).Image.SelectToArray(PaletteRgb5.From);
        using var json = new MemoryStream();
        TitlePaletteFile.Write(json, new TitlePaletteDocument
        {
            Version = TitlePaletteFormat.Version,
            Colors = colors,
            BabyMetroidTubeLight = ExtractAmbient(
                TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight),
            FlickeringDisplays = ExtractAmbient(
                TitleScreenAmbientPaletteFxProgramOwner.FlickeringDisplays),
            SkipCopyrightWhite = PaletteRgb5.From(TitleSequenceDefinitions.Palette.CopyrightWhite),
            SkipCopyrightRed = PaletteRgb5.From(TitleSequenceDefinitions.Palette.CopyrightRed),
        });
        return json.ToArray();

        PaletteRgb5[][] ExtractAmbient(TitleScreenAmbientPaletteFxProgramOwner owner)
        {
            TitleScreenAmbientPaletteFxColorDefinition definition =
                TitleScreenAmbientPaletteFxColorDefinitions.All.Single(
                    item => item.Owner == owner);
            return PaletteFxColorExtraction.ReadFrames(bus, definition.Rows);
        }
    }
}
