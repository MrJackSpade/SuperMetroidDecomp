using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// Editable character sheets and the waiting-scene BG2 map for the ending and credits.
/// Actor instructions, palette selection and timing remain code; the actors are drawn from the installed sheets, whose
/// compositions belong to the import pipeline that reads the installed artwork and names the content's identity (#1463).
/// </summary>
public abstract class EndingObjectArtwork
{
    /// <summary>Copied four-fragment sequence ordered by explosion VRAM destination page.</summary>
    private readonly RoomBackgroundTilemapAtlas[] fragments;

    /// <summary>The same fragments keyed by the explosion map each one holds.</summary>
    private readonly Dictionary<EndingObjectFragmentId, RoomBackgroundTilemapAtlas> fragmentsById;

    /// <summary>Combines already compiled ending artwork, enforcing the native transfer lengths.</summary>
    /// <param name="parts">The ending's object character atlases and tilemaps (#633).</param>
    /// <remarks>Copies the fragment sequence but retains the supplied atlas objects. This constructor validates transfer sizes, not stock hashes or manifest provenance; installation loading performs those checks.</remarks>
    /// <exception cref="ArgumentNullException">An atlas or fragment sequence is null.</exception>
    /// <exception cref="InvalidDataException">A transfer has an incorrect byte length, or the fragment sequence does not contain exactly four non-null, correctly sized entries.</exception>
    internal EndingObjectArtwork(EndingObjectSceneParts parts)
    {
        Ensure.NotNull(parts);
        Clouds = Ensure.NotNull(parts.Clouds);
        Explosion = Ensure.NotNull(parts.Explosion);
        WaitingSamus = Ensure.NotNull(parts.WaitingSamus);
        ShootingScreen = Ensure.NotNull(parts.ShootingScreen);
        SuitlessSamus = Ensure.NotNull(parts.SuitlessSamus);
        WaitingTilemap = Ensure.NotNull(parts.WaitingTilemap);
        PostCreditsFragmentA = Ensure.NotNull(parts.PostCreditsFragmentA);
        PostCreditsFragmentB = Ensure.NotNull(parts.PostCreditsFragmentB);
        PostShotLogoTiles = Ensure.NotNull(parts.PostShotLogoTiles);
        PostShotLogoMap = Ensure.NotNull(parts.PostShotLogoMap);
        Ensure.NotNull(parts.Fragments);
        if (!HasRequiredLength(Clouds, EndingObjectTransfer.Clouds) ||
            !HasRequiredLength(Explosion, EndingObjectTransfer.Explosion) ||
            !HasRequiredLength(WaitingSamus, EndingObjectTransfer.WaitingSamus) ||
            !HasRequiredLength(ShootingScreen, EndingObjectTransfer.ShootingScreen) ||
            !HasRequiredLength(SuitlessSamus, EndingObjectTransfer.SuitlessSamus) ||
            !IsWholeMap(WaitingTilemap) ||
            !HasRequiredLength(PostCreditsFragmentA, EndingObjectTransfer.PostCreditsFragmentA) ||
            !IsWholeMap(PostCreditsFragmentB) ||
            !HasRequiredLength(PostShotLogoTiles, EndingObjectTransfer.PostShotLogoTiles) ||
            !IsWholeMap(PostShotLogoMap) ||
            parts.Fragments.Count != EndingObjectArtworkFormat.FragmentCount ||
            parts.Fragments.Any(fragment => fragment is null || !IsWholeMap(fragment)))
            throw new InvalidDataException("Ending OBJ catalog has an incorrect native transfer length.");
        this.fragments = parts.Fragments.ToArray();
        fragmentsById = Enum.GetValues<EndingObjectFragmentId>().Zip(fragments, (id, fragment) => KeyValuePair.Create(id, fragment)).ToDictionary();
    }

    /// <summary>Checks an atlas transfer against the cartridge's required byte count.</summary>
    private static bool HasRequiredLength(RoomCharacterAtlas atlas, EndingObjectTransfer transfer) =>
        HasRequiredLength(atlas.Characters, transfer);

    /// <summary>Whether <paramref name="map"/> holds exactly one 32-by-32 page of tiles.</summary>
    private static bool IsWholeMap(RoomBackgroundTilemapAtlas map) => map.Tiles.Length == EndingObjectArtworkFormat.MapCellCount;

    /// <summary>Checks raw transfer bytes against the exact length of their ending-artwork role.</summary>
    private static bool HasRequiredLength(ReadOnlyMemory<byte> bytes, EndingObjectTransfer transfer) =>
        bytes.Length == EndingObjectArtworkFormat.ByteCount(transfer);

    /// <summary>Compiled 4-bpp atmospheric-cloud OBJ sheet imported from <c>$99:A56F</c> and uploaded at VRAM byte $C000.</summary>
    public RoomCharacterAtlas Clouds { get; }
    /// <summary>Compiled 4-bpp explosion OBJ sheet imported from <c>$98:8304</c> and uploaded at VRAM byte $8000; separate tilemap fragments occupy $E000-$FFFF.</summary>
    public RoomCharacterAtlas Explosion { get; }
    /// <summary>BG2 waiting scene, also reused by both suited reward variants.</summary>
    public RoomCharacterAtlas WaitingSamus { get; }
    /// <summary>Post-credits shooting scene OBJ sheet.</summary>
    public RoomCharacterAtlas ShootingScreen { get; }
    /// <summary>Under-three-hour suitless reward sheet.</summary>
    public RoomCharacterAtlas SuitlessSamus { get; }
    /// <summary>BG2 waiting scene's ordered 32x32 tile and palette references.</summary>
    public RoomBackgroundTilemapAtlas WaitingTilemap { get; }
    /// <summary>BG3 transformation characters imported from <c>$99:DA9F</c> and uploaded at VRAM byte $4000; separate from the transformation map.</summary>
    public RoomCharacterAtlas PostCreditsFragmentA { get; }
    /// <summary>BG3 transformation map's ordered 32x32 tile and palette references, imported from <c>$99:DAB1</c>.</summary>
    public RoomBackgroundTilemapAtlas PostCreditsFragmentB { get; }
    /// <summary>Four post-shot logo tile chunks uploaded to BG/OBJ VRAM.</summary>
    public RoomCharacterAtlas PostShotLogoTiles { get; }
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
