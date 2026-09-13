using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Imports ordered map sprite artwork only, excluding all game collision and navigation data.</summary>
public static class MapSpriteExtractor
{
    public static Dictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        if (RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.LabelSpritemapBase) != MapSpriteDefinitions.WorldTitle)
            throw new InvalidDataException("Unexpected native world-title sprite binding.");
        var frames = new Dictionary<string, SpriteVisualPart[]>();
        foreach (var definition in MapSpriteDefinitions.Frames)
            frames.Add(definition.Name, MenuSpriteExtractor.Read(bus, definition.NativeId));
        byte[] pixels = SnesGraphics.DecodePlanarTiles(RomDataReader.ReadFixedBank(bus, MapSpriteFormat.SourceAddress, MapSpriteFormat.ByteCount),
            4, MapSpriteFormat.TileColumns, out int width, out int height);
        using var png = new MemoryStream();
        IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new MapSpriteDocument { Version = MapSpriteFormat.Version, Frames = frames },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
        byte[] pngBytes = png.ToArray();
        _ = MapSpriteCatalog.Load(new MemoryStream(json), new MemoryStream(pngBytes));
        return new() { [MapSpriteFormat.JsonFile] = json, [MapSpriteFormat.PngFile] = pngBytes };
    }
}
