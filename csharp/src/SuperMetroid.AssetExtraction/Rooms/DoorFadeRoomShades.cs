using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// A tileset's room pictures as a door's fade leaves them: every colour black but those the door keeps lit. The
/// cartridge's door fades every palette toward a target that is black but for a handful of entries it copies from
/// the colours shown (<c>$82:E1B7</c>): they are the HUD's, and the HUD's small palettes are the first thirty-two
/// colours, which are also the first two palette lines of the room's own tiles. So a room's tiles drawn in those
/// entries stay lit through the door's scroll, the doors' frames among them. The entries are the ones
/// <see cref="DoorFadeHudShades"/> keeps for the HUD.
/// </summary>
internal static partial class DoorFadeRoomShades
{
    /// <summary>A room's inks: its eight colour sets of sixteen.</summary>
    private const int RoomInks = 128;

    /// <summary>A room's pictures once a door has faded it out: every colour the door fades, black.</summary>
    internal static PictureShade Dark { get; } = new("door-room-dark", 0);

    /// <summary>The same for a door between two common rooms, which keeps more lit.</summary>
    internal static PictureShade DarkBetweenCommonRooms { get; } = new("door-room-dark-common-rooms", 0);

    /// <summary>A room's two dark shades: every door's, and a door's between two common rooms.</summary>
    internal static IReadOnlyList<SheetShade> Of() =>
    [
        SheetShade.Of(Dark, Enumerable.Range(0, RoomInks).Except(DoorFadeHudShades.Kept), _ => RgbColor.Black),
        SheetShade.Of(DarkBetweenCommonRooms,
            Enumerable.Range(0, RoomInks).Except(DoorFadeHudShades.Kept).Except(DoorFadeHudShades.KeptBetweenCommonRooms), _ => RgbColor.Black),
    ];
}
