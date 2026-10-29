using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// What one library-background command reads (#1414): a compressed tilemap, a staged tilemap, a sky page or a set of
/// character artwork, named for what it is rather than where the cartridge stores it. Each kind is its own record,
/// carrying only what that kind names. Extraction maps each source to its cartridge or work-RAM location.
/// </summary>
public abstract record LibraryBackgroundSource
{
    /// <summary>Only the nested kinds derive.</summary>
    private LibraryBackgroundSource()
    {
    }

    /// <summary>The source's name in diagnostics and artwork identities.</summary>
    public abstract string Name { get; }

    /// <summary>The identity of the characters a transfer from the source brings, as extraction composes pictures over them.</summary>
    public BgArtworkIdentity CharacterArtwork => new($"library characters {Name}");

    /// <summary>No source: a clear command's.</summary>
    public static LibraryBackgroundSource None { get; } = new NothingRead();

    /// <summary>The tilemap the list staged.</summary>
    public static LibraryBackgroundSource TilemapStaging { get; } = new StagedTilemap();

    /// <summary>The tilemap the room's enemies staged.</summary>
    public static LibraryBackgroundSource EnemyTilemapStaging { get; } = new StagedEnemyTilemap();

    /// <summary>The Tourian entrance statue ghost's characters.</summary>
    public static LibraryBackgroundSource TourianStatueGhostCharacters { get; } = new StatueGhostCharacters();

    /// <summary>The HUD's characters.</summary>
    public static LibraryBackgroundSource HudCharacters { get; } = new HudCharacterSet();

    /// <summary>The compressed tilemap <paramref name="tilemap"/>.</summary>
    public static LibraryBackgroundSource Compressed(RoomBackgroundTilemap tilemap) => new CompressedTilemap(tilemap);

    /// <summary>Scrolling-sky page <paramref name="page"/>, 0 through 6.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> is not a sky page.</exception>
    public static LibraryBackgroundSource Sky(int page) => new SkyPage(page);

    /// <inheritdoc cref="Name"/>
    public sealed override string ToString() => Name;

    /// <summary>A clear command, which reads nothing.</summary>
    public sealed record NothingRead : LibraryBackgroundSource
    {
        /// <inheritdoc/>
        public override string Name => "None";
    }

    /// <summary>The tilemap the list staged.</summary>
    public sealed record StagedTilemap : LibraryBackgroundSource
    {
        /// <inheritdoc/>
        public override string Name => "TilemapStaging";
    }

    /// <summary>The tilemap the room's enemies staged.</summary>
    public sealed record StagedEnemyTilemap : LibraryBackgroundSource
    {
        /// <inheritdoc/>
        public override string Name => "EnemyTilemapStaging";
    }

    /// <summary>The Tourian entrance statue ghost's characters.</summary>
    public sealed record StatueGhostCharacters : LibraryBackgroundSource
    {
        /// <inheritdoc/>
        public override string Name => "TourianStatueGhostCharacters";
    }

    /// <summary>The HUD's characters.</summary>
    public sealed record HudCharacterSet : LibraryBackgroundSource
    {
        /// <inheritdoc/>
        public override string Name => "HudCharacters";
    }

    /// <summary>A compressed background tilemap.</summary>
    /// <param name="Tilemap">The tilemap decompressed.</param>
    public sealed record CompressedTilemap(RoomBackgroundTilemap Tilemap) : LibraryBackgroundSource
    {
        /// <inheritdoc/>
        public override string Name => $"tilemap {Tilemap}";
    }

    /// <summary>One scrolling-sky page.</summary>
    public sealed record SkyPage : LibraryBackgroundSource
    {
        /// <summary>Sky page <paramref name="page"/>, 0 through 6.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> is not a sky page.</exception>
        public SkyPage(int page) =>
            Page = Ensure.BetweenInclusive(0, SuperMetroid.Core.Rooms.Artwork.RoomSkyTilemapFormat.PageCount - 1, page);

        /// <summary>The sky page, 0 through 6.</summary>
        public int Page { get; }

        /// <inheritdoc/>
        public override string Name => $"sky page {Page}";
    }
}
