using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

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
        SpriteArtwork page = MapSpriteArtwork.Of(maps.Sprites).Page;
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
        var cgram = new SnesCgram();
        new CgramPalettes(cgram).LoadImage(ScreenColourImage.Encode(maps.Palettes.Pause));
        return new SpriteSheetSources(frames, cgram.Colors[SpriteColours..].ToArray(), ObjectSelectionCodec.Sizes(PauseSheetDefinitions.ObjectSelection),
            PartsKeepOwnRows: true);
    }

    /// <summary>The frames the pause sheet of <paramref name="maps"/> holds.</summary>
    internal static IEnumerable<string> FrameNames(AreaMapPresentation maps) =>
        Ensure.NotNull(maps).PauseSelectors.FrameNames.Select(PauseSelectorAnchors.SheetFrameName)
            .Concat(PauseReserveTankParts.Frames().Select(frame => PauseReserveTankParts.SheetFrameName(frame.Name)));
}
