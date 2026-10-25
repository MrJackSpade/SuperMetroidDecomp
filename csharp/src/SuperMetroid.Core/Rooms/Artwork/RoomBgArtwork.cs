using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The identities of the background artwork a room binds over the BG characters (#1392): its tileset and common room
/// elements, its library background's transfers and its collectibles' graphics. A pre-coloured block picture names the
/// artwork its characters were composed from by these.
/// </summary>
public static class RoomBgArtwork
{
    /// <summary>The common room element characters every Zebes room loads.</summary>
    public static BgArtworkIdentity CommonRoomElements { get; } = new("common room element characters");

    /// <summary>The characters of tileset <paramref name="set"/>.</summary>
    public static BgArtworkIdentity Tileset(RoomCharacterSet set) => new($"tileset characters {set}");

    /// <summary>The characters a library background transfers from <paramref name="source"/>.</summary>
    public static BgArtworkIdentity Library(LibraryBackgroundSource source) => new($"library characters {source.Name}");

    /// <summary>The installed graphics of collectible <paramref name="kind"/>, two animation frames of four characters.</summary>
    public static BgArtworkIdentity Collectible(InWorldCollectibleKind kind) => new($"collectible {kind}");

    /// <summary>Graphics a constructed population supplied for collectible <paramref name="kind"/> in place of the installed ones.</summary>
    public static BgArtworkIdentity SuppliedCollectible(InWorldCollectibleKind kind) => new($"supplied collectible {kind}");
}
