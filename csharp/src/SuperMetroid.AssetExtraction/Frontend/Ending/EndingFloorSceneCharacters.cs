using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Artwork;
using System.Text.Json;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// One installed ending backdrop as the import pipeline reads it (#1409): its Mode 7 characters as chunky 8-bit pixels,
/// the high-byte lane the ending floor sheets compose from. The discarded odd bytes of the native packed map source never
/// reach visible VRAM because the subsequent high-byte-only transfer overwrites them.
/// </summary>
public sealed class EndingFloorSceneCharacters : EndingFloorSceneArtwork
{
    /// <summary>Retains the validated low-byte map and high-byte character lanes for one scene.</summary>
    /// <param name="map">The row-ordered tile indices.</param>
    /// <param name="characters">The chunky Mode 7 characters.</param>
    private EndingFloorSceneCharacters(byte[] map, byte[] characters)
        : base(map, characters.Length / EndingFloorArtworkFormat.CharacterBytes) => Characters = characters;

    /// <summary>The $4000 chunky pixel bytes for 256 ordered 8x8 tiles, written to the high VRAM byte lane.</summary>
    public ReadOnlyMemory<byte> Characters { get; }

    /// <summary>Validates a 128x64 map and a 128x128 indexed character atlas, then compiles their separate native Mode-7 lanes.</summary>
    /// <param name="mapJson">Version-1 JSON containing 8192 ordered tile indices in the range 0..255.</param>
    /// <param name="charactersPng">A 256-color indexed PNG whose 16x16 grid supplies the 256 character tiles; palette colors are not imported here.</param>
    /// <returns>The decoded map lane and tile-ordered character lane.</returns>
    /// <exception cref="ArgumentNullException">Either input stream is null.</exception>
    /// <exception cref="InvalidDataException">The map schema, atlas format, palette length, or native transfer length is invalid.</exception>
    public static EndingFloorSceneCharacters Read(Stream mapJson, Stream charactersPng)
    {
        Ensure.NotNull(mapJson);
        Ensure.NotNull(charactersPng);
        byte[] map = Array.ConvertAll(ReadMapCells(mapJson), tile => checked((byte)tile));
        IndexedPngImage image = IndexedPng.Read(charactersPng,
            new(EndingFloorArtworkFormat.CharacterWidth, EndingFloorArtworkFormat.CharacterHeight));
        if (image.Colours.Length != 256)
            throw new InvalidDataException("Ending Mode-7 characters require a 256-color indexed PNG.");
        byte[] characters = SnesMode7TileEncoder.Encode(image.Pixels,
            image.Size);
        if (characters.Length != EndingFloorArtworkFormat.CharacterByteCount)
            throw new InvalidDataException("Ending Mode-7 PNG has the wrong native transfer length.");
        return new EndingFloorSceneCharacters(map, characters);
    }

    /// <summary>The characters <paramref name="scene"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The scene was not read from installed artwork.</exception>
    public static EndingFloorSceneCharacters Of(EndingFloorSceneArtwork scene) => scene as EndingFloorSceneCharacters
        ?? throw new InvalidDataException("The ending backdrop was not read from installed artwork.");

    /// <summary>Reads and validates a 128x64 map's ordered tile indices.</summary>
    /// <param name="mapJson">Version-1 JSON containing 8192 ordered tile indices in the range 0..255.</param>
    /// <exception cref="InvalidDataException">The map schema is invalid.</exception>
    internal static int[] ReadMapCells(Stream mapJson) =>
        ReadMap(Ensure.NotNull(mapJson)).Tiles.Select(tile => Ensure.BetweenInclusive(0, byte.MaxValue, tile)).ToArray();

    /// <summary>Serializes an ending backdrop map to the destination.</summary>
    /// <param name="json">Destination stream for the map JSON.</param>
    /// <param name="document">Version-1, 128x64 map with exactly 8192 eight-bit tile indices.</param>
    /// <exception cref="ArgumentNullException">The destination stream is null.</exception>
    public static void WriteMap(Stream json, EndingFloorMapDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        json.Write(bytes);
    }

    /// <summary>Loads and validates the 128-by-64 backdrop half-map and its byte-valued tile indices.</summary>
    private static EndingFloorMapDocument ReadMap(Stream json)
    {
        EndingFloorMapDocument document;
        try
        {
            document = JsonSerializer.Deserialize<EndingFloorMapDocument>(json,
                ArtworkJson.Options)
                ?? throw new InvalidDataException("Ending Mode-7 map JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid ending Mode-7 map JSON.", error);
        }
        if (document.Version != EndingFloorArtworkFormat.Version ||
            document.Width != EndingFloorArtworkFormat.MapWidth ||
            document.Height != EndingFloorArtworkFormat.MapHeight ||
            document.Tiles is not { Length: EndingFloorArtworkFormat.MapByteCount } ||
            document.Tiles.Length != document.Width * document.Height ||
            document.Tiles.Any(tile => (uint)tile > byte.MaxValue))
            throw new InvalidDataException(
                "Ending Mode-7 map requires 128x64 ordered eight-bit tile indexes.");
        return document;
    }
}
