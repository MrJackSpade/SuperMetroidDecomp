namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// What one library-background command reads (#1414): a compressed tilemap, a staged tilemap, a sky page or a set of
/// character artwork, named for what it is rather than where the cartridge stores it. Extraction maps each source to its
/// cartridge or work-RAM location.
/// </summary>
public readonly record struct LibraryBackgroundSource
{
    /// <summary>A source of <paramref name="kind"/> with its tilemap or sky page.</summary>
    private LibraryBackgroundSource(LibraryBackgroundSourceKind kind, RoomBackgroundTilemap tilemap, int skyPage)
    {
        Kind = kind;
        Tilemap = tilemap;
        SkyPage = skyPage;
    }

    /// <summary>What the command reads.</summary>
    public LibraryBackgroundSourceKind Kind { get; }

    /// <summary>The compressed tilemap a <see cref="LibraryBackgroundSourceKind.Tilemap"/> source names; unused otherwise.</summary>
    public RoomBackgroundTilemap Tilemap { get; }

    /// <summary>The sky page, 0 through 6, a <see cref="LibraryBackgroundSourceKind.SkyPage"/> source names; zero otherwise.</summary>
    public int SkyPage { get; }

    /// <summary>No source: a clear command's.</summary>
    public static LibraryBackgroundSource None { get; } = new(LibraryBackgroundSourceKind.None, default, 0);

    /// <summary>The tilemap the list staged.</summary>
    public static LibraryBackgroundSource TilemapStaging { get; } = new(LibraryBackgroundSourceKind.TilemapStaging, default, 0);

    /// <summary>The tilemap the room's enemies staged.</summary>
    public static LibraryBackgroundSource EnemyTilemapStaging { get; } = new(LibraryBackgroundSourceKind.EnemyTilemapStaging, default, 0);

    /// <summary>The Tourian entrance statue ghost's characters.</summary>
    public static LibraryBackgroundSource TourianStatueGhostCharacters { get; } = new(LibraryBackgroundSourceKind.TourianStatueGhostCharacters, default, 0);

    /// <summary>The HUD's characters.</summary>
    public static LibraryBackgroundSource HudCharacters { get; } = new(LibraryBackgroundSourceKind.HudCharacters, default, 0);

    /// <summary>The compressed tilemap <paramref name="tilemap"/>.</summary>
    public static LibraryBackgroundSource Compressed(RoomBackgroundTilemap tilemap) =>
        new(LibraryBackgroundSourceKind.Tilemap, tilemap, 0);

    /// <summary>Scrolling-sky page <paramref name="page"/>, 0 through 6.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> is not a sky page.</exception>
    public static LibraryBackgroundSource Sky(int page) =>
        new(LibraryBackgroundSourceKind.SkyPage, default, Ensure.BetweenInclusive(0, SuperMetroid.Core.Rooms.Artwork.RoomSkyTilemapFormat.PageCount - 1, page));

    /// <summary>The source's name in diagnostics and artwork identities.</summary>
    public string Name => Kind switch
    {
        LibraryBackgroundSourceKind.Tilemap => $"tilemap {Tilemap}",
        LibraryBackgroundSourceKind.SkyPage => $"sky page {SkyPage}",
        LibraryBackgroundSourceKind.None or LibraryBackgroundSourceKind.TilemapStaging or LibraryBackgroundSourceKind.EnemyTilemapStaging
            or LibraryBackgroundSourceKind.TourianStatueGhostCharacters or LibraryBackgroundSourceKind.HudCharacters => Kind.ToString(),
        _ => throw new InvalidOperationException($"Unknown library-background source kind {Kind}."),
    };

    /// <inheritdoc cref="Name"/>
    public override string ToString() => Name;
}
