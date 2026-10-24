using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The installed post-credits reward icon (#1409): its full 128x128 Mode 7 map, unlike the repeated half-map of the earlier
/// atmospheric scenes, and how many Mode 7 characters its artwork holds. The installed reward icon sheet pictures the
/// characters; their pixels belong to the import pipeline that reads the installed artwork.
/// </summary>
public class EndingRewardIconArtwork
{
    /// <summary>Retains the validated full map and the installed characters' count.</summary>
    /// <param name="map">The row-ordered tile indices of the full 128x128 map.</param>
    /// <param name="characterCount">The installed Mode 7 characters.</param>
    internal EndingRewardIconArtwork(byte[] map, int characterCount)
    {
        Map = map;
        CharacterCount = characterCount;
    }

    /// <summary>The 16384 row-ordered tile indices the sixteen reward-chunk uploads at native $8B:F682 lay out.</summary>
    public ReadOnlyMemory<byte> Map { get; }

    /// <summary>The installed Mode 7 characters.</summary>
    public int CharacterCount { get; }

    /// <summary>Reads and validates the full 128x128 map's ordered tile indices.</summary>
    /// <param name="mapJson">Version-1 JSON containing 16384 row-ordered tile indices, each in the range 0..255.</param>
    /// <exception cref="InvalidDataException">The map schema is invalid.</exception>
    internal static byte[] ReadMapCells(Stream mapJson)
    {
        Ensure.NotNull(mapJson);
        EndingFloorMapDocument map;
        try
        {
            map = JsonSerializer.Deserialize<EndingFloorMapDocument>(mapJson,
                ArtworkJson.Options)
                ?? throw new InvalidDataException("Reward icon map JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid reward icon map JSON.", error);
        }
        ValidateMap(map);
        return map.Tiles.Select(tile => checked((byte)tile)).ToArray();
    }

    /// <summary>Validates and writes the full post-credits 128x128 Mode-7 map using the shared map JSON schema.</summary>
    /// <param name="json">Destination stream for the validated map.</param>
    /// <param name="map">Version-1 map containing exactly 16384 eight-bit tile indices.</param>
    /// <exception cref="ArgumentNullException">The destination stream is null.</exception>
    /// <exception cref="InvalidDataException">The map's version, dimensions, index count, or index range is invalid.</exception>
    public static void WriteMap(Stream json, EndingFloorMapDocument map)
    {
        Ensure.NotNull(json);
        ValidateMap(map);
        JsonSerializer.Serialize(json, map, ArtworkJson.Options);
    }

    /// <summary>Requires a full 128-by-128 reward map of byte-valued tile indices.</summary>
    private static void ValidateMap(EndingFloorMapDocument map)
    {
        if (map.Version != EndingFloorArtworkFormat.Version ||
            map.Width != EndingRewardIconArtworkFormat.MapWidth ||
            map.Height != EndingRewardIconArtworkFormat.MapHeight ||
            map.Tiles is not { Length: EndingRewardIconArtworkFormat.MapBytes } ||
            map.Tiles.Any(tile => (uint)tile > byte.MaxValue))
            throw new InvalidDataException(
                "Reward icon map requires 128x128 ordered eight-bit tile indexes.");
    }
}
