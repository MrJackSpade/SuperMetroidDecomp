using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// The installed title presentation (#1409, #1463, #1467). The installed title sheets picture its OBJ compositions and
/// its floor, which belong to the import pipeline that reads the installed artwork. Motion and animation page order
/// remain compiled behavior.
/// </summary>
public abstract class TitleGraphicsPresentation
{
    /// <summary>Only the import's reading of the installed artwork is a title presentation.</summary>
    internal TitleGraphicsPresentation()
    {
    }

    /// <summary>Places one title frame of the title sheet, its parts in the title's colours <paramref name="colouring"/>.</summary>
    public static void DrawSprite(SpriteSheet sheet, TitleSprite sprite, ScreenSprites sprites, Position16<SpritePixels> origin,
        ScreenColourGroup colouring) =>
        Ensure.NotNull(sprites).AddImage(Ensure.NotNull(sheet).Frame(TitleSpriteDefinitions.FrameName(sprite)), origin,
            ScreenSpriteColouring.Drawn(colouring));
}
