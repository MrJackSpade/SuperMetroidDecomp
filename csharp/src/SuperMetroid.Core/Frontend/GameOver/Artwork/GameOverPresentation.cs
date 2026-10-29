using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>
/// Immutable game-over artwork, static text layout, palettes and screen anchors the game binds. The actors are drawn
/// from the installed game-over sheet, whose compositions belong to the import pipeline that reads the installed
/// presentation (#1463). Menu control, answer semantics, music and Baby sequence dispatch stay compiled in the
/// application.
/// </summary>
public abstract class GameOverPresentation
{
    /// <summary>Stores an authored tilemap only when it differs from the calculated stock game-over text page.</summary>
    internal readonly BackgroundTile[]? tilemap;
    /// <summary>Installed idle and crying color sets for the Baby's fixed CGRAM row.</summary>
    private readonly GameOverBabyPaint babyPalettes;

    /// <summary>Retains compiled artwork and screen anchors, eliding an unchanged stock tilemap.</summary>
    /// <param name="document">The validated layout.</param>
    /// <param name="tilemap">The compiled game-over tilemap.</param>
    /// <param name="babyPalettes">The Baby's idle and crying colours.</param>
    /// <param name="contentIdentity">The identity of the selected presentation's bytes.</param>
    internal GameOverPresentation(GameOverLayoutDocument document, BackgroundTile[] tilemap, GameOverBabyPaint babyPalettes,
        string contentIdentity)
    {
        for (int cell = 0; cell < GameOverPresentationDefinitions.TilemapCellCount; cell++)
            if (tilemap[cell] != GameOverText.TilemapTile(cell))
            {
                this.tilemap = tilemap;
                break;
            }
        this.babyPalettes = babyPalettes;
        BabyAnchor = document.BabyAnchor;
        CursorX = document.CursorX;
        YesCursorY = document.YesCursorY;
        NoCursorY = document.NoCursorY;
        BabyPaletteIndex = document.BabyPalette;
        EggPaletteIndex = document.EggPalette;
        CursorPaletteIndex = document.CursorPalette;
        CursorFrameDuration = document.CursorFrameDuration;
        ContentIdentity = contentIdentity;
    }

    /// <summary>Uppercase SHA-256 digest of the loaded JSON bytes, identifying the selected presentation including its encoding.</summary>
    public string ContentIdentity { get; }
    /// <summary>Shared drawing origin of the Baby and egg compositions in screen pixels, with both coordinates in 0-255.</summary>
    public Position16<SpritePixels> BabyAnchor { get; }
    /// <summary>Missile-cursor horizontal screen coordinate in pixels, shared by both answers and bounded to 0-255.</summary>
    public int CursorX { get; }
    /// <summary>Missile-cursor vertical screen coordinate in pixels for the Yes answer, bounded to 0-255.</summary>
    public int YesCursorY { get; }
    /// <summary>Missile-cursor vertical screen coordinate in pixels for the No answer, bounded to 0-255.</summary>
    public int NoCursorY { get; }
    /// <summary>OBJ palette selector 0-7 used when drawing the Baby composition; palette animation writes to its fixed native CGRAM destination.</summary>
    public int BabyPaletteIndex { get; }
    /// <summary>OBJ palette selector 0-7 used when drawing the egg at the shared Baby anchor.</summary>
    public int EggPaletteIndex { get; }
    /// <summary>OBJ palette selector 0-7 used by all four missile-cursor frames.</summary>
    public int CursorPaletteIndex { get; }
    /// <summary>Menu updates per frame of the looping missile cursor, in the range 1-65535.</summary>
    public int CursorFrameDuration { get; }

    /// <summary>Copies the complete game-over tilemap into the menu's BG1, calculating the stock text tiles when the loaded page has no edits.</summary>
    /// <param name="destination">The menu BG1 tilemap.</param>
    internal void LoadTilemapTo(Span<BackgroundTile> destination)
    {
        if (tilemap is not null)
        {
            MenuTilemaps.Load(destination, tilemap);
            return;
        }
        Ensure.LengthEqual(destination, GameOverPresentationDefinitions.TilemapCellCount);
        for (int cell = 0; cell < GameOverPresentationDefinitions.TilemapCellCount; cell++)
            destination[cell] = GameOverText.TilemapTile(cell);
    }

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
            new Position16<SpritePixels>(checked((ushort)CursorX), checked((ushort)(selectNo ? NoCursorY : YesCursorY))));
    }

    /// <summary>Writes one selected sixteen-colour Baby animation palette over the Baby's sprite palette row.</summary>
    /// <param name="colours">The screen's colours.</param>
    /// <param name="palette">Idle or one of the three crying palettes selected by the compiled Baby sequence.</param>
    /// <exception cref="ArgumentOutOfRangeException">The palette identity is unsupported.</exception>
    public void ApplyBabyPalette(ScreenPalette colours, GameOverBabyPalette palette)
    {
        _ = GameOverPresentationNames.BabyPaletteName(palette);
        ColourRun row = Ensure.NotNull(colours).Colours(ScreenColourGroup.GameOverBaby);
        for (int index = 0; index < row.Count; index++)
            row.Write(index, babyPalettes.Read(palette, index));
    }
}
