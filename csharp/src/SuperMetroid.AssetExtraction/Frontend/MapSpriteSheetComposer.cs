using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the map sprite sheet (#1392): each map marker, elevator and area label, arrow and position indicator frame
/// over the menu sprite page, its palette-inheriting parts in the sheet's frame palette and the others in their own,
/// coloured as file select loads the menu palette, with a shade for each colouring the map sprites draw in on each map
/// screen and for each frame of the highlight cycle (#1512).
/// </summary>
internal static class MapSpriteSheetComposer
{
    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>What the map sprite sheet composes.</summary>
    internal static SpriteSheetSources Sources(AreaMapPresentation maps) => Sources(InstalledAreaMapPresentation.SpritesOf(Ensure.NotNull(maps)), maps);

    /// <summary>What the map sprite sheet composes from <paramref name="sprites"/>, coloured with the menus of <paramref name="maps"/>.</summary>
    internal static SpriteSheetSources Sources(MapSpriteCompositions sprites, AreaMapPresentation maps)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(maps);
        MapSpriteArtwork artwork = MapSpriteArtwork.Of(sprites);
        List<SheetFrameSource> frames = [.. MapSpriteArtwork.FrameNames.Select(frame =>
            new SheetFrameSource(frame.Name, artwork.Frame(frame.Id), artwork.Page, FrontendSheetPalette.FrameColourGroup.Number))];
        RgbColor[] fileSelect = SpriteColoursOf(maps.Palettes.FileSelect);
        RgbColor[] pause = SpriteColoursOf(maps.Palettes.Pause);
        // A frame's palette-inheriting parts are coloured in its frame group's inks; each shade recolours those inks.
        var shades = new List<SheetShade>();
        foreach (MapSpriteColouring colouring in Enum.GetValues<MapSpriteColouring>())
        {
            int set = MapSpriteColouringSets.SpriteSet(colouring);
            shades.Add(Drawn(MapSpriteShades.Drawn(MapSpriteScreen.FileSelect, colouring), fileSelect, set));
            shades.Add(Drawn(MapSpriteShades.Drawn(MapSpriteScreen.Pause, colouring), pause, set));
        }
        MapPaletteCycle highlight = InstalledAreaMapPresentation.HighlightColoursOf(maps);
        for (int frame = 0; frame < highlight.FrameCount; frame++)
            shades.Add(SheetShade.Run(MapSpriteShades.Highlight(frame), FrameInk, highlight.Colours(frame)));
        return new SpriteSheetSources(frames, fileSelect, ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection), PartsKeepOwnRows: true)
        {
            Shades = shades,
        };
    }

    /// <summary>The colours of one sprite colour group.</summary>
    private const int GroupColours = 16;

    /// <summary>The first ink of the frame group that a map sprite's palette-inheriting parts are coloured in.</summary>
    private static int FrameInk => FrontendSheetPalette.FrameColourGroup.Number * GroupColours;

    /// <summary>The sprite colours a map screen loads, as the sheet's sprite inks hold them.</summary>
    private static RgbColor[] SpriteColoursOf(ScreenColours screen)
    {
        var cgram = new SnesCgram();
        new CgramPalettes(cgram).LoadImage(ScreenColourImage.Encode(screen));
        return cgram.Colors[SpriteColours..].ToArray();
    }

    /// <summary>The shade that shows a frame's palette-inheriting parts in sprite set <paramref name="set"/> of <paramref name="sprites"/>.</summary>
    private static SheetShade Drawn(PictureShade shade, RgbColor[] sprites, int set) =>
        SheetShade.Run(shade, FrameInk, sprites.AsSpan(set * GroupColours, GroupColours));
}
