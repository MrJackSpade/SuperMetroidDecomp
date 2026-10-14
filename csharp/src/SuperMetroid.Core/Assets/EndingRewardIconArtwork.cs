using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The native $8000-byte reward-jump stream has one tilemap byte followed by one
/// Mode-7 character byte per word. Keep its full 128x128 map, unlike the repeated
/// half-map used by the earlier atmospheric scenes.
/// </summary>
public sealed class EndingRewardIconArtwork
{
    /// <summary>Retains the interleaved full-map and character transfer for reward landing.</summary>
    private EndingRewardIconArtwork(byte[] transfer) => Transfer = transfer;

    /// <summary>The $8000-byte low-map/high-character word stream consumed by the sixteen $0800-byte uploads at native $8B:F682.</summary>
    public ReadOnlyMemory<byte> Transfer { get; }

    /// <summary>Compiles the full 128x128 tilemap and the 256-tile character atlas into the native interleaved post-credits beam/icon stream.</summary>
    /// <param name="mapJson">Version-1 JSON containing 16384 row-ordered tile indices, each in the range 0..255.</param>
    /// <param name="charactersPng">A 128x128, 256-color indexed PNG; its palette is validated but its colors are not installed.</param>
    /// <returns>A transfer with each map byte followed by the corresponding byte of tile-ordered chunky character data.</returns>
    /// <exception cref="ArgumentNullException">Either input stream is null.</exception>
    /// <exception cref="InvalidDataException">The map schema or indexed character atlas is invalid.</exception>
    public static EndingRewardIconArtwork Load(Stream mapJson, Stream charactersPng)
    {
        Ensure.NotNull(mapJson);
        Ensure.NotNull(charactersPng);
        EndingMode7MapDocument map;
        try
        {
            map = JsonSerializer.Deserialize<EndingMode7MapDocument>(mapJson,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Reward icon map JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid reward icon map JSON.", error);
        }
        ValidateMap(map);
        IndexedPngImage image = IndexedPng.Read(charactersPng,
            new(EndingMode7ArtworkFormat.CharacterWidth, EndingMode7ArtworkFormat.CharacterHeight));
        if (image.Palette.Length != 256)
            throw new InvalidDataException("Reward icon characters require a 256-color indexed PNG.");
        byte[] characters = SnesMode7TileEncoder.Encode(image.Pixels,
            image.Size);
        var transfer = new byte[EndingRewardIconArtworkFormat.TransferBytes];
        for (int index = 0; index < EndingRewardIconArtworkFormat.MapBytes; index++)
        {
            transfer[index * 2] = checked((byte)map.Tiles[index]);
            transfer[index * 2 + 1] = characters[index];
        }
        return new EndingRewardIconArtwork(transfer);
    }

    /// <summary>Validates and writes the full post-credits 128x128 Mode-7 map using the shared map JSON schema.</summary>
    /// <param name="json">Destination stream for the validated map.</param>
    /// <param name="map">Version-1 map containing exactly 16384 eight-bit tile indices.</param>
    /// <exception cref="ArgumentNullException">The destination stream is null.</exception>
    /// <exception cref="InvalidDataException">The map's version, dimensions, index count, or index range is invalid.</exception>
    public static void WriteMap(Stream json, EndingMode7MapDocument map)
    {
        Ensure.NotNull(json);
        ValidateMap(map);
        JsonSerializer.Serialize(json, map, MapPresentationFormat.JsonOptions);
    }

    /// <summary>Requires a full 128-by-128 reward map of byte-valued tile indices.</summary>
    private static void ValidateMap(EndingMode7MapDocument map)
    {
        if (map.Version != EndingMode7ArtworkFormat.Version ||
            map.Width != EndingRewardIconArtworkFormat.MapWidth ||
            map.Height != EndingRewardIconArtworkFormat.MapHeight ||
            map.Tiles is not { Length: EndingRewardIconArtworkFormat.MapBytes } ||
            map.Tiles.Any(tile => (uint)tile > byte.MaxValue))
            throw new InvalidDataException(
                "Reward icon map requires 128x128 ordered eight-bit tile indexes.");
    }
}
