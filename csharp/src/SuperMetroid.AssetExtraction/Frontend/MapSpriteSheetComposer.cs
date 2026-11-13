using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the map sprite sheet (#1392): each map marker, elevator and area label, arrow and position indicator frame
/// over the menu sprite page, its palette-inheriting parts in its standard colouring's sprite set (#1513) and the others in their own,
/// coloured as file select loads the menu palette, with a shade for each colouring the map sprites draw in on each map
/// screen and for each frame of the highlight cycle (#1512).
/// </summary>
internal static class MapSpriteSheetComposer
{
    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>What the map sprite sheet composes.</summary>
    internal static SpriteSheetSources Sources(ImportAreaMapPresentation maps) => Sources(Ensure.NotNull(maps).Sprites, maps);

    /// <summary>What the map sprite sheet composes from <paramref name="sprites"/>, coloured with the menus of <paramref name="maps"/>.</summary>
    internal static SpriteSheetSources Sources(MapSpriteCompositions sprites, ImportAreaMapPresentation maps)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(maps);
        MapSpriteArtwork artwork = MapSpriteArtwork.Of(sprites);
        // Each sprite is pictured once, its palette-inheriting parts in its standard colouring's set (#1513).
        List<ColouredFrame<MapSpriteColouring>> frames = [.. MapSpriteArtwork.FrameNames.Select(frame =>
        {
            MapSpriteColouring[] colourings = [.. Ensure.Listed(MapSpriteColourings.Drawn, frame.Id)];
            return new ColouredFrame<MapSpriteColouring>(new SheetFrameSource(frame.Name, artwork.Frame(frame.Id), artwork.Page,
                MapSpriteColouringSets.SpriteSet(colourings[0])), colourings);
        })];
        ColouringPictures<MapSpriteColouring> pictures = ColouringPictures<MapSpriteColouring>.Of(frames, MapSpriteColouringSets.SpriteSet);
        MapStaticPalettes palettes = maps.Palettes;
        RgbColor[] fileSelect = palettes.FileSelect[SpriteColours..].ToArray();
        RgbColor[] pause = palettes.Pause[SpriteColours..].ToArray();
        // Each colouring's shade shows its set's colours on each screen over the sets its sprites are pictured in.
        var shades = new List<SheetShade>();
        foreach (MapSpriteColouring colouring in Enum.GetValues<MapSpriteColouring>())
        {
            int set = MapSpriteColouringSets.SpriteSet(colouring);
            shades.Add(Drawn(MapSpriteShades.Drawn(MapSpriteScreen.FileSelect, colouring), fileSelect, set, pictures.SetsOf(colouring)));
            shades.Add(Drawn(MapSpriteShades.Drawn(MapSpriteScreen.Pause, colouring), pause, set, pictures.SetsOf(colouring)));
        }
        // The highlight cycle shows each frame's highlight colour over the highlighted sprites' highlight, in place of their
        // drawn shade; their outline keeps its drawn colour (#1637).
        int highlighted = MapSpriteColouringSets.SpriteSet(MapSpriteColouring.Highlighted);
        MapPaletteCycle highlight = maps.HighlightColours;
        for (int frame = 0; frame < highlight.FrameCount; frame++)
        {
            RgbColor[] colours = fileSelect.AsSpan(highlighted * GroupColours, GroupColours).ToArray();
            foreach (int ink in MapPaletteCycleFormat.HighlightInks)
                colours[ink] = highlight.Highlight(frame);
            shades.Add(SheetShade.DrawnOver(MapSpriteShades.Highlight(frame), highlighted, pictures.SetsOf(MapSpriteColouring.Highlighted),
                colours) with { TakesColourMath = false });
        }
        return new SpriteSheetSources([.. frames.Select(frame => frame.Frame)], fileSelect, ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection),
            PartsKeepOwnRows: true)
        {
            Shades = shades,
        };
    }

    /// <summary>The colours of one sprite colour group.</summary>
    private const int GroupColours = 16;

    /// <summary>
    /// The shade that shows the sprites drawn in the colouring held in sprite set <paramref name="set"/> in that set's
    /// colours of <paramref name="sprites"/>, over the sets <paramref name="pictured"/> holds.
    /// </summary>
    private static SheetShade Drawn(PictureShade shade, RgbColor[] sprites, int set, SortedSet<int> pictured) =>
        SheetShade.DrawnOver(shade, set, pictured, sprites.AsSpan(set * GroupColours, GroupColours));
}
