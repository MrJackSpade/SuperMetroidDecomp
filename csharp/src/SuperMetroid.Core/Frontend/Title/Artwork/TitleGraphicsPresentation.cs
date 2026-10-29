using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// The installed title presentation (#1409, #1463): its Mode 7 tile layout. The installed title sheets picture its
/// characters and OBJ compositions, which belong to the import pipeline that reads the installed artwork. Motion,
/// animation page order, and VRAM destinations remain compiled behavior.
/// </summary>
public abstract class TitleGraphicsPresentation
{
    /// <summary>The 64-by-64 title map of Mode 7 tile numbers.</summary>
    private readonly byte[] floorMap;

    /// <summary>Retains the title's validated Mode 7 map.</summary>
    /// <param name="floorMap">The 64-by-64 title map of Mode 7 tile numbers.</param>
    internal TitleGraphicsPresentation(byte[] floorMap) => this.floorMap = Ensure.NotNull(floorMap);

    /// <summary>64-by-64 title map of Mode-7 character indexes.</summary>
    public ReadOnlySpan<byte> FloorMap => floorMap;

    /// <summary>Places one title frame of the title sheet, its parts in the title's colours <paramref name="colouring"/>.</summary>
    public static void DrawSprite(SpriteSheet sheet, TitleSprite sprite, ScreenSprites sprites, Position16<SpritePixels> origin,
        ScreenColourGroup colouring) =>
        Ensure.NotNull(sprites).AddImage(Ensure.NotNull(sheet).Frame(TitleSpriteDefinitions.FrameName(sprite)), origin,
            ScreenSpriteColouring.Drawn(colouring));
}
