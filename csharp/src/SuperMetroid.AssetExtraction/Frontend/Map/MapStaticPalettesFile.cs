using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Reads and writes the installed map palette document.</summary>
public static class MapStaticPalettesFile
{
    /// <summary>Loads and validates the pause, file-select, and six world-map palettes from JSON.</summary>
    /// <param name="json">Stream containing the map-palette document.</param>
    /// <returns>The compiled static palettes.</returns>
    public static MapStaticPalettes Read(Stream json)
    {
        MapStaticPalettesDocument document = JsonAssetDocument.Read<MapStaticPalettesDocument>(
            json, ArtworkJson.Options, "map palettes");
        if (document.Version != MapStaticPalettesFormat.Version || document.World is null || document.World.Count != AreaIds.Zebes.Count)
            throw new InvalidDataException("Map palettes require version 1 and the six Zebes world-map selections.");
        var world = new Dictionary<AreaId, RgbColor[]>();
        foreach (AreaId area in AreaIds.Zebes)
        {
            if (!document.World.TryGetValue(area.ToString(), out var colors))
                throw new InvalidDataException($"Map palettes are missing world selection {area}.");
            world.Add(area, Compile(colors, area.ToString()));
        }
        return new(Compile(document.Pause, "pause"), Compile(document.FileSelect, "fileSelect"), world);
    }

    /// <summary>Requires exactly 256 RGB5 entries, in the cartridge's colour order.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? colors, string name)
    {
        if (colors is null || colors.Length != ScreenColourImage.ColorCount)
            throw new InvalidDataException($"Map palette {name} requires 256 RGB colors.");
        var result = new RgbColor[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            var color = colors[i];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 || (uint)color.Blue > 31)
                throw new InvalidDataException($"Map palette {name}, color {i} requires RGB components from 0 to 31.");
            result[i] = color.ToRgbColor();
        }
        return result;
    }

    /// <summary>Writes a map-palette document as JSON.</summary>
    /// <param name="json">Destination stream.</param>
    /// <param name="document">Document to serialize.</param>
    public static void Write(Stream json, MapStaticPalettesDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        json.Write(bytes);
    }
}
