using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// The installed title presentation (#1409, #1463, #1467). The installed title sheets picture its sprite compositions and
/// its floor, which belong to the import pipeline that reads the installed artwork. Motion and animation page order
/// remain compiled behavior.
/// </summary>
public abstract class TitleGraphicsPresentation
{
    /// <summary>Only the import's reading of the installed artwork is a title presentation.</summary>
    internal TitleGraphicsPresentation()
    {
    }

    /// <summary>Places one title frame of the title sheet, its parts drawn in <paramref name="colouring"/> (#1512).</summary>
    public static void DrawSprite(SpriteSheet sheet, TitleSprite sprite, ScreenSprites sprites, Position16<SpritePixels> origin,
        TitleSpriteColouring colouring) =>
        DrawSprite(sheet, sprite, sprites, origin, PictureShading.Shaded(TitleShades.Drawn(colouring)));

    /// <summary>Places one title frame of the title sheet in <paramref name="shading"/> (#1512).</summary>
    public static void DrawSprite(SpriteSheet sheet, TitleSprite sprite, ScreenSprites sprites, Position16<SpritePixels> origin,
        PictureShading shading)
    {
        Ensure.NotNull(shading);
        Ensure.NotNull(sprites).AddImage(Ensure.NotNull(sheet).Frame(TitleSpriteDefinitions.FrameName(sprite)), origin,
            shading: shading);
    }
}
