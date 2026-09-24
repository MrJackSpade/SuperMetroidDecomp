using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable character pixels and paired Mode-7 maps for the approach to Ceres.
/// Flight motion, DMA order and the SPACE COLONY letter script remain compiled logic.
/// </summary>
public sealed class CeresFlightArtworkCatalog
{
    private CeresFlightArtworkCatalog(byte[] mode7Characters, byte[] mode7Maps,
        byte[] objectCharacters)
    {
        Mode7Characters = mode7Characters;
        Mode7Maps = mode7Maps;
        ObjectCharacters = objectCharacters;
    }

    public ReadOnlyMemory<byte> Mode7Characters { get; }
    /// <summary>Front 768 map bytes followed by rear 768 map bytes.</summary>
    public ReadOnlyMemory<byte> Mode7Maps { get; }
    public ReadOnlyMemory<byte> ObjectCharacters { get; }

    public static CeresFlightArtworkCatalog Load(Stream mode7Png, Stream mapJson,
        Stream objectPng)
    {
        ArgumentNullException.ThrowIfNull(mode7Png);
        ArgumentNullException.ThrowIfNull(mapJson);
        ArgumentNullException.ThrowIfNull(objectPng);
        IndexedPngImage mode7 = IndexedPng.Read(mode7Png,
            CeresFlightArtworkFormat.Mode7Width, CeresFlightArtworkFormat.Mode7Height);
        IndexedPngImage objects = IndexedPng.Read(objectPng,
            CeresFlightArtworkFormat.ObjectWidth, CeresFlightArtworkFormat.ObjectHeight);
        if (mode7.Palette.Length != 256 || objects.Palette.Length != 16)
            throw new InvalidDataException("Ceres flight requires 256-color Mode-7 and 16-color OBJ PNGs.");

        CeresFlightMapDocument document = ReadMap(mapJson);
        byte[] map = new byte[2 * CeresFlightArtworkFormat.MapCellsPerView];
        for (int index = 0; index < CeresFlightArtworkFormat.MapCellsPerView; index++)
        {
            map[index] = checked((byte)document.FrontTiles[index]);
            map[CeresFlightArtworkFormat.MapCellsPerView + index] =
                checked((byte)document.RearTiles[index]);
        }
        byte[] characters = SnesMode7TileEncoder.Encode(mode7.Pixels, mode7.Width, mode7.Height);
        byte[] objectCharacters = SnesPlanarTileEncoder.Encode(objects.Pixels,
            objects.Width, objects.Height, 4);
        if (characters.Length != CeresFlightArtworkFormat.Mode7ByteCount ||
            objectCharacters.Length != CeresFlightArtworkFormat.ObjectByteCount)
            throw new InvalidDataException("Ceres flight PNGs compile to unexpected DMA lengths.");
        return new CeresFlightArtworkCatalog(characters, map, objectCharacters);
    }

    public static void WriteMap(Stream json, CeresFlightMapDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = ReadMap(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    private static CeresFlightMapDocument ReadMap(Stream json)
    {
        CeresFlightMapDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CeresFlightMapDocument>(json,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Ceres flight Mode-7 map JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres flight Mode-7 map JSON.", error);
        }
        if (document.Version != CeresFlightArtworkFormat.Version ||
            document.Width != CeresFlightArtworkFormat.MapWidth ||
            document.Height != CeresFlightArtworkFormat.MapHeight ||
            !ValidView(document.FrontTiles) || !ValidView(document.RearTiles))
            throw new InvalidDataException(
                "Ceres flight map requires two ordered 32x24 arrays of eight-bit tile indexes.");
        return document;

        static bool ValidView(int[]? tiles) =>
            tiles is { Length: CeresFlightArtworkFormat.MapCellsPerView } &&
            tiles.All(tile => (uint)tile <= byte.MaxValue);
    }
}

public sealed record CeresFlightMapDocument
{
    public required int Version { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required int[] FrontTiles { get; init; }
    public required int[] RearTiles { get; init; }
}

/// <summary>File identities and exact Ceres approach artwork geometry.</summary>
public static class CeresFlightArtworkFormat
{
    public const int Version = 1;
    public const string Mode7FileName = "ceres-flight-mode7-characters.png";
    public const string MapFileName = "ceres-flight-mode7-maps.json";
    public const string ObjectFileName = "ceres-flight-object-characters.png";
    public const int Mode7Width = 128;
    public const int Mode7Height = 128;
    public const int Mode7ByteCount = 0x4000;
    public const int ObjectWidth = 256;
    public const int ObjectHeight = 128;
    public const int ObjectByteCount = 0x4000;
    public const int MapWidth = 32;
    public const int MapHeight = 24;
    public const int MapCellsPerView = MapWidth * MapHeight;
}
