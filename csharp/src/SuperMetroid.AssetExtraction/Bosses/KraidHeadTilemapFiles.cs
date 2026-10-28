using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Writes one of Kraid's cartridge head frames as its editable head-tilemap document.</summary>
internal static class KraidHeadTilemapFiles
{
    /// <summary>Converts one native head frame to indented camel-case UTF-8 JSON, then reloads it and verifies that every tile round-trips unchanged.</summary>
    /// <param name="tilemap">Exactly 704 bytes containing 352 little-endian cartridge tilemap entries in 32-column row-major order.</param>
    /// <returns>A new JSON byte array expressing character coordinates, palette, priority, and horizontal/vertical flips for every cell.</returns>
    /// <exception cref="InvalidDataException">The source length is incorrect or the JSON round-trip changes a tile.</exception>
    internal static byte[] Encode(ReadOnlySpan<byte> tilemap)
    {
        if (tilemap.Length != KraidBackgroundDefinitions.HeadImageCells * sizeof(ushort))
            throw new InvalidDataException("Kraid head tilemap source has wrong length.");
        BackgroundTile[] tiles = BackgroundTileWords.ToTiles(tilemap);
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new KraidHeadTilemapDocument
        {
            Version = KraidHeadTilemapFormat.Version,
            Width = KraidHeadTilemapFormat.Width,
            Height = KraidHeadTilemapFormat.Height,
            Cells = [.. tiles.Select(tile => BackgroundTileCells.Of(tile))],
        }, JsonOptions);
        KraidHeadTilemapAtlas roundtrip = KraidHeadTilemapAtlas.Load(new MemoryStream(json, writable: false));
        if (!roundtrip.Tiles.Span.SequenceEqual(tiles))
            throw new InvalidDataException("Kraid head JSON changed a native tile.");
        return json;
    }

    /// <summary>The head-tilemap document's camel-case, indented schema settings.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
