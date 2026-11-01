using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>
/// Immutable game-over palettes and screen anchors the game binds. The page is a picture the import composes once
/// (#1467); the actors are drawn from the installed game-over sheet, whose compositions belong to the import pipeline that
/// reads the installed presentation (#1463). Menu control, answer semantics, music and Baby sequence dispatch stay compiled in the
/// application.
/// </summary>
public abstract class GameOverPresentation
{
    /// <summary>Installed idle and crying color sets for the Baby's fixed sprite colour set.</summary>
    private readonly GameOverBabyPaint babyPalettes;

    /// <summary>Retains compiled artwork and screen anchors; the page's tiles are the import's, which pictures the page (#1467).</summary>
    /// <param name="placements">The validated anchors and cursor duration.</param>
    /// <param name="babyPalettes">The Baby's idle and crying colours.</param>
    internal GameOverPresentation(GameOverPlacements placements, GameOverBabyPaint babyPalettes)
    {
        this.babyPalettes = babyPalettes;
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
    /// <summary>Menu updates per frame of the looping missile cursor, in the range 1-65535.</summary>
    public int CursorFrameDuration { get; }

    /// <summary>Appends the chosen Baby sprite composition at the installed anchor using its OBJ palette selector.</summary>
    /// <param name="screenSprites">The screen's sprite entries for the current frame.</param>
    /// <param name="frame">Closed, middle, or open Baby frame selected by the compiled animation sequence.</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame is not a supported Baby frame.</exception>
    /// <param name="sheet">The installed game-over sheet.</param>
    public void DrawBaby(ScreenSprites screenSprites, SpriteSheet sheet, GameOverBabyFrame frame) =>
        Ensure.NotNull(screenSprites).AddImage(Ensure.NotNull(sheet).Frame(GameOverPresentationNames.BabyFrameName(frame)), BabyAnchor);

    /// <summary>Appends the egg composition at the shared Baby anchor using the installed egg palette selector.</summary>
    /// <param name="screenSprites">The screen's sprite entries for the current frame.</param>
    /// <param name="sheet">The installed game-over sheet.</param>
    public void DrawEgg(ScreenSprites screenSprites, SpriteSheet sheet) =>
        Ensure.NotNull(screenSprites).AddImage(Ensure.NotNull(sheet).Frame(GameOverPresentationDefinitions.EggFrame), BabyAnchor);

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
            new Position16<SpritePixels>(CursorX, selectNo ? NoCursorY : YesCursorY));
    }

    /// <summary>Writes one selected sixteen-colour Baby animation palette over the Baby's sprite colour set.</summary>
    /// <param name="colours">The screen's colours.</param>
    /// <param name="palette">Idle or one of the three crying palettes selected by the compiled Baby sequence.</param>
    /// <exception cref="ArgumentOutOfRangeException">The palette identity is unsupported.</exception>
    public void ApplyBabyPalette(ScreenPalette colours, GameOverBabyPalette palette)
    {
        _ = GameOverPresentationNames.BabyPaletteName(palette);
        ColourRun babyColours = Ensure.NotNull(colours).Colours(ScreenColourGroup.GameOverBaby);
        for (int index = 0; index < babyColours.Count; index++)
            babyColours.Write(index, babyPalettes.Read(palette, index));
    }
}
