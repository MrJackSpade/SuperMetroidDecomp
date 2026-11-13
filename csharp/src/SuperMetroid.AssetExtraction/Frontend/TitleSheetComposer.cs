using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the title sheet (#1392): each title composition over the title OBJ page, its palette-inheriting parts in the
/// sprite set of the colouring the title draws it in (#1513), coloured as the title loads its palette, with a shade for each colouring the compositions draw in, for
/// the copyright's colours as the skip restores them (#1512) and for each record the logo's and copyright's fades show (#1714).
/// </summary>
internal static class TitleSheetComposer
{

    /// <summary>What the title sheet composes.</summary>
    internal static SpriteSheetSources Sources(ImportAreaMapPresentation maps) =>
        Sources(Ensure.NotNull(maps).TitleGraphics, maps.TitlePalette, maps.RoomPaletteFx);

    /// <summary>
    /// What the title sheet composes from <paramref name="graphics"/>, coloured with <paramref name="palette"/>, its fades'
    /// records coloured from <paramref name="paletteFx"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">A fade's record has no installed colour, or writes past its colours.</exception>
    internal static SpriteSheetSources Sources(TitleGraphicsCharacters graphics, TitlePalettePresentation palette, IPaletteFxColorSource paletteFx)
    {
        TitleGraphicsCharacters characters = Ensure.NotNull(graphics);
        Ensure.NotNull(palette);
        Ensure.NotNull(paletteFx);
        // Each composition's palette-inheriting parts are pictured in the sprite set of the one colouring the title draws
        // it in (#1513), so the sheet shows every frame in its own colours. A card's reveals are pictured as the glyphs they
        // type, each once: a reveal's glyph is the parts it adds to the reveal before it, and a reveal adding none types a
        // space, which the sheet holds as showing nothing.
        var frames = new List<SheetFrameSource>();
        var blank = new List<string>();
        foreach (TitleSprite sprite in TitleGraphicsFormat.Sprites)
        {
            if (TitleTypedCards.Find(sprite) is not null)
                continue;
            string name = TitleSpriteDefinitions.FrameName(sprite);
            frames.Add(new SheetFrameSource(name, characters.Frame(sprite, name), characters.ObjectPage,
                TitleSheetInks.SpriteSet(TitleSpriteColourings.Of(sprite))));
        }
        foreach (TitleSequencePhase card in TitleTypedCards.Cards)
        {
            IReadOnlyList<TitleSprite> reveals = TitleTypedCardSteps.Of(card);
            SpriteFramePart[][] glyphs = TypedTextGlyphs.Of(card.ToString(),
                [.. reveals.Select(sprite => characters.Frame(sprite, TitleSpriteDefinitions.FrameName(sprite)))],
                Offset16<ScreenPixels>.Of(TitleCardDefinitions.TypedScroll.X, TitleCardDefinitions.TypedScroll.Y));
            for (int glyph = 0; glyph < glyphs.Length; glyph++)
            {
                string name = TitleSpriteDefinitions.GlyphFrameName(card, glyph);
                if (glyphs[glyph].Length == 0)
                    blank.Add(name);
                else
                    frames.Add(new SheetFrameSource(name, new SpriteFrame(name, [.. glyphs[glyph]]), characters.ObjectPage,
                        TitleSheetInks.SpriteSet(TitleSpriteColourings.Of(reveals[glyph]))));
            }
        }
        // The title's colours run in cartridge order: its background colours, then its sprite colours.
        RgbColor[] sprites = palette.Colours.AsSpan()[ImageColourGroup.HalfColours..].ToArray();
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
        // The logo and the copyright show each record their fades show over the inks each fade recolours (#1714), laid
        // over their drawn shades.
        foreach (PaletteFxColours faded in TitleSheetInks.Fades)
        {
            shades.AddRange(PaletteFxScreenShades.Of([TitleSequenceDefinitions.Fades.Logo, TitleSequenceDefinitions.Fades.Copyright],
                faded, TitleSheetInks.Fade(faded), paletteFx));
        }
        return new SpriteSheetSources(frames, sprites,
            ObjectSelectionCodec.Sizes(TitleSourceDefinitions.ObjectSizeAndBaseSelector), PartsKeepOwnRows: true, Blank: blank)
        {
            Shades = shades,
        };
    }

    /// <summary>The frames the title sheet holds: each card's glyphs, and the logo and copyright whole (#1513).</summary>
    internal static IEnumerable<string> FrameNames() =>
        TitleGraphicsFormat.Sprites.Where(sprite => TitleTypedCards.Find(sprite) is null).Select(TitleSpriteDefinitions.FrameName)
            .Concat(TitleTypedCards.Cards.SelectMany(card => Enumerable.Range(0, TitleTypedCardSteps.Of(card).Count)
                .Select(glyph => TitleSpriteDefinitions.GlyphFrameName(card, glyph))));
}
