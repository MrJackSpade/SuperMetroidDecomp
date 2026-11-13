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
    /// <summary>The first sprite colour of a whole-screen colour image.</summary>
    private const int SpriteColours = 128;
    /// <summary>The colours of one sprite colour group.</summary>
    private const int GroupColours = 16;

    /// <summary>What the game-over sheet composes.</summary>
    internal static SpriteSheetSources Sources(ImportAreaMapPresentation maps)
    {
        Ensure.NotNull(maps);
        GameOverSourcePresentation gameOver = maps.GameOverSource;
        SpriteArtwork page = MapSpriteArtwork.Of(maps.Sprites).Page;
        var frames = new List<SheetFrameSource>();
        foreach (GameOverBabyFrame frame in Enum.GetValues<GameOverBabyFrame>())
            frames.Add(Frame(GameOverPresentationNames.BabyFrameName(frame), gameOver.babySprites[frame], page, gameOver.BabyPaletteIndex));
        frames.Add(Frame(GameOverPresentationDefinitions.EggFrame, gameOver.eggSprite, page, gameOver.EggPaletteIndex));
        for (int frame = 0; frame < GameOverPresentationDefinitions.CursorFrameCount; frame++)
            frames.Add(Frame(GameOverPresentationNames.CursorFrameName(frame), gameOver.cursorSprites[frame], page, gameOver.CursorPaletteIndex));
        RgbColor[] colours = maps.Palettes.FileSelect[SpriteColours..].ToArray();
        int babyInk = gameOver.BabyPaletteIndex * GroupColours;
        gameOver.BabyPaint.Colours(GameOverBabyPalette.Idle).CopyTo(colours.AsSpan(babyInk, GroupColours));
        return new SpriteSheetSources(frames, colours, ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection), PartsKeepOwnRows: true)
        {
            Shades = [.. Enum.GetValues<GameOverBabyPalette>().Select(palette =>
                SheetShade.Run(GameOverBabyShades.Colours(palette), babyInk, gameOver.BabyPaint.Colours(palette)))],
        };
    }

    /// <summary>A composition as a sheet frame, its palette-inheriting parts in the palette its draw selects.</summary>
    private static SheetFrameSource Frame(string name, SpriteComposition composition, SpriteArtwork page, int palette) =>
        new(name, composition.ToFrame(name, 0), page, palette);
}
