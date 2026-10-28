using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the game-over sheet (#1392): the Baby's, egg's and cursor's compositions over the menu sprite page, each
/// in the palette its draw selects, coloured as the screen loads the menu palette with the Baby idle.
/// </summary>
internal static class GameOverSheetComposer
{
    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>What the game-over sheet composes.</summary>
    internal static SpriteSheetSources Sources(AreaMapPresentation maps)
    {
        Ensure.NotNull(maps);
        GameOverPresentation gameOver = maps.GameOver;
        SpriteArtwork page = MapSpriteArtwork.Of(maps.Sprites).Page;
        var frames = new List<SheetFrameSource>();
        foreach (GameOverBabyFrame frame in Enum.GetValues<GameOverBabyFrame>())
            frames.Add(Frame(GameOverPresentationNames.BabyFrameName(frame), gameOver.babySprites[frame], page, gameOver.BabyPaletteIndex));
        frames.Add(Frame(GameOverPresentationDefinitions.EggFrame, gameOver.eggSprite, page, gameOver.EggPaletteIndex));
        for (int frame = 0; frame < GameOverPresentationDefinitions.CursorFrameCount; frame++)
            frames.Add(Frame(GameOverPresentationNames.CursorFrameName(frame), gameOver.cursorSprites[frame], page, gameOver.CursorPaletteIndex));
        var colours = new ScreenPalette();
        colours.Load(maps.Palettes.FileSelect);
        gameOver.ApplyBabyPalette(colours, GameOverBabyPalette.Idle);
        return new SpriteSheetSources(frames, colours.Colors[SpriteColours..].ToArray(), ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection), PartsKeepOwnRows: true);
    }

    /// <summary>A composition as a sheet frame, its palette-inheriting parts in the palette its draw selects.</summary>
    private static SheetFrameSource Frame(string name, SpriteComposition composition, SpriteArtwork page, int palette) =>
        new(name, composition.ToFrame(name, 0), page, palette);
}
