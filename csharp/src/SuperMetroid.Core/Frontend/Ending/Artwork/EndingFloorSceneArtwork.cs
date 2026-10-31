using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// One installed ending backdrop (#1409): its low-byte map, copied into both native $2000-word halves, and how many Mode 7
/// characters its artwork holds. The installed ending floor sheets picture the characters; their pixels belong to the
/// import pipeline that reads the installed artwork.
/// </summary>
public class EndingFloorSceneArtwork
{
    /// <summary>Retains the validated low-byte map and the installed characters' count for one scene.</summary>
    /// <param name="map">The row-ordered tile indices.</param>
    /// <param name="characterCount">The installed Mode 7 characters.</param>
    internal EndingFloorSceneArtwork(byte[] map, int characterCount)
    {
        Map = map;
        CharacterCount = characterCount;
    }

    /// <summary>The $2000 row-ordered tile indices written to the low VRAM byte lane at words $0000 and $2000.</summary>
    public ReadOnlyMemory<byte> Map { get; }
    /// <summary>The installed Mode 7 characters.</summary>
    public int CharacterCount { get; }

    /// <summary>Reads and validates a 128x64 map's ordered tile indices.</summary>
    /// <param name="mapJson">Version-1 JSON containing 8192 ordered tile indices in the range 0..255.</param>
    /// <exception cref="InvalidDataException">The map schema is invalid.</exception>
    internal static byte[] ReadMapCells(Stream mapJson) =>
        ReadMap(Ensure.NotNull(mapJson)).Tiles.Select(tile => Ensure.BetweenInclusive(0, byte.MaxValue, tile)).ToArray();

    /// <summary>Serializes an ending backdrop map, validating the complete encoded document before writing any bytes to the destination.</summary>
    /// <param name="json">Destination stream for the map JSON.</param>
    /// <param name="document">Version-1, 128x64 map with exactly 8192 eight-bit tile indices.</param>
    /// <exception cref="ArgumentNullException">The destination stream is null.</exception>
    /// <exception cref="InvalidDataException">The serialized document does not satisfy the ending backdrop map schema.</exception>
    public static void WriteMap(Stream json, EndingFloorMapDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = ReadMap(new MemoryStream(bytes, writable: false));
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
            document.Tiles.Any(tile => (uint)tile > byte.MaxValue))
            throw new InvalidDataException(
                "Ending Mode-7 map requires 128x64 ordered eight-bit tile indexes.");
        return document;
    }
}
