using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>
/// Immutable game-over screen anchors the game binds (#1616); the Baby's colourings are shades the import pre-colours (#1512).
/// The page is a picture the import composes once (#1467); the actors are drawn from the installed game-over sheet, whose
/// compositions, page layout and palette selectors belong to the import pipeline that reads the installed presentation
/// (#1463). Menu control, answer semantics, music and Baby sequence dispatch stay compiled in the application.
/// </summary>
public sealed class GameOverPresentation
{
    /// <summary>Retains the screen anchors and cursor duration; the page's composition is the import's, which pictures the page (#1467).</summary>
    /// <param name="placements">The validated anchors and cursor duration.</param>
    internal GameOverPresentation(GameOverPlacements placements)
    {
        BabyAnchor = placements.BabyAnchor;
        CursorX = placements.CursorX;
        YesCursorY = placements.YesCursorY;
        NoCursorY = placements.NoCursorY;
        CursorFrameDuration = placements.CursorFrameDuration;
    }

    /// <summary>Shared drawing origin of the Baby and egg compositions in screen pixels, with both coordinates in 0-255.</summary>
    public Position16<SpritePixels> BabyAnchor { get; }
    /// <summary>Missile-cursor horizontal screen coordinate in pixels, shared by both answers and bounded to 0-255.</summary>
    public int CursorX { get; }
    /// <summary>Missile-cursor vertical screen coordinate in pixels for the Yes answer, bounded to 0-255.</summary>
    public int YesCursorY { get; }
    /// <summary>Missile-cursor vertical screen coordinate in pixels for the No answer, bounded to 0-255.</summary>
    public int NoCursorY { get; }
    /// <summary>Menu updates per frame of the looping missile cursor, at least one.</summary>
    public int CursorFrameDuration { get; }

    /// <summary>Appends the chosen Baby sprite composition at the installed anchor, shown in its colours <paramref name="colours"/>.</summary>
    /// <param name="screenSprites">The screen's sprite entries for the current frame.</param>
    /// <param name="sheet">The installed game-over sheet.</param>
    /// <param name="frame">Closed, middle, or open Baby frame selected by the compiled animation sequence.</param>
    /// <param name="colours">Idle or one of the three crying colourings selected by the compiled Baby sequence (#1512).</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame is not a supported Baby frame.</exception>
    public void DrawBaby(ScreenSprites screenSprites, SpriteSheet sheet, GameOverBabyFrame frame, GameOverBabyPalette colours) =>
        Ensure.NotNull(screenSprites).AddImage(Ensure.NotNull(sheet).Frame(GameOverPresentationNames.BabyFrameName(frame)), BabyAnchor,
            shading: PictureShading.Shaded(GameOverBabyShades.Colours(colours)));

    /// <summary>Appends the egg composition at the shared Baby anchor using the installed egg palette selector.</summary>
    /// <param name="screenSprites">The screen's sprite entries for the current frame.</param>
    /// <param name="sheet">The installed game-over sheet.</param>
    public void DrawEgg(ScreenSprites screenSprites, SpriteSheet sheet) =>
        Ensure.NotNull(screenSprites).AddImage(Ensure.NotNull(sheet).Frame(GameOverPresentationDefinitions.EggFrame), BabyAnchor,
            shading: PictureShading.Standard);

    /// <summary>Appends one missile-cursor frame at the installed Yes or No answer position.</summary>
    /// <param name="screenSprites">The screen's sprite entries for the current frame.</param>
    /// <param name="frame">Zero-based looping animation frame 0-3.</param>
    /// <param name="selectNo">True draws at the No position; false draws at the Yes position.</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame is outside 0-3.</exception>
    /// <param name="sheet">The installed game-over sheet.</param>
    public void DrawCursor(ScreenSprites screenSprites, SpriteSheet sheet, int frame, bool selectNo)
    {
        if ((uint)frame >= GameOverPresentationDefinitions.CursorFrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        Ensure.NotNull(screenSprites).AddImage(Ensure.NotNull(sheet).Frame(GameOverPresentationNames.CursorFrameName(frame)),
            new Position16<SpritePixels>(CursorX, selectNo ? NoCursorY : YesCursorY), shading: PictureShading.Standard);
    }
}
