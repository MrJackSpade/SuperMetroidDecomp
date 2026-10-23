using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A boss body as BG2 shows it in a frame (#1403): the picture of each of its tilemap's cells, in the body sheet over what
/// the cell's character shows (#1392), sampled with the tilemap's wrap.
/// </summary>
[PpuLayoutBoundary("backend: samples a boss body's cells in the hardware's screen order")]
public sealed class BossBodyPictures : IBgSource
{
    /// <summary>The tilemap's screen arrangement.</summary>
    private readonly BgTilemapGeometry geometry;

    /// <summary>Holds the picture of each cell of a tilemap of <paramref name="size"/>, the first of <paramref name="pictures"/>.</summary>
    /// <exception cref="ArgumentException">A cell has no picture.</exception>
    public BossBodyPictures(Extent16<TilemapCells> size, ImmutableArray<BgTilePicture?> pictures)
    {
        int cells = size.X * size.Y;
        if (pictures.IsDefault || pictures.Length < cells)
            throw new ArgumentException($"A boss body of {cells} cells needs a picture for each.", nameof(pictures));
        var shown = new BgTilePicture[cells];
        for (int cell = 0; cell < cells; cell++)
            shown[cell] = pictures[cell] ?? throw new ArgumentException($"The boss body's cell {cell} has no picture.", nameof(pictures));
        Size = size;
        Pictures = [.. shown];
        geometry = new BgTilemapGeometry(size);
    }

    /// <summary>The tilemap's size in cells.</summary>
    public Extent16<TilemapCells> Size { get; }

    /// <summary>Each cell's picture in the hardware's screen order: each 32x32 screen's rows, screens left to right then down.</summary>
    public ImmutableArray<BgTilePicture> Pictures { get; }

    /// <inheritdoc />
    public bool TrySample(Position16<BgPlanePixels> scrolled, out BgSample sample)
    {
        int x = geometry.WrapX(scrolled.X), y = geometry.WrapY(scrolled.Y);
        BgTilePicture picture = Pictures[geometry.EntryWordAddress(0,
            new Position16<TilemapCells>((ushort)BgCharacterGrid.CharacterOf(x), (ushort)BgCharacterGrid.CharacterOf(y)))];
        sample = BgSample.OfPicture(picture.Picture, new Position16<ImagePixels>(
            (ushort)(picture.Corner.X + BgCharacterGrid.PixelWithinCharacter(x)), (ushort)(picture.Corner.Y + BgCharacterGrid.PixelWithinCharacter(y))));
        return true;
    }
}
