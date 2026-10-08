using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>The three mutually exclusive native Mode-7 ending backdrops.</summary>
public enum EndingMode7SceneId
{
    /// <summary>Zebes zooming out during the first escape backdrop, using native characters at $98:BCD6 and packed map at $99:D17E.</summary>
    EscapeA,
    /// <summary>Grey clouds during the second escape backdrop, using native characters at $98:ED4F and packed map at $99:D65B.</summary>
    EscapeB,
    /// <summary>The large Zebes backdrop for the planet explosion, using native characters at $99:9101 and packed map at $99:D932.</summary>
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

    /// <summary>The $2000 row-ordered tile indices written to the low VRAM byte lane at words $0000 and $2000.</summary>
    public ReadOnlyMemory<byte> Map { get; }
    /// <summary>The $4000 chunky pixel bytes for 256 ordered 8x8 tiles, written to the high VRAM byte lane.</summary>
    public ReadOnlyMemory<byte> Characters { get; }

    /// <summary>Validates a 128x64 map and a 128x128 indexed character atlas, then compiles their separate native Mode-7 lanes.</summary>
    /// <param name="mapJson">Version-1 JSON containing 8192 ordered tile indices in the range 0..255.</param>
    /// <param name="charactersPng">A 256-color indexed PNG whose 16x16 grid supplies the 256 character tiles; palette colors are not imported here.</param>
    /// <returns>The decoded map lane and tile-ordered character lane.</returns>
    /// <exception cref="ArgumentNullException">Either input stream is null.</exception>
    /// <exception cref="InvalidDataException">The map schema, atlas format, palette length, or native transfer length is invalid.</exception>
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

    /// <summary>Serializes an ending backdrop map, validating the complete encoded document before writing any bytes to the destination.</summary>
    /// <param name="json">Destination stream for the map JSON.</param>
    /// <param name="document">Version-1, 128x64 map with exactly 8192 eight-bit tile indices.</param>
    /// <exception cref="ArgumentNullException">The destination stream is null.</exception>
    /// <exception cref="InvalidDataException">The serialized document does not satisfy the ending backdrop map schema.</exception>
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
    private readonly EndingMode7SceneArtwork escapeA, escapeB, planetExplosion;

    /// <summary>Installs the three decoded backdrop pairs and the complete post-credits beam/icon transfer.</summary>
    /// <param name="escapeA">Zebes zoom-out map and character lanes for the first escape scene.</param>
    /// <param name="escapeB">Grey-cloud map and character lanes for the second escape scene.</param>
    /// <param name="planetExplosion">Large-Zebes map and character lanes for the explosion scene.</param>
    /// <param name="rewardIcon">Interleaved map and characters progressively uploaded during the post-credits reward jump.</param>
    /// <exception cref="ArgumentNullException">Any artwork argument is null.</exception>
    public EndingMode7ArtworkCatalog(EndingMode7SceneArtwork escapeA,
        EndingMode7SceneArtwork escapeB, EndingMode7SceneArtwork planetExplosion,
        EndingRewardIconArtwork rewardIcon)
    {
        this.escapeA = escapeA ?? throw new ArgumentNullException(nameof(escapeA));
        this.escapeB = escapeB ?? throw new ArgumentNullException(nameof(escapeB));
        this.planetExplosion = planetExplosion ?? throw new ArgumentNullException(nameof(planetExplosion));
        RewardIcon = rewardIcon ?? throw new ArgumentNullException(nameof(rewardIcon));
    }

    /// <summary>Interleaved map/character sheet transferred during the reward landing.</summary>
    public EndingRewardIconArtwork RewardIcon { get; }

    /// <summary>Identity of each decoded scene's two transfer lanes and the full reward-icon transfer.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(EndingMode7ArtworkCatalog), content =>
    {
        content.Append("scene-count", 3);
        content.Append("map", escapeA.Map.Span);
        content.Append("characters", escapeA.Characters.Span);
        content.Append("map", escapeB.Map.Span);
        content.Append("characters", escapeB.Characters.Span);
        content.Append("map", planetExplosion.Map.Span);
        content.Append("characters", planetExplosion.Characters.Span);
        content.Append("reward-icon", RewardIcon.Transfer.Span);
    });

    /// <summary>Gets the map and character lanes for the selected native ending backdrop.</summary>
    /// <param name="id">The escape or planet-explosion scene whose artwork is required.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value does not identify one of the three supported scenes.</exception>
    public EndingMode7SceneArtwork this[EndingMode7SceneId id] => id switch
    {
        EndingMode7SceneId.EscapeA => escapeA,
        EndingMode7SceneId.EscapeB => escapeB,
        EndingMode7SceneId.PlanetExplosion => planetExplosion,
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };
}

/// <summary>
/// The native $8000-byte reward-jump stream has one tilemap byte followed by one
/// Mode-7 character byte per word. Keep its full 128x128 map, unlike the repeated
/// half-map used by the earlier atmospheric scenes.
/// </summary>
public sealed class EndingRewardIconArtwork
{
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

    /// <summary>Validates and writes the full post-credits 128x128 Mode-7 map using the shared map JSON schema.</summary>
    /// <param name="json">Destination stream for the validated map.</param>
    /// <param name="map">Version-1 map containing exactly 16384 eight-bit tile indices.</param>
    /// <exception cref="ArgumentNullException">The destination stream is null.</exception>
    /// <exception cref="InvalidDataException">The map's version, dimensions, index count, or index range is invalid.</exception>
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
    /// <summary>Editable JSON filename for the full post-credits beam/icon tilemap.</summary>
    public const string MapFileName = "post-credits-icon-map.json";
    /// <summary>Indexed PNG filename for the 128x128 post-credits beam/icon character atlas.</summary>
    public const string CharacterFileName = "post-credits-icon-characters.png";
    /// <summary>Width of the full native Mode-7 tilemap in tile indices.</summary>
    public const int MapWidth = 128;
    /// <summary>Height of the full native Mode-7 tilemap in tile indices; this map is not a repeated 64-row half-map.</summary>
    public const int MapHeight = 128;
    /// <summary>$4000 low-lane map bytes, one eight-bit tile index per entry of the 128x128 map.</summary>
    public const int MapBytes = MapWidth * MapHeight;
    /// <summary>$8000 bytes after interleaving the map and character lanes, covering sixteen native $0800-byte DMA chunks.</summary>
    public const int TransferBytes = MapBytes * 2;
}

/// <summary>Editable eight-bit Mode-7 tilemap schema shared by 128x64 ending backdrops and the full 128x128 post-credits beam/icon map.</summary>
public sealed record EndingMode7MapDocument
{
    /// <summary>Schema revision, currently 1 for both ending backdrop and reward-icon maps.</summary>
    public required int Version { get; init; }
    /// <summary>Number of tile indices per map row; both supported map forms require 128.</summary>
    public required int Width { get; init; }
    /// <summary>Number of map rows: 64 for a repeated ending backdrop half-map, or 128 for the reward-icon map.</summary>
    public required int Height { get; init; }
    /// <summary>Exactly Width times Height row-ordered tile indices in the range 0..255, selecting tiles from the separate 256-tile character atlas.</summary>
    public required int[] Tiles { get; init; }
}

/// <summary>File identities and transfer geometry for the three ending backdrops.</summary>
public static class EndingMode7ArtworkFormat
{
    /// <summary>Supported JSON map schema revision for both the backdrop and reward-icon documents.</summary>
    public const int Version = 1;
    /// <summary>Manifest filename identifying the editable ending Mode-7 artwork collection.</summary>
    public const string ManifestFileName = "ending-mode7-artwork.json";
    /// <summary>Indexed character-atlas width in pixels, containing sixteen 8-pixel-wide tiles per row.</summary>
    public const int CharacterWidth = 128;
    /// <summary>Indexed character-atlas height in pixels, containing sixteen rows of 8-pixel-high tiles.</summary>
    public const int CharacterHeight = 128;
    /// <summary>$4000 high-lane character bytes: 256 tiles of 64 chunky eight-bit pixels each.</summary>
    public const int CharacterByteCount = 0x4000;
    /// <summary>Width of one native ending backdrop half-map in tile indices.</summary>
    public const int MapWidth = 128;
    /// <summary>Height of the editable backdrop half-map, whose 64 rows are repeated to fill the native 128-row map.</summary>
    public const int MapHeight = 64;
    /// <summary>$2000 low-lane tile indices per half-map, uploaded at both VRAM word $0000 and word $2000.</summary>
    public const int MapByteCount = MapWidth * MapHeight;

    /// <summary>Gets the indexed character-atlas filename for escape-a, escape-b, or planet-explosion.</summary>
    /// <param name="id">The native backdrop whose editable PNG is requested.</param>
    /// <exception cref="ArgumentOutOfRangeException">The scene identifier is unsupported.</exception>
    public static string CharacterFileName(EndingMode7SceneId id) =>
        $"ending-{SceneName(id)}-mode7-characters.png";

    /// <summary>Gets the half-map JSON filename for escape-a, escape-b, or planet-explosion.</summary>
    /// <param name="id">The native backdrop whose editable map is requested.</param>
    /// <exception cref="ArgumentOutOfRangeException">The scene identifier is unsupported.</exception>
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
