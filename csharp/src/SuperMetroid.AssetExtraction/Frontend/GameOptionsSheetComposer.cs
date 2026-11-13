using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the options sheet (#1392): each page heading and cursor frame over the menu sprite page in the cursor
/// palette, coloured as the screen loads the menu palette. The Japanese text colours recolour it through its shade.
/// </summary>
internal static class GameOptionsSheetComposer
{
    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>What the options sheet composes.</summary>
    internal static SpriteSheetSources Sources(ImportAreaMapPresentation maps) => Sources(Ensure.NotNull(maps).GameOptions, maps);

    /// <summary>What the options sheet composes from <paramref name="options"/>, over the menus of <paramref name="maps"/>.</summary>
    internal static SpriteSheetSources Sources(GameOptionsSourcePresentation options, ImportAreaMapPresentation maps)
    {
        Ensure.NotNull(options);
        Ensure.NotNull(maps);
        SpriteArtwork page = MapSpriteArtwork.Of(maps.Sprites).Page;
        GameOptionsSourcePresentation source = options;
        int palette = source.CursorPalette;
        var frames = new List<SheetFrameSource>();
        foreach (GameOptionsPage menuPage in GameOptionsPresentationFormat.MenuPages)
        {
            string name = GameOptionsNames.HeadingSpriteName(menuPage);
            frames.Add(new(name, source.headingSprites[menuPage].ToFrame(name, 0), page, palette));
        }
        for (int frame = 0; frame < GameOptionsPresentationFormat.CursorFrameCount; frame++)
        {
            string name = GameOptionsNames.CursorSpriteName(frame);
            frames.Add(new(name, source.cursorSprites[frame].ToFrame(name, 0), page, palette));
        }
        return new SpriteSheetSources(frames, maps.Palettes.FileSelect[SpriteColours..].ToArray(), ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection), PartsKeepOwnRows: true);
    }
}
