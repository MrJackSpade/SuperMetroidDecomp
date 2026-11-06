using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the title sheet (#1392): each title composition over the title OBJ page, its palette-inheriting parts in the
/// sprite set of the colouring the title draws it in (#1513), coloured as the title loads its palette, with a shade for each colouring the compositions draw in and for
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
        // Each composition's palette-inheriting parts are pictured in the sprite set of the one colouring the title draws
        // it in (#1513), so the sheet shows every frame in its own colours.
        List<SheetFrameSource> frames = [.. TitleGraphicsFormat.Sprites.Select(sprite =>
        {
            string name = TitleSpriteDefinitions.FrameName(sprite);
            return new SheetFrameSource(name, characters.Frame(sprite, name), characters.ObjectPage,
                TitleSheetInks.SpriteSet(TitleSpriteColourings.Of(sprite)));
        })];
        // The title's colours run in cartridge order: its background colours, then its sprite colours.
        RgbColor[] sprites = palette.Colours.AsSpan()[SpriteImage.SpriteColourCount..].ToArray();
        // Each colouring's drawn shade shows its own set's colours over its own set's inks, carrying whether its draws
        // take part in colour math.
        var shades = new List<SheetShade>();
        foreach (TitleSpriteColouring colouring in Enum.GetValues<TitleSpriteColouring>())
        {
            int set = TitleSheetInks.SpriteSet(colouring);
            shades.Add(SheetShade.DrawnRun(TitleShades.Drawn(colouring), set * TitleSheetInks.GroupColours,
                sprites.AsSpan(set * TitleSheetInks.GroupColours, TitleSheetInks.GroupColours), set));
        }
        int copyright = TitleSheetInks.SpriteSet(TitleSequenceDefinitions.Sprites.CopyrightColouring) * TitleSheetInks.GroupColours;
        shades.Add(new SheetShade(TitleShades.RestoredCopyright,
            [copyright + TitleSheetInks.CopyrightWhiteColour, copyright + TitleSheetInks.CopyrightRedColour],
            [palette.SkipCopyrightWhite, palette.SkipCopyrightRed]));
        return new SpriteSheetSources(frames, sprites,
            ObjectSelectionCodec.Sizes(TitleSourceDefinitions.ObjectSizeAndBaseSelector), PartsKeepOwnRows: true)
        {
            Shades = shades,
        };
    }
}
