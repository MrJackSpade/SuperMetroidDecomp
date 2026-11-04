using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the pause sheet (#1392): each equipment-selector frame the selector presentation names, in its palette, and
/// each reserve-tank strip visual, in the strip's, over the menu sprite page and coloured as pause loads its palette,
/// with a shade for each frame of the highlight cycle over the row it recolours, which the selector and the strip are
/// drawn in (#1512).
/// </summary>
internal static class PauseSheetComposer
{
    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;


    /// <summary>What the pause sheet composes.</summary>
    internal static SpriteSheetSources Sources(AreaMapPresentation maps) =>
        Sources(Ensure.NotNull(maps).PauseSelectors, maps.PauseReserveTanks, maps);

    /// <summary>
    /// What the pause sheet composes from <paramref name="selectors"/> and <paramref name="tanks"/>, over the menus of
    /// <paramref name="maps"/>.
    /// </summary>
    internal static SpriteSheetSources Sources(PauseSelectorPresentation selectors, PauseReserveTankPresentation tanks,
        AreaMapPresentation maps)
    {
        Ensure.NotNull(selectors);
        Ensure.NotNull(tanks);
        Ensure.NotNull(maps);
        SpriteArtwork page = MapSpriteArtwork.Of(InstalledAreaMapPresentation.SpritesOf(maps)).Page;
        PauseReserveTankSourcePresentation tankSource = PauseReserveTankSourcePresentation.Of(tanks);
        PauseSelectorSourcePresentation selectorSource = PauseSelectorSourcePresentation.Of(selectors);
        List<SheetFrameSource> frames = [.. selectorSource.Visuals.Select(visual =>
        {
            string name = PauseSelectorAnchors.SheetFrameName(visual.Name);
            return new SheetFrameSource(name, visual.ToFrame(name), page, selectorSource.SpriteSet);
        })];
        frames.AddRange(PauseReserveTankParts.Frames().Select(frame =>
        {
            string name = PauseReserveTankParts.SheetFrameName(frame.Name);
            return new SheetFrameSource(name, tankSource.ToFrame(frame.Id, name), page, tankSource.SpriteSet);
        }));
        // The pause screen's palette animation rewrites the highlighted sprites' row every frame of its cycle, on the map
        // and equipment pages alike, so every ink of that row shows each frame's colours.
        MapPaletteCycle highlight = InstalledAreaMapPresentation.HighlightColoursOf(maps);
        int highlightInk = MapSpriteColouringSets.SpriteSet(MapSpriteColouring.Highlighted) * GroupColours;
        var shades = new List<SheetShade>();
        for (int frame = 0; frame < highlight.FrameCount; frame++)
            shades.Add(SheetShade.Run(MapSpriteShades.Highlight(frame), highlightInk, highlight.Colours(frame)));
        return new SpriteSheetSources(frames, InstalledAreaMapPresentation.PalettesOf(maps).Pause[SpriteColours..].ToArray(), ObjectSelectionCodec.Sizes(PauseSheetDefinitions.ObjectSelection),
            PartsKeepOwnRows: true)
        {
            Shades = shades,
        };
    }

    /// <summary>The colours of one sprite colour group.</summary>
    private const int GroupColours = 16;

    /// <summary>The frames the pause sheet of <paramref name="maps"/> holds.</summary>
    internal static IEnumerable<string> FrameNames(AreaMapPresentation maps) =>
        PauseSelectorSourcePresentation.Of(Ensure.NotNull(maps).PauseSelectors).FrameNames.Select(PauseSelectorAnchors.SheetFrameName)
            .Concat(PauseReserveTankParts.Frames().Select(frame => PauseReserveTankParts.SheetFrameName(frame.Name)));
}
