using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The identities of the background artwork a room binds over the BG characters (#1392): its tileset and common room
/// elements, its collectibles' graphics and Kraid's backdrop. Extraction keys the bytes it composes pictures from by
/// these; the pictures themselves are named by the appearance inputs that show the artwork (#1466).
/// </summary>
public static class RoomBgArtwork
{
    /// <summary>The common room element characters every Zebes room loads.</summary>
    public static BgArtworkIdentity CommonRoomElements { get; } = new("common room element characters");

    /// <summary>The characters of tileset <paramref name="set"/>.</summary>
    public static BgArtworkIdentity Tileset(RoomCharacterSet set) => new($"tileset characters {set}");


    /// <summary>
    /// Collectible <paramref name="kind"/> over a collectible graphics slot, two animation frames of four characters: the
    /// slot's blocks show the room block sheet's pictures named for it.
    /// </summary>
    public static BgArtworkIdentity Collectible(InWorldCollectibleKind kind) => new($"collectible {kind}");

    /// <summary>The room backdrop characters Kraid's rise shows over the room's tiles.</summary>
    public static BgArtworkIdentity KraidBackdrop { get; } = new("Kraid room background characters");
}
