using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>Reads and writes the installed opening palette document.</summary>
public static class IntroCinematicPaletteFile
{
    /// <summary>Validates 256 ordered RGB5 colors and compiles their little-endian BGR555 image, calculating stock/shared shade relationships only when supplied colors match them.</summary>
    /// <param name="json">Caller-owned stream containing the supported editable opening-palette document.</param>
    /// <returns>The installed palette used for opening setup and subsequent cinematic fade targets, without changing fade scheduling.</returns>
    /// <exception cref="InvalidDataException">The schema version, complete color count, or RGB components are invalid.</exception>
    public static IntroCinematicPalette Read(Stream json)
    {
        IntroCinematicPaletteDocument document = JsonAssetDocument.Read<IntroCinematicPaletteDocument>(
            json, ArtworkJson.Options, "opening palette");
        if (document.Version != IntroCinematicPaletteFormat.Version ||
            document.Colors is not { Length: ScreenPalette.ColorCount })
            throw new InvalidDataException("Opening palette requires 256 RGB5 colors.");

        var image = new RgbColor[ScreenPalette.ColorCount];
        for (int index = 0; index < document.Colors.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException(
                    $"Opening palette color {index} requires red, green and blue in 0..31.");
            image[index] = color.ToRgbColor();
        }
        return IntroCinematicPalette.FromColours(image);
    }

    /// <summary>Serializes the complete opening palette as UTF-8 JSON and validates it through <see cref="Read"/> before writing any destination bytes.</summary>
    /// <param name="json">Caller-owned destination stream.</param>
    /// <param name="document">Complete ordered RGB5 palette to validate and serialize.</param>
    public static void Write(Stream json, IntroCinematicPaletteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Read(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
