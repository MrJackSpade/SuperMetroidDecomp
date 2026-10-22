using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the options sheet (#1392): each page heading and cursor frame over the menu sprite page in the cursor
/// palette, coloured as the screen loads the menu palette. The Japanese text palette recolours it through its palette map.
/// </summary>
internal static class GameOptionsSheetComposer
{
    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>What the options sheet composes.</summary>
    internal static SpriteSheetSources Sources(AreaMapPresentationCatalog maps) => Sources(Ensure.NotNull(maps).GameOptions, maps);

    /// <summary>What the options sheet composes from <paramref name="options"/>, over the menus of <paramref name="maps"/>.</summary>
    internal static SpriteSheetSources Sources(GameOptionsPresentation options, AreaMapPresentationCatalog maps)
    {
        Ensure.NotNull(options);
        Ensure.NotNull(maps);
        SpriteArtwork page = maps.Sprites.Page;
        int palette = options.CursorPalette;
        var frames = new List<SheetFrameSource>();
        foreach (GameOptionsPage menuPage in GameOptionsPresentationDefinitions.MenuPages)
        {
            string name = GameOptionsPresentationDefinitions.HeadingSpriteName(menuPage);
            frames.Add(new(name, options.headingSprites[menuPage].ToFrame(name, 0), page, palette));
        }
        for (int frame = 0; frame < GameOptionsPresentationDefinitions.CursorFrameCount; frame++)
        {
            string name = GameOptionsPresentationDefinitions.CursorSpriteName(frame);
            frames.Add(new(name, options.cursorSprites[frame].ToFrame(name, 0), page, palette));
        }
        var cgram = new SnesCgram();
        new CgramPalettes(cgram).LoadImage(maps.Palettes.FileSelect);
        return new SpriteSheetSources(frames, cgram.Colors[SpriteColours..].ToArray(), ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection), PartsKeepOwnRows: true);
    }
}
