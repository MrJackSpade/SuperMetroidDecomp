
namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>File identities and transfer geometry for the three ending backdrops.</summary>
public static class EndingFloorArtworkFormat
{
    /// <summary>Supported JSON map schema revision for both the backdrop and reward-icon documents.</summary>
    public const int Version = 1;
    /// <summary>Manifest filename identifying the editable ending Mode-7 artwork collection.</summary>
    public const string ManifestFileName = "ending-mode7-artwork.json";
    /// <summary>Indexed character-atlas width in pixels, containing sixteen 8-pixel-wide tiles per row.</summary>
    public const int CharacterWidth = 128;
    /// <summary>Indexed character-atlas height in pixels, containing sixteen rows of 8-pixel-high tiles.</summary>
    public const int CharacterHeight = 128;
    /// <summary>16384 high-lane character bytes: 256 tiles of 64 chunky eight-bit pixels each.</summary>
    public const int CharacterByteCount = 256 * CharacterBytes;
    /// <summary>The chunky 8-bpp pixels of one Mode 7 character.</summary>
    public const int CharacterBytes = 64;

    /// <summary>Width of one ending backdrop half-map in tile indices.</summary>
    public const int MapWidth = 128;
    /// <summary>Height of the editable backdrop half-map, whose 64 rows are repeated to fill the full 128-row map.</summary>
    public const int MapHeight = 64;
    /// <summary>8192 low-lane tile indices per half-map, loaded into both halves of the map.</summary>
    public const int MapByteCount = MapWidth * MapHeight;

    /// <summary>Gets the indexed character-atlas filename for the side-cloud escape, the top-and-bottom-cloud escape, or the planet explosion.</summary>
    /// <param name="id">The backdrop whose editable PNG is requested.</param>
    /// <exception cref="ArgumentOutOfRangeException">The scene identifier is unsupported.</exception>
    public static string CharacterFileName(EndingFloorSceneId id) =>
        $"ending-{SceneName(id)}-mode7-characters.png";

    /// <summary>Gets the half-map JSON filename for the side-cloud escape, the top-and-bottom-cloud escape, or the planet explosion.</summary>
    /// <param name="id">The backdrop whose editable map is requested.</param>
    /// <exception cref="ArgumentOutOfRangeException">The scene identifier is unsupported.</exception>
    public static string MapFileName(EndingFloorSceneId id) =>
        $"ending-{SceneName(id)}-mode7-map.json";

    /// <summary>Maps a supported ending scene to its stable filename stem.</summary>
    private static string SceneName(EndingFloorSceneId id) => id switch
    {
        EndingFloorSceneId.SideCloudEscape => "side-cloud-escape",
        EndingFloorSceneId.TopAndBottomCloudEscape => "top-and-bottom-cloud-escape",
        EndingFloorSceneId.PlanetExplosion => "planet-explosion",
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };
}
