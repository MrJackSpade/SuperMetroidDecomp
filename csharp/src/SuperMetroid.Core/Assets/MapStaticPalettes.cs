using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Named complete presentation palettes; no native offsets, palette programs or selection rules.</summary>
public sealed class MapStaticPalettes
{
    private readonly ushort[] pause, fileSelect;
    private readonly Dictionary<AreaId, ushort[]> world;
    private MapStaticPalettes(ushort[] pause, ushort[] fileSelect, Dictionary<AreaId, ushort[]> world)
    { this.pause = pause; this.fileSelect = fileSelect; this.world = world; }
    public ReadOnlySpan<ushort> Pause => pause;
    public ReadOnlySpan<ushort> FileSelect => fileSelect;
    public ReadOnlySpan<ushort> World(AreaId selectedArea) => world.TryGetValue(selectedArea, out var colors)
        ? colors : throw new ArgumentOutOfRangeException(nameof(selectedArea), "World map palettes cover the six Zebes areas.");

    public static MapStaticPalettes Load(Stream json)
    {
        MapStaticPalettesDocument document;
        try { document = JsonSerializer.Deserialize<MapStaticPalettesDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Map palettes are null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map palettes JSON.", error); }
        if (document.Version != MapStaticPalettesFormat.Version || document.World is null || document.World.Count != AreaIds.RetailCount - 1)
            throw new InvalidDataException("Map palettes require version 1 and the six Zebes world-map selections.");
        var world = new Dictionary<AreaId, ushort[]>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            if (area == AreaId.Ceres) continue;
            if (!document.World.TryGetValue(area.ToString(), out var colors))
                throw new InvalidDataException($"Map palettes are missing world selection {area}.");
            world.Add(area, Compile(colors, area.ToString()));
        }
        return new(Compile(document.Pause, "pause"), Compile(document.FileSelect, "fileSelect"), world);
    }

    private static ushort[] Compile(PaletteRgb5[]? colors, string name)
    {
        if (colors is null || colors.Length != SnesCgram.ColorCount)
            throw new InvalidDataException($"Map palette {name} requires 256 RGB colors.");
        var result = new ushort[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            var color = colors[i];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 || (uint)color.Blue > 31)
                throw new InvalidDataException($"Map palette {name}, color {i} requires RGB components from 0 to 31.");
            result[i] = (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
        }
        return result;
    }

    public static void Write(Stream json, MapStaticPalettesDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record MapStaticPalettesDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Pause { get; init; }
    public required PaletteRgb5[] FileSelect { get; init; }
    public required Dictionary<string, PaletteRgb5[]> World { get; init; }
}
public static class MapStaticPalettesFormat
{
    public const int Version = 1;
    public const string FileName = "map-palettes.json";
}
