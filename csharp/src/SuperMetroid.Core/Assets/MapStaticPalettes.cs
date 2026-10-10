using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Named complete presentation palettes; no native offsets, palette programs or selection rules.</summary>
public sealed class MapStaticPalettes
{
    private readonly Bgr555[] pause, fileSelect;
    private readonly Dictionary<AreaId, Bgr555[]> world;
    private MapStaticPalettes(Bgr555[] pause, Bgr555[] fileSelect, Dictionary<AreaId, Bgr555[]> world)
    { this.pause = pause; this.fileSelect = fileSelect; this.world = world; }

    /// <summary>Gets the complete 256-color pause-screen palette as packed SNES RGB555 words.</summary>
    public ReadOnlySpan<Bgr555> Pause => pause;

    /// <summary>Gets the complete 256-color file-select palette as packed SNES RGB555 words.</summary>
    public ReadOnlySpan<Bgr555> FileSelect => fileSelect;

    /// <summary>Gets the complete 256-color world-map palette for one of the six Zebes areas.</summary>
    /// <param name="selectedArea">Zebes area whose selected-map palette is requested.</param>
    /// <returns>The area's packed SNES RGB555 palette.</returns>
    public ReadOnlySpan<Bgr555> World(AreaId selectedArea) => world.TryGetValue(selectedArea, out var colors)
        ? colors : throw new ArgumentOutOfRangeException(nameof(selectedArea), "World map palettes cover the six Zebes areas.");

    /// <summary>Loads and validates the pause, file-select, and six world-map palettes from JSON.</summary>
    /// <param name="json">Stream containing the map-palette document.</param>
    /// <returns>The compiled static palettes.</returns>
    public static MapStaticPalettes Load(Stream json)
    {
        MapStaticPalettesDocument document = JsonAssetDocument.Read<MapStaticPalettesDocument>(
            json, MapPresentationFormat.JsonOptions, "map palettes");
        if (document.Version != MapStaticPalettesFormat.Version || document.World is null || document.World.Count != AreaIds.RetailCount - 1)
            throw new InvalidDataException("Map palettes require version 1 and the six Zebes world-map selections.");
        var world = new Dictionary<AreaId, Bgr555[]>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            if (area == AreaId.Ceres) continue;
            if (!document.World.TryGetValue(area.ToString(), out var colors))
                throw new InvalidDataException($"Map palettes are missing world selection {area}.");
            world.Add(area, Compile(colors, area.ToString()));
        }
        return new(Compile(document.Pause, "pause"), Compile(document.FileSelect, "fileSelect"), world);
    }

    private static Bgr555[] Compile(PaletteRgb5[]? colors, string name)
    {
        if (colors is null || colors.Length != SnesCgram.ColorCount)
            throw new InvalidDataException($"Map palette {name} requires 256 RGB colors.");
        var result = new Bgr555[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            var color = colors[i];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 || (uint)color.Blue > 31)
                throw new InvalidDataException($"Map palette {name}, color {i} requires RGB components from 0 to 31.");
            result[i] = color.ToBgr555();
        }
        return result;
    }

    /// <summary>Validates and writes a map-palette document as JSON.</summary>
    /// <param name="json">Destination stream.</param>
    /// <param name="document">Document to validate and serialize.</param>
    public static void Write(Stream json, MapStaticPalettesDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>JSON schema for complete pause, file-select, and area-map palettes.</summary>
public sealed record MapStaticPalettesDocument
{
    /// <summary>Gets the schema version, which must equal <see cref="MapStaticPalettesFormat.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the 256 editable RGB5 colors used by the pause screen.</summary>
    public required PaletteRgb5[] Pause { get; init; }

    /// <summary>Gets the 256 editable RGB5 colors used by file select.</summary>
    public required PaletteRgb5[] FileSelect { get; init; }

    /// <summary>Gets the six 256-color Zebes world-map palettes keyed by area name.</summary>
    public required Dictionary<string, PaletteRgb5[]> World { get; init; }
}

/// <summary>Defines the static map-palette document contract.</summary>
public static class MapStaticPalettesFormat
{
    /// <summary>Current static map-palette schema version.</summary>
    public const int Version = 1;

    /// <summary>Canonical static map-palette asset file name.</summary>
    public const string FileName = "map-palettes.json";
}
