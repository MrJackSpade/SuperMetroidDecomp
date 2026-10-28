using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the pause sheet (#1392): each equipment-selector frame the selector presentation names, in its palette, and
/// each reserve-tank strip visual, in the strip's, over the menu sprite page and coloured as pause loads its palette.
/// </summary>
internal static class PauseSheetComposer
{
    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>What the pause sheet composes.</summary>
    internal static SpriteSheetSources Sources(AreaMapPresentationCatalog maps) =>
        Sources(Ensure.NotNull(maps).PauseSelectors, maps.PauseReserveTanks, maps);

    /// <summary>
    /// What the pause sheet composes from <paramref name="selectors"/> and <paramref name="tanks"/>, over the menus of
    /// <paramref name="maps"/>.
    /// </summary>
    internal static SpriteSheetSources Sources(PauseSelectorPresentation selectors, PauseReserveTankPresentation tanks,
        AreaMapPresentationCatalog maps)
    {
        Ensure.NotNull(selectors);
        Ensure.NotNull(tanks);
        Ensure.NotNull(maps);
        SpriteArtwork page = MapSpriteArtwork.Of(maps.Sprites).Page;
        List<SheetFrameSource> frames = [.. selectors.Visuals.Select(visual =>
        {
            string name = PauseSelectorDefinitions.SheetFrameName(visual.Name);
            return new SheetFrameSource(name, visual.ToFrame(name), page, selectors.ObjectPalette);
        })];
        frames.AddRange(PauseReserveTankDefinitions.Frames().Select(frame =>
        {
            string name = PauseReserveTankDefinitions.SheetFrameName(frame.Name);
            return new SheetFrameSource(name, tanks.ToFrame(frame.Id, name), page, tanks.Palette);
        }));
        var cgram = new SnesCgram();
        new CgramPalettes(cgram).LoadImage(maps.Palettes.Pause);
        return new SpriteSheetSources(frames, cgram.Colors[SpriteColours..].ToArray(), ObjectSelectionCodec.Sizes(PauseMenuLayout.ObjectSelection),
            PartsKeepOwnRows: true);
    }

    /// <summary>The frames the pause sheet of <paramref name="maps"/> holds.</summary>
    internal static IEnumerable<string> FrameNames(AreaMapPresentationCatalog maps) =>
        Ensure.NotNull(maps).PauseSelectors.Visuals.Select(visual => PauseSelectorDefinitions.SheetFrameName(visual.Name))
            .Concat(PauseReserveTankDefinitions.Frames().Select(frame => PauseReserveTankDefinitions.SheetFrameName(frame.Name)));
}
