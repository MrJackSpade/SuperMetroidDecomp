using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// What a background shows at a pixel (#1392): a tile drawn from characters, a pixel of a pre-coloured block picture, or a
/// pixel of a tile layer cell's pre-coloured tile, which takes its priority from the cell.
/// </summary>
public readonly record struct BgSample
{
    /// <summary>Keeps one of the two kinds of sample.</summary>
    private BgSample(BgTileSample tile, SpriteSheetFrame? picture, Position16<ImagePixels> picturePixel)
    {
        Tile = tile;
        Picture = picture;
        PicturePixel = picturePixel;
    }

    /// <summary>The tile shown, when <see cref="Picture"/> is null.</summary>
    public BgTileSample Tile { get; }

    /// <summary>Whether <see cref="Picture"/> pictures a tile layer cell, whose priority, <see cref="HighPriority"/>, the pixel takes.</summary>
    public bool TilePriority { get; private init; }

    /// <summary>Whether the cell <see cref="Picture"/> pictures is in its layer's upper priority tier, when <see cref="TilePriority"/> is set.</summary>
    public bool HighPriority { get; private init; }

    /// <summary>The block picture shown, or null where a tile drawn from characters shows.</summary>
    public SpriteSheetFrame? Picture { get; }

    /// <summary>The pixel of <see cref="Picture"/> shown, from its top-left corner, after the block's mirroring.</summary>
    public Position16<ImagePixels> PicturePixel { get; }

    /// <summary>A tile drawn from characters.</summary>
    public static BgSample OfTile(BgTileSample tile) => new(tile, null, default);

    /// <summary>Pixel <paramref name="pixel"/> of block picture <paramref name="picture"/>.</summary>
    public static BgSample OfPicture(SpriteSheetFrame picture, Position16<ImagePixels> pixel) => new(default, Ensure.NotNull(picture), pixel);

    /// <summary>Pixel <paramref name="pixel"/> of <paramref name="picture"/>, the pre-coloured tile of a cell in priority tier <paramref name="highPriority"/>.</summary>
    public static BgSample OfTilePicture(SpriteSheetFrame picture, Position16<ImagePixels> pixel, bool highPriority) =>
        new(default, Ensure.NotNull(picture), pixel) { TilePriority = true, HighPriority = highPriority };
}
