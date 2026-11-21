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
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the title sheet (#1392): each title composition over the title OBJ page, its palette-inheriting parts in the
/// sprite set of the colouring the title draws it in (#1513), coloured as the title loads its palette, with a shade for each colouring the compositions draw in, for
/// the copyright's colours as the skip restores them (#1512) and for each record the logo's and copyright's fades show (#1714).
/// </summary>
internal static class TitleSheetComposer
{
    /// <summary>The sprite colourings in the order the title sheet lists their drawn shades (#2567): text, logo, copyright.</summary>
    private static readonly IReadOnlyList<TitleSpriteColouring> Colourings = Ensure.EveryMemberOnce<TitleSpriteColouring>(
    [
        TitleSpriteColouring.Text,
        TitleSpriteColouring.Logo,
        TitleSpriteColouring.Copyright,
    ]);


    /// <summary>What the title sheet composes.</summary>
    internal static SpriteSheetSources Sources(ImportAreaMapPresentation maps) =>
        Sources(Ensure.NotNull(maps).TitleGraphics, maps.TitlePalette, maps.RoomPaletteFx);

    /// <summary>
    /// What the title sheet composes from <paramref name="graphics"/>, coloured with <paramref name="palette"/>, its fades'
    /// records coloured from <paramref name="paletteFx"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">A fade's record has no colour in the import's palette-effect colours, or writes past its colours.</exception>
    private static SpriteSheetSources Sources(TitleGraphicsCharacters graphics, TitlePalettePresentation palette, IPaletteFxColorSource paletteFx)
    {
        TitleGraphicsCharacters characters = Ensure.NotNull(graphics);
        Ensure.NotNull(palette);
        Ensure.NotNull(paletteFx);
        // Each composition's palette-inheriting parts are pictured in the sprite set of the one colouring the title draws
        // it in (#1513), so the sheet shows every frame in its own colours. A card's text is pictured as the letters it
        // types, each once (#1979): a letter is the parts its typing step adds to the step before it, and a step adding none
        // types a space, which the sheet holds as showing nothing.
        var frames = new List<SheetFrameSource>();
        var blank = new List<string>();
        foreach (TitleSprite sprite in TitleGraphicsFormat.Sprites)
        {
            string name = TitleSpriteDefinitions.FrameName(sprite);
            frames.Add(new SheetFrameSource(name, characters.Frame(sprite, name), characters.ObjectPage,
                TitleSheetInks.SpriteSet(TitleSpriteColourings.Of(sprite))));
        }
        foreach (TitleCardDefinition card in TitleCards.All)
        {
            IReadOnlyList<TitleCardTyping> typings = TitleCards.Typings(card.Card);
            SpriteFramePart[][] letters = TypedTextGlyphs.Of(card.Card.ToString(),
                [.. typings.Select(typed => characters.Frame(typed, TitleSpriteDefinitions.LetterFrameName(typed)))],
                Offset16<ScreenPixels>.Of(TitleCardDefinitions.TypedScroll.X, TitleCardDefinitions.TypedScroll.Y));
            for (int letter = 0; letter < letters.Length; letter++)
            {
                string name = TitleSpriteDefinitions.LetterFrameName(typings[letter]);
                if (letters[letter].Length == 0)
                    blank.Add(name);
                else
                    frames.Add(new SheetFrameSource(name, new SpriteFrame(name, [.. letters[letter]]), characters.ObjectPage,
                        TitleSheetInks.SpriteSet(TitleSpriteColourings.Of(typings[letter]))));
            }
        }
        // The title's colours run in cartridge order: its background colours, then its sprite colours.
        RgbColor[] sprites = palette.Colours.AsSpan()[ImageColourGroup.HalfColours..].ToArray();
        // Each colouring's drawn shade shows its own set's colours over its own set's inks, carrying whether its draws
        // take part in colour math.
        var shades = new List<SheetShade>();
        foreach (TitleSpriteColouring colouring in Colourings)
        {
            int set = TitleSheetInks.SpriteSet(colouring);
            shades.Add(SheetShade.DrawnRun(TitleShades.Drawn(colouring), set * TitleSheetInks.GroupColours,
                sprites.AsSpan(set * TitleSheetInks.GroupColours, TitleSheetInks.GroupColours), set));
        }
        int copyright = TitleSheetInks.SpriteSet(TitleSequenceDefinitions.Sprites.CopyrightColouring) * TitleSheetInks.GroupColours;
        shades.Add(SheetShade.Of(TitleShades.RestoredCopyright,
        [
            KeyValuePair.Create(copyright + TitleSheetInks.CopyrightWhiteColour, palette.SkipCopyrightWhite),
            KeyValuePair.Create(copyright + TitleSheetInks.CopyrightRedColour, palette.SkipCopyrightTint),
        ]));
        // The logo and the copyright show each record their fades show over the inks each fade recolours (#1714), laid
        // over their drawn shades.
        foreach (ColourEffectColours faded in TitleSheetInks.Fades)
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

    /// <summary>The frames the title sheet holds: each card's letters, and the logo and copyright whole (#1513, #1979).</summary>
    internal static IEnumerable<string> FrameNames() =>
        TitleGraphicsFormat.Sprites.Select(TitleSpriteDefinitions.FrameName)
            .Concat(TitleGraphicsFormat.Typings.Select(TitleSpriteDefinitions.LetterFrameName));
}
