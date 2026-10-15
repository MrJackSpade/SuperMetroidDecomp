namespace SuperMetroid.Core.Frontend.Ending.Artwork;

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
    public const int CharacterByteCount = 256 * 64;
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

    /// <summary>Maps a supported ending scene to its stable filename stem.</summary>
    private static string SceneName(EndingMode7SceneId id) => id switch
    {
        EndingMode7SceneId.EscapeA => "escape-a",
        EndingMode7SceneId.EscapeB => "escape-b",
        EndingMode7SceneId.PlanetExplosion => "planet-explosion",
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };
}
