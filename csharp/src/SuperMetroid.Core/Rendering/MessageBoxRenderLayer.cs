using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The message box's text as BG3 shows it (#1392): the pre-coloured image of the message it shows, centered on the
/// window, shown only on the reveal window's scanlines.
/// </summary>
public sealed record MessageBoxRenderLayer : RenderLayer
{
    /// <summary>Checks the image's shape and the reveal radius.</summary>
    /// <param name="picture">The image of the message's text rows.</param>
    /// <param name="radiusPixels">The centered reveal half-height in pixels, from zero through 24.</param>
    /// <exception cref="ArgumentException">The image is not three through six full-width rows.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The radius is out of range.</exception>
    public MessageBoxRenderLayer(SpriteSheetFrame picture, int radiusPixels)
    {
        Ensure.NotNull(picture);
        int rows = picture.Size.Y / GameplayMessageRomData.Layout.TilePixels;
        if (picture.Size.X != GameplayMessageRomData.Layout.TilemapWidth * GameplayMessageRomData.Layout.TilePixels
            || picture.Size.Y % GameplayMessageRomData.Layout.TilePixels != 0
            || rows < GameplayMessageRomData.Layout.MinimumRows || rows > GameplayMessageRomData.Layout.MaximumRows)
            throw new ArgumentException("A message's image must be three through six complete rows.", nameof(picture));
        if (radiusPixels is < 0 or > GameplayMessageRomData.Timing.MaximumRadiusPixels)
            throw new ArgumentOutOfRangeException(nameof(radiusPixels));
        Picture = picture;
        RadiusPixels = radiusPixels;
    }

    /// <summary>The image of the message's text rows.</summary>
    public SpriteSheetFrame Picture { get; }

    /// <summary>The current centered reveal half-height in pixels, from zero through 24.</summary>
    public int RadiusPixels { get; }

    /// <summary>The scanline the image's top row is drawn on: the rows centered on the window.</summary>
    public int Top => GameplayMessageRomData.Layout.WindowCenterY - Picture.Size.Y / 2;

    /// <summary>The first scanline the reveal window shows.</summary>
    public int FirstScanline => GameplayMessageRomData.Layout.WindowCenterY - RadiusPixels;

    /// <summary>The scanline after the last one the reveal window shows.</summary>
    public int EndScanline => GameplayMessageRomData.Layout.WindowCenterY + RadiusPixels;

    /// <summary>
    /// The colours the message's text shows in over <paramref name="room"/>, the frame's: the room's, with the box's light
    /// and dark text colours in place of the two inks they replace while the box is open.
    /// </summary>
    public static BackgroundColours ShownIn(BackgroundColours room) => Ensure.NotNull(room)
        .With(GameplayMessageRomData.Palette.TemporaryLightIndex, GameplayMessageRomData.Palette.TemporaryLightColor)
        .With(GameplayMessageRomData.Palette.TemporaryDarkIndex, GameplayMessageRomData.Palette.TemporaryDarkColor);

    /// <summary>The layer showing the image <paramref name="sheet"/> pictures <paramref name="tilemap"/> with.</summary>
    /// <param name="tilemap">The message's packed tilemap words, 32 per row.</param>
    /// <param name="radiusPixels">The reveal half-height in pixels.</param>
    /// <param name="sheet">The room's message box images.</param>
    /// <exception cref="InvalidDataException">The sheet pictures no message of these words.</exception>
    public static MessageBoxRenderLayer FromTilemap(ReadOnlySpan<ushort> tilemap, int radiusPixels, MessageBoxSheet sheet) =>
        new(Ensure.NotNull(sheet).Picture(tilemap), radiusPixels);
}
