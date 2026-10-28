using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the map sprite sheet (#1392): each map marker, elevator and area label, arrow and position indicator frame
/// over the menu sprite page, its palette-inheriting parts in the sheet's frame palette and the others in their own,
/// coloured as file select loads the menu palette.
/// </summary>
internal static class MapSpriteSheetComposer
{
    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>What the map sprite sheet composes.</summary>
    internal static SpriteSheetSources Sources(AreaMapPresentation maps) => Sources(Ensure.NotNull(maps).Sprites, maps);

    /// <summary>What the map sprite sheet composes from <paramref name="sprites"/>, coloured with the menus of <paramref name="maps"/>.</summary>
    internal static SpriteSheetSources Sources(MapSpriteCompositions sprites, AreaMapPresentation maps)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(maps);
        List<SheetFrameSource> frames = [.. MapSpriteNames.Frames.Select(frame =>
            new SheetFrameSource(frame.Name, sprites.Frame(frame.Id), MapSpriteArtwork.Of(sprites).Page, FrontendSheetPalette.FramePalette))];
        var cgram = new SnesCgram();
        new CgramPalettes(cgram).LoadImage(maps.Palettes.FileSelect);
        return new SpriteSheetSources(frames, cgram.Colors[SpriteColours..].ToArray(), ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection), PartsKeepOwnRows: true);
    }
}
