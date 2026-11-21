using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the game-over sheet (#1392): the Baby's, egg's and cursor's compositions over the menu sprite page, each
/// in the palette its draw selects, coloured as the screen loads the menu palette with the Baby idle, and with a shade
/// for each of the Baby's colourings (#1512).
/// </summary>
internal static class GameOverSheetComposer
{
    /// <summary>The Baby's frames in the sheet's and the document's order (#2567): closed, middle, open.</summary>
    internal static readonly IReadOnlyList<GameOverBabyFrame> BabyFrames = Ensure.EveryMemberOnce<GameOverBabyFrame>(
    [
        GameOverBabyFrame.Closed,
        GameOverBabyFrame.Middle,
        GameOverBabyFrame.Open,
    ]);

    /// <summary>The Baby's colourings in the order the sheet lists their shades (#2567): idle, then each cry's.</summary>
    private static readonly IReadOnlyList<GameOverBabyColouring> BabyColourings = Ensure.EveryMemberOnce<GameOverBabyColouring>(
    [
        GameOverBabyColouring.Idle,
        GameOverBabyColouring.ClosedCry,
        GameOverBabyColouring.MiddleCry,
        GameOverBabyColouring.OpenCry,
    ]);

    /// <summary>The first sprite colour of a whole-screen colour image.</summary>
    private const int SpriteColours = ImageColourGroup.HalfColours;
    /// <summary>The colours of one sprite colour group.</summary>
    private const int GroupColours = ImageColourGroup.Colours;

    /// <summary>What the game-over sheet composes.</summary>
    internal static SpriteSheetSources Sources(ImportAreaMapPresentation maps)
    {
        Ensure.NotNull(maps);
        GameOverSourcePresentation gameOver = maps.GameOverSource;
        SpriteArtwork page = MapSpriteArtwork.Of(maps.Sprites).Page;
        var frames = new List<SheetFrameSource>();
        foreach (GameOverBabyFrame frame in BabyFrames)
            frames.Add(Frame(GameOverPresentationNames.BabyFrameName(frame), gameOver.babySprites[frame], page, gameOver.BabyPaletteIndex));
        frames.Add(Frame(GameOverPresentationDefinitions.EggFrame, gameOver.eggSprite, page, gameOver.EggPaletteIndex));
        foreach (MenuMissileFrame frame in MenuMissileAnimationDefinitions.Roll)
            frames.Add(Frame(MenuMissileFrames.Name(frame), gameOver.cursorSprites[frame], page, gameOver.CursorPaletteIndex));
        RgbColor[] colours = maps.Palettes.FileSelect[SpriteColours..].ToArray();
        int babyInk = gameOver.BabyPaletteIndex * GroupColours;
        gameOver.BabyPaint.Colours(GameOverBabyColouring.Idle).CopyTo(colours.AsSpan(babyInk, GroupColours));
        return new SpriteSheetSources(frames, colours, ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection), PartsKeepOwnRows: true)
        {
            Shades = [.. BabyColourings.Select(palette =>
                SheetShade.Run(GameOverBabyShades.Colours(palette), babyInk, gameOver.BabyPaint.Colours(palette)))],
        };
    }

    /// <summary>A composition as a sheet frame, its palette-inheriting parts in the palette its draw selects.</summary>
    private static SheetFrameSource Frame(string name, SpriteComposition composition, SpriteArtwork page, int palette) =>
        new(name, composition.ToFrame(name, 0), page, palette);
}
