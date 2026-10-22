using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// What a background shows at a pixel (#1392): a tile drawn from characters, or a pixel of a pre-coloured block picture.
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

    /// <summary>The block picture shown, or null where a tile drawn from characters shows.</summary>
    public SpriteSheetFrame? Picture { get; }

    /// <summary>The pixel of <see cref="Picture"/> shown, from its top-left corner, after the block's mirroring.</summary>
    public Position16<ImagePixels> PicturePixel { get; }

    /// <summary>A tile drawn from characters.</summary>
    public static BgSample OfTile(BgTileSample tile) => new(tile, null, default);

    /// <summary>Pixel <paramref name="pixel"/> of block picture <paramref name="picture"/>.</summary>
    public static BgSample OfPicture(SpriteSheetFrame picture, Position16<ImagePixels> pixel) => new(default, Ensure.NotNull(picture), pixel);
}
