using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A room's background image as BG2 shows it at one moment (#1392): the image, and the block picture of each 16x16 cell
/// whose characters show artwork the room bound after loading.
/// </summary>
public sealed class RoomBackgroundImagePlane
{
    /// <summary>Each cell's picture, null where the image shows; the array is never written after it is shared.</summary>
    private readonly SpriteSheetFrame?[] cells;

    /// <summary>Keeps the image and the cells' pictures.</summary>
    public RoomBackgroundImagePlane(RoomBackgroundImage image, SpriteSheetFrame?[] cells)
    {
        Image = Ensure.NotNull(image);
        this.cells = Ensure.NotNull(cells);
        Ensure.LengthEqual(cells, image.CellCount);
    }

    /// <summary>The background's image.</summary>
    public RoomBackgroundImage Image { get; }

    /// <summary>The picture cell <paramref name="cell"/> shows in place of the image, or null.</summary>
    public SpriteSheetFrame? Cell(int cell) => cells[cell];

    /// <summary>A library background as BG2 shows it through a tilemap of <paramref name="mapSize"/> cells.</summary>
    public IBgSource At(Extent16<TilemapCells> mapSize) => new Shown(this, mapSize);

    /// <summary>
    /// The scrolling sky as BG2 shows it (#1347): the row a vertical position selects, repeating every 2048 pixels down
    /// and 256 across, and nothing on a row <paramref name="sky"/> shows no page on.
    /// </summary>
    public IBgSource AsSky(SkyTilemap sky) => new Sky(this, Ensure.NotNull(sky));

    /// <summary>The image as the sky's rows.</summary>
    private sealed class Sky(RoomBackgroundImagePlane plane, SkyTilemap sky) : IBgSource
    {
        /// <inheritdoc />
        public bool TrySample(Position16<BgPlanePixels> scrolled, out BgSample sample)
        {
            int y = scrolled.Y % (SkyTilemap.Rows * 8), x = scrolled.X % RoomBackgroundImage.PagePixels;
            if (!sky.RowShown(y / 8))
            {
                sample = default;
                return false;
            }
            int cell = y / 16 * (RoomBackgroundImage.PageTiles / 2) + x / 16;
            sample = plane.cells[cell] is { } picture
                ? BgSample.OfPicture(picture, new Position16<ImagePixels>((ushort)(x % 16), (ushort)(y % 16)))
                : BgSample.OfPicture(plane.Image.Picture, new Position16<ImagePixels>((ushort)x, (ushort)y));
            return true;
        }
    }

    /// <summary>The background through one map size: the hardware's page order over the image's pages.</summary>
    private sealed class Shown(RoomBackgroundImagePlane plane, Extent16<TilemapCells> mapSize) : IBgSource
    {
        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">The map size selects a page past the two the image pictures.</exception>
        public bool TrySample(Position16<BgPlanePixels> scrolled, out BgSample sample)
        {
            int x = scrolled.X & (mapSize.X * 8 - 1), y = scrolled.Y & (mapSize.Y * 8 - 1);
            int tileX = x / 8, tileY = y / 8, pageTiles = RoomBackgroundImage.PageTiles;
            int page = tileY / pageTiles * (mapSize.X / pageTiles) + tileX / pageTiles;
            if (page >= RoomBackgroundImage.Pages)
                throw new InvalidOperationException($"BG2's {mapSize.X}x{mapSize.Y} map shows page {page}, past the two its background pictures.");
            int pageX = x % RoomBackgroundImage.PagePixels, pageY = y % RoomBackgroundImage.PagePixels;
            int cell = page * (pageTiles / 2) * (pageTiles / 2) + pageY / 16 * (pageTiles / 2) + pageX / 16;
            sample = plane.cells[cell] is { } picture
                ? BgSample.OfPicture(picture, new Position16<ImagePixels>((ushort)(pageX % 16), (ushort)(pageY % 16)))
                : BgSample.OfPicture(plane.Image.Picture, new Position16<ImagePixels>((ushort)pageX, (ushort)(page * RoomBackgroundImage.PagePixels + pageY)));
            return true;
        }
    }
}
