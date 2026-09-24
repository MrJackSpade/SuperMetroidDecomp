using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>The three mutually exclusive native Mode-7 ending backdrops.</summary>
public enum EndingMode7SceneId
{
    EscapeA,
    EscapeB,
    PlanetExplosion,
}

/// <summary>
/// One editable ending backdrop. The low-byte map is copied into both native
/// $2000-word halves; the high-byte character lane supplies the Mode-7 pixels.
/// The discarded odd bytes of the native packed map source never reach visible
/// VRAM because the subsequent high-byte-only transfer overwrites them.
/// </summary>
public sealed class EndingMode7SceneArtwork
{
    private EndingMode7SceneArtwork(byte[] map, byte[] characters)
    {
        Map = map;
        Characters = characters;
    }

    public ReadOnlyMemory<byte> Map { get; }
    public ReadOnlyMemory<byte> Characters { get; }

    public static EndingMode7SceneArtwork Load(Stream mapJson, Stream charactersPng)
    {
        ArgumentNullException.ThrowIfNull(mapJson);
        ArgumentNullException.ThrowIfNull(charactersPng);
        EndingMode7MapDocument document = ReadMap(mapJson);
        IndexedPngImage image = IndexedPng.Read(charactersPng,
            EndingMode7ArtworkFormat.CharacterWidth,
            EndingMode7ArtworkFormat.CharacterHeight);
        if (image.Palette.Length != 256)
            throw new InvalidDataException("Ending Mode-7 characters require a 256-color indexed PNG.");
        byte[] characters = SnesMode7TileEncoder.Encode(image.Pixels,
            image.Width, image.Height);
        if (characters.Length != EndingMode7ArtworkFormat.CharacterByteCount)
            throw new InvalidDataException("Ending Mode-7 PNG has the wrong native transfer length.");
        return new EndingMode7SceneArtwork(
            document.Tiles.Select(tile => checked((byte)tile)).ToArray(), characters);
    }

    public static void WriteMap(Stream json, EndingMode7MapDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = ReadMap(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    private static EndingMode7MapDocument ReadMap(Stream json)
    {
        EndingMode7MapDocument document;
        try
        {
            document = JsonSerializer.Deserialize<EndingMode7MapDocument>(json,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Ending Mode-7 map JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid ending Mode-7 map JSON.", error);
        }
        if (document.Version != EndingMode7ArtworkFormat.Version ||
            document.Width != EndingMode7ArtworkFormat.MapWidth ||
            document.Height != EndingMode7ArtworkFormat.MapHeight ||
            document.Tiles is not { Length: EndingMode7ArtworkFormat.MapByteCount } ||
            document.Tiles.Any(tile => (uint)tile > byte.MaxValue))
            throw new InvalidDataException(
                "Ending Mode-7 map requires 128x64 ordered eight-bit tile indexes.");
        return document;
    }
}

/// <summary>Host-owned visual streams for the escape and planet-explosion scenes.</summary>
public sealed class EndingMode7ArtworkCatalog
{
    private readonly EndingMode7SceneArtwork[] scenes;

    public EndingMode7ArtworkCatalog(EndingMode7SceneArtwork escapeA,
        EndingMode7SceneArtwork escapeB, EndingMode7SceneArtwork planetExplosion,
        EndingRewardIconArtwork rewardIcon)
    {
        scenes =
        [
            escapeA ?? throw new ArgumentNullException(nameof(escapeA)),
            escapeB ?? throw new ArgumentNullException(nameof(escapeB)),
            planetExplosion ?? throw new ArgumentNullException(nameof(planetExplosion)),
        ];
        RewardIcon = rewardIcon ?? throw new ArgumentNullException(nameof(rewardIcon));
    }

    /// <summary>Interleaved map/character sheet transferred during the reward landing.</summary>
    public EndingRewardIconArtwork RewardIcon { get; }

    public EndingMode7SceneArtwork this[EndingMode7SceneId id] =>
        (uint)id < scenes.Length ? scenes[(int)id] :
            throw new ArgumentOutOfRangeException(nameof(id));
}

/// <summary>
/// The native $8000-byte reward-jump stream has one tilemap byte followed by one
/// Mode-7 character byte per word. Keep its full 128x128 map, unlike the repeated
/// half-map used by the earlier atmospheric scenes.
/// </summary>
public sealed class EndingRewardIconArtwork
{
    private EndingRewardIconArtwork(byte[] transfer) => Transfer = transfer;

    public ReadOnlyMemory<byte> Transfer { get; }

    public static EndingRewardIconArtwork Load(Stream mapJson, Stream charactersPng)
    {
        ArgumentNullException.ThrowIfNull(mapJson);
        ArgumentNullException.ThrowIfNull(charactersPng);
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
            EndingMode7ArtworkFormat.CharacterWidth,
            EndingMode7ArtworkFormat.CharacterHeight);
        if (image.Palette.Length != 256)
            throw new InvalidDataException("Reward icon characters require a 256-color indexed PNG.");
        byte[] characters = SnesMode7TileEncoder.Encode(image.Pixels,
            image.Width, image.Height);
        var transfer = new byte[EndingRewardIconArtworkFormat.TransferBytes];
        for (int index = 0; index < EndingRewardIconArtworkFormat.MapBytes; index++)
        {
            transfer[index * 2] = checked((byte)map.Tiles[index]);
            transfer[index * 2 + 1] = characters[index];
        }
        return new EndingRewardIconArtwork(transfer);
    }

    public static void WriteMap(Stream json, EndingMode7MapDocument map)
    {
        ArgumentNullException.ThrowIfNull(json);
        ValidateMap(map);
        JsonSerializer.Serialize(json, map, MapPresentationFormat.JsonOptions);
    }

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

/// <summary>File identities and native dimensions of the reward-jump icon DMA.</summary>
public static class EndingRewardIconArtworkFormat
{
    public const string MapFileName = "post-credits-icon-map.json";
    public const string CharacterFileName = "post-credits-icon-characters.png";
    public const int MapWidth = 128;
    public const int MapHeight = 128;
    public const int MapBytes = MapWidth * MapHeight;
    public const int TransferBytes = MapBytes * 2;
}

public sealed record EndingMode7MapDocument
{
    public required int Version { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required int[] Tiles { get; init; }
}

/// <summary>File identities and transfer geometry for the three ending backdrops.</summary>
public static class EndingMode7ArtworkFormat
{
    public const int Version = 1;
    public const string ManifestFileName = "ending-mode7-artwork.json";
    public const int CharacterWidth = 128;
    public const int CharacterHeight = 128;
    public const int CharacterByteCount = 0x4000;
    public const int MapWidth = 128;
    public const int MapHeight = 64;
    public const int MapByteCount = MapWidth * MapHeight;

    public static string CharacterFileName(EndingMode7SceneId id) =>
        $"ending-{SceneName(id)}-mode7-characters.png";

    public static string MapFileName(EndingMode7SceneId id) =>
        $"ending-{SceneName(id)}-mode7-map.json";

    private static string SceneName(EndingMode7SceneId id) => id switch
    {
        EndingMode7SceneId.EscapeA => "escape-a",
        EndingMode7SceneId.EscapeB => "escape-b",
        EndingMode7SceneId.PlanetExplosion => "planet-explosion",
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };
}
