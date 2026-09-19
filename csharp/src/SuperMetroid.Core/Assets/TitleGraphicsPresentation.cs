using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable title character artwork and Mode 7 tile layout. Motion, animation page
/// order, VRAM destinations, and sprite composition remain compiled behavior.
/// </summary>
public sealed class TitleGraphicsPresentation
{
    private readonly byte[] mode7Characters, mode7Map, objectCharacters, babyCharacters;

    private TitleGraphicsPresentation(
        byte[] mode7Characters,
        byte[] mode7Map,
        byte[] objectCharacters,
        byte[] babyCharacters)
    {
        this.mode7Characters = mode7Characters;
        this.mode7Map = mode7Map;
        this.objectCharacters = objectCharacters;
        this.babyCharacters = babyCharacters;
    }

    public ReadOnlySpan<byte> Mode7Characters => mode7Characters;
    public ReadOnlySpan<byte> Mode7Map => mode7Map;
    public ReadOnlySpan<byte> ObjectCharacters => objectCharacters;
    public ReadOnlySpan<byte> BabyCharacters => babyCharacters;

    public static TitleGraphicsPresentation Load(
        Stream mode7TilesPng,
        Stream mode7MapJson,
        Stream objectTilesPng,
        Stream babyTilesPng)
    {
        IndexedPngImage mode7 = IndexedPng.Read(
            mode7TilesPng,
            TitleGraphicsFormat.Mode7Width,
            TitleGraphicsFormat.Mode7Height);
        IndexedPngImage objects = IndexedPng.Read(
            objectTilesPng,
            TitleGraphicsFormat.ObjectWidth,
            TitleGraphicsFormat.ObjectHeight);
        IndexedPngImage baby = IndexedPng.Read(
            babyTilesPng,
            TitleGraphicsFormat.BabyWidth,
            TitleGraphicsFormat.BabyHeight);
        if (mode7.Palette.Length != 256 || baby.Palette.Length != 256 || objects.Palette.Length != 16)
            throw new InvalidDataException("Title graphics require 256-color Mode 7/Baby PNGs and a 16-color OBJ PNG.");

        TitleMode7MapDocument map;
        try
        {
            map = JsonSerializer.Deserialize<TitleMode7MapDocument>(
                mode7MapJson,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Title Mode 7 map is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid title Mode 7 map JSON.", error);
        }
        if (map.Version != TitleGraphicsFormat.Version ||
            map.Width != TitleGraphicsFormat.MapWidth ||
            map.Height != TitleGraphicsFormat.MapHeight ||
            map.Tiles is null ||
            map.Tiles.Length != TitleGraphicsFormat.MapWidth * TitleGraphicsFormat.MapHeight ||
            map.Tiles.Any(tile => (uint)tile > byte.MaxValue))
        {
            throw new InvalidDataException(
                $"Title Mode 7 map requires version {TitleGraphicsFormat.Version}, " +
                $"{TitleGraphicsFormat.MapWidth}x{TitleGraphicsFormat.MapHeight} cells, and eight-bit tile indexes.");
        }

        return new TitleGraphicsPresentation(
            SnesMode7TileEncoder.Encode(mode7.Pixels, mode7.Width, mode7.Height),
            map.Tiles.Select(tile => checked((byte)tile)).ToArray(),
            SnesPlanarTileEncoder.Encode(objects.Pixels, objects.Width, objects.Height, 4),
            SnesMode7TileEncoder.Encode(baby.Pixels, baby.Width, baby.Height));
    }

    public static void WriteMap(Stream json, TitleMode7MapDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        TitleMode7MapDocument restored;
        try
        {
            restored = JsonSerializer.Deserialize<TitleMode7MapDocument>(bytes, MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Title Mode 7 map is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid title Mode 7 map JSON.", error);
        }
        if (restored.Version != TitleGraphicsFormat.Version ||
            restored.Width != TitleGraphicsFormat.MapWidth ||
            restored.Height != TitleGraphicsFormat.MapHeight ||
            restored.Tiles is null ||
            restored.Tiles.Length != TitleGraphicsFormat.MapWidth * TitleGraphicsFormat.MapHeight ||
            restored.Tiles.Any(tile => (uint)tile > byte.MaxValue))
            throw new InvalidDataException("Invalid title Mode 7 map document.");
        json.Write(bytes);
    }
}

public sealed record TitleMode7MapDocument
{
    public required int Version { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required int[] Tiles { get; init; }
}

public static class TitleGraphicsFormat
{
    public const int Version = 1;
    public const string Mode7TilesFile = "title-mode7-tiles.png";
    public const string Mode7MapFile = "title-mode7-map.json";
    public const string ObjectTilesFile = "title-object-tiles.png";
    public const string BabyTilesFile = "title-baby-tiles.png";
    public const int Mode7Width = 128;
    public const int Mode7Height = 128;
    public const int MapWidth = 64;
    public const int MapHeight = 64;
    public const int ObjectWidth = 256;
    public const int ObjectHeight = 128;
    public const int BabyWidth = 32;
    public const int BabyHeight = 32;
}
