using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
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
/// palette, coloured as the title loads its palette, with a shade for each colouring the compositions draw in and for
/// the copyright's colours as the skip restores them (#1512).
/// </summary>
internal static class TitleSheetComposer
{

    /// <summary>What the title sheet composes.</summary>
    internal static SpriteSheetSources Sources(AreaMapPresentation maps) =>
        Sources(Ensure.NotNull(maps).TitleGraphics, InstalledAreaMapPresentation.TitlePaletteOf(maps));

    /// <summary>What the title sheet composes from <paramref name="graphics"/>, coloured with <paramref name="palette"/>.</summary>
    internal static SpriteSheetSources Sources(TitleGraphicsPresentation graphics, TitlePalettePresentation palette)
    {
        Ensure.NotNull(graphics);
        Ensure.NotNull(palette);
        TitleGraphicsCharacters characters = TitleGraphicsCharacters.Of(graphics);
        List<SheetFrameSource> frames = [.. TitleGraphicsFormat.Sprites.Select(sprite =>
        {
            string name = TitleSpriteDefinitions.FrameName(sprite);
            return new SheetFrameSource(name, characters.Frame(sprite, name), characters.ObjectPage, FrontendSheetPalette.FrameColourGroup.Number);
        })];
        // The title's colours run in cartridge order: its background colours, then its sprite colours.
        RgbColor[] sprites = palette.Colours.AsSpan()[SpriteImage.SpriteColourCount..].ToArray();
        // A frame's palette-inheriting parts are coloured in its frame group's inks; each shade recolours those inks.
        var shades = new List<SheetShade>();
        foreach (TitleSpriteColouring colouring in Enum.GetValues<TitleSpriteColouring>())
        {
            shades.Add(SheetShade.DrawnRun(TitleShades.Drawn(colouring), FrameInk,
                sprites.AsSpan(TitleSheetInks.SpriteSet(colouring) * TitleSheetInks.GroupColours, TitleSheetInks.GroupColours),
                TitleSheetInks.SpriteSet(colouring)));
        }
        shades.Add(new SheetShade(TitleShades.RestoredCopyright,
            [FrameInk + TitleSheetInks.CopyrightWhiteColour, FrameInk + TitleSheetInks.CopyrightRedColour],
            [palette.SkipCopyrightWhite, palette.SkipCopyrightRed]));
        return new SpriteSheetSources(frames, sprites,
            ObjectSelectionCodec.Sizes(TitleSourceDefinitions.ObjectSizeAndBaseSelector), PartsKeepOwnRows: true)
        {
            Shades = shades,
        };
    }

    /// <summary>The first ink of the frame group that a title frame's palette-inheriting parts are coloured in.</summary>
    private static int FrameInk => FrontendSheetPalette.FrameColourGroup.Number * TitleSheetInks.GroupColours;
}
