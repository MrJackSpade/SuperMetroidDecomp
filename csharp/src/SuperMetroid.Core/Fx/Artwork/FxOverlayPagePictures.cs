using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The page each room effect shows on the effect overlay, drawn from a room's effect overlay picture sheet by the effect
/// (#1466, #1514, #1536). The import pictures, in every tileset's sheet, the page of each effect a room state of the
/// tileset runs that shows one (<see cref="RoomFxOverlayState.ShowsPage"/>), under <see cref="PictureName"/>.
/// </summary>
public static class FxOverlayPagePictures
{
    /// <summary>The name of the picture, in a room's effect overlay picture sheet, of the page room effect <paramref name="type"/> shows.</summary>
    public static string PictureName(RoomFxType type) => $"Fx{type}";

    /// <summary>
    /// The picture of the page room effect <paramref name="type"/> shows, from <paramref name="sheet"/>, while the room's
    /// appearance inputs take the values <paramref name="appearance"/> reads, as its part behind and its part in front (#1634).
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet pictures no page of the effect, or none for those values.</exception>
    public static LayeredPicture Page(WrittenPictureSheet sheet, RoomFxType type, RoomAppearanceView appearance) =>
        Ensure.NotNull(sheet).Picture(PictureName(type), appearance);
}
