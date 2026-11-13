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

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the title sheet (#1392): each title composition over the title OBJ page, its palette-inheriting parts in the
/// sprite set of the colouring the title draws it in (#1513), coloured as the title loads its palette, with a shade for each colouring the compositions draw in and for
/// the copyright's colours as the skip restores them (#1512).
/// </summary>
internal static class TitleSheetComposer
{

    /// <summary>What the title sheet composes.</summary>
    internal static SpriteSheetSources Sources(ImportAreaMapPresentation maps) =>
        Sources(Ensure.NotNull(maps).TitleGraphics, maps.TitlePalette);

    /// <summary>What the title sheet composes from <paramref name="graphics"/>, coloured with <paramref name="palette"/>.</summary>
    internal static SpriteSheetSources Sources(TitleGraphicsCharacters graphics, TitlePalettePresentation palette)
    {
        TitleGraphicsCharacters characters = Ensure.NotNull(graphics);
        Ensure.NotNull(palette);
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
