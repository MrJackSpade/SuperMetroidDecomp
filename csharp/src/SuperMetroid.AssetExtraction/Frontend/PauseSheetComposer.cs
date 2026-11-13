using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the pause sheet (#1392): each equipment-selector frame the selector presentation names, in its palette, and
/// each reserve-tank strip visual, in the strip's, over the menu sprite page and coloured as pause loads its palette. The
/// selector and the strip show their own colours (#1544): the map highlight cycle shades only the map's highlighted
/// sprites.
/// </summary>
internal static class PauseSheetComposer
{
    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;


    /// <summary>What the pause sheet composes.</summary>
    internal static SpriteSheetSources Sources(ImportAreaMapPresentation maps) =>
        Sources(Ensure.NotNull(maps).PauseSelectors, maps.PauseReserveTanks, maps);

    /// <summary>
    /// What the pause sheet composes from <paramref name="selectors"/> and <paramref name="tanks"/>, over the menus of
    /// <paramref name="maps"/>.
    /// </summary>
    internal static SpriteSheetSources Sources(PauseSelectorSourcePresentation selectors, PauseReserveTankSourcePresentation tanks,
        ImportAreaMapPresentation maps)
    {
        Ensure.NotNull(selectors);
        Ensure.NotNull(tanks);
        Ensure.NotNull(maps);
        SpriteArtwork page = MapSpriteArtwork.Of(maps.Sprites).Page;
        List<SheetFrameSource> frames = [.. selectors.FrameNames.Select(frame =>
        {
            string name = PauseSelectorAnchors.SheetFrameName(frame);
            return new SheetFrameSource(name, selectors.Frame(frame, name), page, selectors.SpriteSet);
        })];
        frames.AddRange(PauseReserveTankParts.Frames().Select(frame =>
        {
            string name = PauseReserveTankParts.SheetFrameName(frame.Name);
            return new SheetFrameSource(name, tanks.ToFrame(frame.Id, name), page, tanks.SpriteSet);
        }));
        return new SpriteSheetSources(frames, maps.Palettes.Pause[SpriteColours..].ToArray(), ObjectSelectionCodec.Sizes(PauseSheetDefinitions.ObjectSelection),
            PartsKeepOwnRows: true);
    }

    /// <summary>The frames the pause sheet of <paramref name="maps"/> holds.</summary>
    internal static IEnumerable<string> FrameNames(InstalledAreaMapPresentation maps) =>
        Ensure.NotNull(maps).PauseSelectors.FrameNames.Select(PauseSelectorAnchors.SheetFrameName)
            .Concat(PauseReserveTankParts.Frames().Select(frame => PauseReserveTankParts.SheetFrameName(frame.Name)));
}
