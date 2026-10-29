using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The ending and credits tilemaps the game binds: the waiting-scene, transformation and final-logo maps and the four
/// explosion maps. Actor instructions, palette selection and timing remain code; the actors and the planes' characters
/// are drawn from the installed sheets, whose character atlases and compositions belong to the import pipeline that
/// reads the installed artwork and names the content's identity (#1463).
/// </summary>
public abstract class EndingObjectArtwork
{
    /// <summary>Copied four-fragment sequence ordered by explosion VRAM destination page.</summary>
    private readonly RoomBackgroundTilemapAtlas[] fragments;

    /// <summary>The same fragments keyed by the explosion map each one holds.</summary>
    private readonly Dictionary<EndingObjectFragmentId, RoomBackgroundTilemapAtlas> fragmentsById;

    /// <summary>Combines already compiled ending tilemaps, enforcing whole 32-by-32 maps.</summary>
    /// <param name="parts">The ending's tilemaps (#633).</param>
    /// <remarks>Copies the fragment sequence but retains the supplied map objects. This constructor validates map sizes, not stock hashes or manifest provenance; installation loading performs those checks.</remarks>
    /// <exception cref="ArgumentNullException">A map or fragment sequence is null.</exception>
    /// <exception cref="InvalidDataException">A map is not one whole page, or the fragment sequence does not contain exactly four non-null, whole maps.</exception>
    internal EndingObjectArtwork(EndingObjectSceneParts parts)
    {
        Ensure.NotNull(parts);
        WaitingTilemap = Ensure.NotNull(parts.WaitingTilemap);
        PostCreditsFragmentB = Ensure.NotNull(parts.PostCreditsFragmentB);
        PostShotLogoMap = Ensure.NotNull(parts.PostShotLogoMap);
        Ensure.NotNull(parts.Fragments);
        if (!IsWholeMap(WaitingTilemap) ||
            !IsWholeMap(PostCreditsFragmentB) ||
            !IsWholeMap(PostShotLogoMap) ||
            parts.Fragments.Count != EndingObjectArtworkFormat.FragmentCount ||
            parts.Fragments.Any(fragment => fragment is null || !IsWholeMap(fragment)))
            throw new InvalidDataException("Ending OBJ catalog has an incorrect native transfer length.");
        this.fragments = parts.Fragments.ToArray();
        fragmentsById = Enum.GetValues<EndingObjectFragmentId>().Zip(fragments, (id, fragment) => KeyValuePair.Create(id, fragment)).ToDictionary();
    }

    /// <summary>Whether <paramref name="map"/> holds exactly one 32-by-32 page of tiles.</summary>
    private static bool IsWholeMap(RoomBackgroundTilemapAtlas map) => map.Tiles.Length == EndingObjectArtworkFormat.MapCellCount;

    /// <summary>BG2 waiting scene's ordered 32x32 tile and palette references.</summary>
    public RoomBackgroundTilemapAtlas WaitingTilemap { get; }
    /// <summary>BG3 transformation map's ordered 32x32 tile and palette references, imported from <c>$99:DAB1</c>.</summary>
    public RoomBackgroundTilemapAtlas PostCreditsFragmentB { get; }
    /// <summary>Post-shot logo's ordered 32x32 tile and palette references.</summary>
    public RoomBackgroundTilemapAtlas PostShotLogoMap { get; }

    /// <summary>The four explosion maps in <see cref="EndingObjectFragmentId"/> order.</summary>
    protected IReadOnlyList<RoomBackgroundTilemapAtlas> Fragments => fragments;

    /// <summary>Identity of every selected ending/credits character, map and the compositions its sheets are composed from.</summary>
    public abstract string ContentIdentity { get; }

    /// <summary>Returns one 32-by-32-cell explosion map.</summary>
    /// <param name="id">The explosion map; the copied fragment sequence holds the maps in its declaration order.</param>
    /// <returns>The retained map's tile references.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is not one of the four defined fragment identities.</exception>
    public RoomBackgroundTilemapAtlas Fragment(EndingObjectFragmentId id) =>
        Ensure.Listed(fragmentsById, id);
}
