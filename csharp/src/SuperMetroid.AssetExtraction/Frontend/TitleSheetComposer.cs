using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the title sheet (#1392): each title composition over the title OBJ page, its parts in the sheet's frame
/// palette, coloured as the title loads its palette. A draw's palette places them in its row.
/// </summary>
internal static class TitleSheetComposer
{
    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>What the title sheet composes.</summary>
    internal static SpriteSheetSources Sources(AreaMapPresentationCatalog maps) =>
        Sources(Ensure.NotNull(maps).TitleGraphics, maps.TitlePalette);

    /// <summary>What the title sheet composes from <paramref name="graphics"/>, coloured with <paramref name="palette"/>.</summary>
    internal static SpriteSheetSources Sources(TitleGraphicsPresentation graphics, TitlePalettePresentation palette)
    {
        Ensure.NotNull(graphics);
        Ensure.NotNull(palette);
        List<SheetFrameSource> frames = [.. TitleSpriteDefinitions.All.Select(sprite =>
        {
            string name = TitleSpriteDefinitions.FrameName(sprite);
            return new SheetFrameSource(name, graphics.Frame(sprite, name), TitleGraphicsCharacters.Of(graphics).ObjectPage, FrontendSheetPalette.FramePalette);
        })];
        var colours = new ScreenPalette();
        palette.Apply(colours);
        return new SpriteSheetSources(frames, colours.Colors[SpriteColours..].ToArray(),
            ObjectSelectionCodec.Sizes(TitleSourceDefinitions.ObjectSizeAndBaseSelector), PartsKeepOwnRows: true);
    }
}
