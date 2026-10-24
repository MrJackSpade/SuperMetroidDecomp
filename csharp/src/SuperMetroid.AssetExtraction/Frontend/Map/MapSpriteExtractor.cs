using SuperMetroid.AssetExtraction.Artwork;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Imports ordered map sprite artwork only, excluding all game collision and navigation data.</summary>
[CpuBusBoundary("codec: ROM bank-$82 map spritemaps → map sprite artwork")]
public static class MapSpriteExtractor
{
    /// <summary>JSON options: Camel-case property names, indented output.</summary>
    private static readonly JsonSerializerOptions CamelCaseIndentedJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    /// <summary>Exports named menu/map OBJ compositions in native part order together with their shared bank-$B6 character atlas.</summary>
    /// <param name="bus">Import-capable cartridge source for native menu spritemaps, world-title binding, and four-bit characters at $B6:C000.</param>
    /// <returns>A new filename-keyed dictionary with owned <c>map-sprites.json</c> and 128-by-128 indexed <c>map-objects.png</c> buffers.</returns>
    /// <remarks>The generated JSON/PNG pair is loaded through the production catalog before return. The sixteen-color diagnostic palette illustrates indices, not runtime colors; map collision, navigation, and discovery state are not exported or changed.</remarks>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not provide cartridge import access, including null.</exception>
    /// <exception cref="InvalidDataException">The world-title binding or generated composition/artwork pair is incompatible with the installed sprite format.</exception>
    public static Dictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        if (RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.LabelSpritemapBase) != (ushort)MapSpriteId.WorldTitle)
            throw new InvalidDataException("Unexpected native world-title sprite binding.");
        var frames = new Dictionary<string, SpriteVisualPart[]>();
        foreach (var definition in MapSpriteDefinitions.Frames)
            frames.Add(definition.Name, MenuSpriteExtractor.Read(bus, (ushort)definition.Id));
        byte[] pixels = SnesGraphics.DecodePlanarTiles(RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), MapSpriteFormat.SourceAddress, MapSpriteFormat.ByteCount),
            4, MapSpriteFormat.TileColumns, out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new MapSpriteDocument { Version = MapSpriteFormat.Version, Frames = frames },
            CamelCaseIndentedJson);
        byte[] pngBytes = png.ToArray();
        _ = MapSpriteCatalog.Load(new MemoryStream(json), new MemoryStream(pngBytes));
        return new() { [MapSpriteFormat.JsonFile] = json, [MapSpriteFormat.PngFile] = pngBytes };
    }
}
