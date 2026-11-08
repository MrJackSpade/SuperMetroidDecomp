using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// The shades the import pre-colours the opening's sheets in (#1512): every cross-fade level below full of each faded
/// group over the scene pictures and the actors, each actor colouring, and each narration letter colour variant. A level
/// shows each colour scaled by the level and rounded down, as the opening's cross-fader shows it.
/// </summary>
internal static class IntroSheetShades
{
    /// <summary>The shades of every background group at every level below full, over <paramref name="backgrounds"/>.</summary>
    /// <param name="backgrounds">The opening's installed background colours, at the inks the scene pictures colour them at.</param>
    internal static IReadOnlyList<SheetShade> Scene(IReadOnlyList<RgbColor> backgrounds)
    {
        Ensure.NotNull(backgrounds);
        var shades = new List<SheetShade>();
        foreach (IntroBackgroundFade group in Enum.GetValues<IntroBackgroundFade>())
        {
            int[] inks = [.. IntroSheetInks.Background(group)];
            for (int level = 0; level < CinematicPaletteFader.Levels; level++)
                shades.Add(new SheetShade(IntroShades.Faded(group, level), inks, Faded(inks.Select(ink => backgrounds[ink]), level)));
        }
        return shades;
    }

    /// <summary>
    /// The intro sprite sheet's shades over <paramref name="sprites"/> (#1513): each colouring over its own sprite set's inks,
    /// which every frame it draws is coloured in, and every sprite group at every level below full over its inks, both as
    /// its own shade and as each colouring's, whose frames show the group's inks where their set holds them.
    /// </summary>
    /// <param name="sprites">The opening's installed sprite colours, at the inks every frame colours them at.</param>
    internal static IReadOnlyList<SheetShade> Sprites(IReadOnlyList<RgbColor> sprites)
    {
        Ensure.NotNull(sprites);
        var shades = new List<SheetShade>();
        IntroSpriteColouring[] colourings = Enum.GetValues<IntroSpriteColouring>();
        foreach (IntroSpriteColouring colouring in colourings)
        {
            int set = IntroSheetInks.SpriteSet(colouring);
            shades.Add(SheetShade.DrawnRun(IntroShades.Drawn(colouring), set * IntroSheetInks.SetColours,
                [.. sprites.Skip(set * IntroSheetInks.SetColours).Take(IntroSheetInks.SetColours)], set));
        }
        foreach (IntroSpriteFade group in Enum.GetValues<IntroSpriteFade>())
        {
            int[] colours = [.. IntroSheetInks.Sprite(group)];
            for (int level = 0; level < CinematicPaletteFader.Levels; level++)
            {
                shades.Add(new SheetShade(IntroShades.Faded(group, level), colours, Faded(colours.Select(colour => sprites[colour]), level)));
                foreach (IntroSpriteColouring colouring in colourings)
                {
                    int set = IntroSheetInks.SpriteSet(colouring);
                    int[] drawn = [.. colours.Where(colour => colour / IntroSheetInks.SetColours == set)];
                    shades.Add(new SheetShade(IntroShades.Faded(group, level, IntroShades.Drawn(colouring)), drawn,
                        Faded(drawn.Select(colour => sprites[colour]), level)));
                }
            }
        }
        return shades;
    }

    /// <summary>
    /// The narration letters' shades: each colour variant over the inks of the variant the letters are composed in,
    /// showing <paramref name="backgrounds"/>' colours of that variant.
    /// </summary>
    /// <param name="backgrounds">The opening's installed background colours.</param>
    internal static IReadOnlyList<SheetShade> Glow(IReadOnlyList<RgbColor> backgrounds)
    {
        Ensure.NotNull(backgrounds);
        var shades = new List<SheetShade>();
        for (int variant = 0; variant <= CinematicTextGlowDefinitions.FinalVariant; variant++)
        {
            shades.Add(SheetShade.Run(IntroShades.Glow(variant), 0,
                [.. backgrounds.Skip(variant * IntroSheetInks.VariantColours).Take(IntroSheetInks.VariantColours)]));
        }
        return shades;
    }

    /// <summary>Each of <paramref name="colours"/> at cross-fade <paramref name="level"/>.</summary>
    private static RgbColor[] Faded(IEnumerable<RgbColor> colours, int level)
    {
        var faded = new ColourLevel(level, CinematicPaletteFader.Levels);
        return [.. colours.Select(faded.Apply)];
    }
}
