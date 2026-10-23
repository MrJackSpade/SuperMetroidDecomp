using SuperMetroid.Core.Rooms.Artwork;
using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The scrolling sky as BG2 shows it in a frame (#1347, #1403): its image and the pictures of the cells the room rebound
/// (#1392), the row a vertical position selects repeating every 2048 pixels down and 256 across, and nothing on a row that
/// selects no sky page.
/// </summary>
public sealed class SkyPictures : IBgSource
{
    /// <summary>Holds the sky's image and which of <paramref name="rows"/>' rows select a page.</summary>
    public SkyPictures(RoomBackgroundImagePlane image, SkyTilemap rows)
    {
        Image = Ensure.NotNull(image);
        Ensure.NotNull(rows);
        RowsShown = [.. Enumerable.Range(0, SkyTilemap.Rows).Select(rows.RowShown)];
    }

    /// <summary>The sky's image and its rebound cells' pictures.</summary>
    public RoomBackgroundImagePlane Image { get; }

    /// <summary>Whether each tile row selects a sky page; the ocean table's last chunks select none.</summary>
    public ImmutableArray<bool> RowsShown { get; }

    /// <inheritdoc />
    public bool TrySample(Position16<BgPlanePixels> scrolled, out BgSample sample)
    {
        const int tile = 8, cell = 16;
        int y = scrolled.Y % (SkyTilemap.Rows * tile), x = scrolled.X % RoomBackgroundImage.PagePixels;
        if (!RowsShown[y / tile])
        {
            sample = default;
            return false;
        }
        sample = Image.Cell(y / cell * (RoomBackgroundImage.PageTiles / 2) + x / cell) is { } picture
            ? BgSample.OfPicture(picture, new Position16<ImagePixels>((ushort)(x % cell), (ushort)(y % cell)))
            : BgSample.OfPicture(Image.Picture, new Position16<ImagePixels>((ushort)x, (ushort)y));
        return true;
    }
}
