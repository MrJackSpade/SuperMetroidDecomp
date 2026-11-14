using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// The shades the import pre-colours the ending's sheets in (#1512), over the screen's colours: each colour load, each fade
/// level of each faded group, each flash, each logo crossfade step and each palette-effect record the ending's objects show.
/// Each ending sheet takes the part of each that falls on its inks, only for the effects authored for what it holds
/// (#1544): a picture sheet its background colours and a floor all of them, for the effects authored for its scenery, and a
/// sprite sheet, under each actor colouring and for the effects authored for its actors, its colouring's group over the
/// palette-inheriting parts' inks and its own-palette parts' groups over their own inks.
/// </summary>
internal static class EndingScreenShades
{
    /// <summary>Every palette object the ending spawns, whose records the ending's sheets are pre-coloured in (#1512).</summary>
    private static readonly ColourEffectId[] PaletteFxObjects =
    [
        EndingColourEffectDefinitions.FadePlanet, EndingColourEffectDefinitions.Supernova, EndingColourEffectDefinitions.SupernovaFinale,
        EndingColourEffectDefinitions.Explosion, EndingColourEffectDefinitions.Lava, EndingColourEffectDefinitions.Crust,
        EndingColourEffectDefinitions.GreyClouds, EndingColourEffectDefinitions.WideExplosion, EndingColourEffectDefinitions.PlanetAfterglow,
        EndingColourEffectDefinitions.GunshipEmergence, EndingLogoDefinitions.LandingColourEffect,
    ];

    /// <summary>Every shade of the ending's screen colours, coloured from <paramref name="palettes"/> and <paramref name="paletteFx"/>.</summary>
    /// <param name="palettes">The ending's installed colours.</param>
    /// <param name="paletteFx">The palette effects' installed colours.</param>
    /// <exception cref="InvalidDataException">A palette-effect record has no installed colour or writes past its group.</exception>
    internal static IReadOnlyList<SheetShade> Screen(EndingPaletteSet palettes, IPaletteFxColorSource paletteFx)
    {
        Ensure.NotNull(palettes);
        Ensure.NotNull(paletteFx);
        RgbColor[] escape = Whole(palettes[EndingPaletteId.Escape]);
        RgbColor[] explosion = Whole(palettes[EndingPaletteId.Explosion]);
        RgbColor[] credits = Whole(palettes[EndingPaletteId.Credits]);
        RgbColor[] postCredits = Whole(palettes[EndingPaletteId.PostCredits]);
        var shades = new List<SheetShade>
        {
            Loaded(EndingColourLoad.Escape, escape),
            Loaded(EndingColourLoad.Explosion, explosion),
            Loaded(EndingColourLoad.ExplosionBackgrounds, explosion),
            Loaded(EndingColourLoad.Credits, credits),
            Loaded(EndingColourLoad.PostCredits, postCredits),
            Loaded(EndingColourLoad.RestoredShot, postCredits),
            new(EndingShades.Loaded(EndingColourLoad.FinalGunship), EndingScreenInks.Load(EndingColourLoad.FinalGunship),
                Run(palettes[EndingPaletteId.FinalGunship], EndingScreenInks.Load(EndingColourLoad.FinalGunship).Length)),
            new(EndingShades.Loaded(EndingColourLoad.LogoInitial), EndingScreenInks.Load(EndingColourLoad.LogoInitial),
                Run(palettes[EndingPaletteId.LogoInitial], EndingScreenInks.Load(EndingColourLoad.LogoInitial).Length)),
        };
        // The explosion's crossfade fades the escape's far background out and the explosion's own lava and planet in. The
        // post-credits fades scale the post-credits colours.
        AddFade(shades, EndingFade.ExplosionBackground, escape);
        AddFade(shades, EndingFade.ExplosionLava, explosion);
        AddFade(shades, EndingFade.ExplodingPlanet, explosion);
        foreach (EndingFade postCreditsFade in (EndingFade[])[EndingFade.WaitingBackdrop, EndingFade.SuitlessSamus, EndingFade.SuitedSamus,
            EndingFade.Shot, EndingFade.ShootingSamus])
            AddFade(shades, postCreditsFade, postCredits);
        foreach (EndingFlashColour flashed in Flashes)
        {
            int[] inks = EndingScreenInks.Flash(flashed.Flash);
            shades.Add(new SheetShade(EndingShades.Flashed(flashed.Flash), inks, [.. inks.Select(_ => flashed.Colour)]));
        }
        EndingPalette crossfade = palettes[EndingPaletteId.LogoCrossfade];
        for (int step = 0; step < EndingShades.LogoCrossfadeSteps; step++)
        {
            int stepShown = step;
            RgbColor[] colours =
            [
                .. Enumerable.Range(0, EndingScreenInks.GroupColours).Select(colour => crossfade.LogoCrossfadeColour(stepShown, EndingLogoLayer.Background, colour)),
                .. Enumerable.Range(0, EndingScreenInks.GroupColours).Select(colour => crossfade.LogoCrossfadeColour(stepShown, EndingLogoLayer.Sprites, colour)),
            ];
            shades.Add(new SheetShade(EndingShades.LogoCrossfade(step), EndingScreenInks.LogoCrossfade, colours));
        }
        // Every record any ending object can show, over the colours its group writes.
        foreach ((ColourEffectColours group, ColourEffectFrame record) in PaletteFxRecordShades.ShownBy(PaletteFxObjects))
            shades.Add(PaletteFxRecordShades.Shade(group, record, EndingScreenInks.PaletteFx(group), paletteFx));
        return shades;
    }

    /// <summary>
    /// The shades of <paramref name="screen"/> authored for any of <paramref name="scenery"/> (#1544), the background
    /// pictures or floor one sheet holds: a floor's inks are the screen's colours, so each covers the floor's colours it
    /// writes.
    /// </summary>
    /// <exception cref="InvalidDataException">A screen shade is of no colour effect the ending shows.</exception>
    internal static IReadOnlyList<SheetShade> Scenery(IReadOnlyList<SheetShade> screen, IReadOnlyCollection<EndingScenery> scenery)
    {
        Ensure.NotNull(scenery);
        return [.. Ensure.NotNull(screen).Where(shade => scenery.Any(shown => EndingColourOwners.Shows(shown, shade.Shade)))];
    }

    /// <summary>
    /// The part of each of <paramref name="screen"/> authored for any of <paramref name="scenery"/> (#1544), the pictures one
    /// sheet holds, that falls on a picture's background colours, whose inks are the screen's.
    /// </summary>
    /// <exception cref="InvalidDataException">A screen shade is of no colour effect the ending shows.</exception>
    internal static IReadOnlyList<SheetShade> Pictures(IReadOnlyList<SheetShade> screen, IReadOnlyCollection<EndingScenery> scenery) =>
        [.. Scenery(screen, scenery).Select(shade => Within(shade, 0, EndingScreenInks.SpriteFirst, 0))];

    /// <summary>
    /// A sprite sheet's shades from <paramref name="screen"/> (#1513, #1544): each colouring's drawn shade, its own sprite
    /// set's colours over every set its frames are pictured in, as <paramref name="pictures"/> records them; and under each
    /// colouring, each screen shade authored for its actors (<see cref="EndingColourOwners"/>): the part on the colouring's
    /// set over every set its frames are pictured in, and the part on each set its frames' own-palette parts keep, as
    /// <paramref name="ownSets"/> gives them. No actor's pictures are pre-coloured in a shade authored for another.
    /// </summary>
    /// <param name="screen">The ending's screen shades.</param>
    /// <param name="sprites">The sheet's sprite colours.</param>
    /// <param name="pictures">The sets each colouring's frames are pictured in.</param>
    /// <param name="ownSets">The sets the own-palette parts of each colouring's frames keep.</param>
    /// <exception cref="InvalidDataException">
    /// A screen shade is of no colour effect the ending shows, or two parts of one shade give one ink different colours.
    /// </exception>
    internal static IReadOnlyList<SheetShade> Sprites(IReadOnlyList<SheetShade> screen, IReadOnlyList<RgbColor> sprites,
        ColouringPictures<EndingSpriteColouring> pictures, Func<EndingSpriteColouring, SortedSet<int>> ownSets)
    {
        Ensure.NotNull(screen);
        Ensure.NotNull(sprites);
        Ensure.NotNull(pictures);
        Ensure.NotNull(ownSets);
        var shades = new List<SheetShade>();
        EndingSpriteColouring[] colourings = Enum.GetValues<EndingSpriteColouring>();
        foreach (EndingSpriteColouring colouring in colourings)
        {
            int set = EndingScreenInks.SpriteSet(colouring);
            shades.Add(SheetShade.DrawnOver(EndingShades.Drawn(colouring), set, pictures.SetsOf(colouring),
                [.. sprites.Skip(set * EndingScreenInks.GroupColours).Take(EndingScreenInks.GroupColours)]));
        }
        foreach (SheetShade shade in screen)
        {
            foreach (EndingSpriteColouring colouring in colourings)
            {
                if (!EndingColourOwners.Shows(colouring, shade.Shade))
                    continue;
                int first = EndingScreenInks.SpriteFirst + EndingScreenInks.SpriteSet(colouring) * EndingScreenInks.GroupColours;
                IEnumerable<SheetShade> inherited = pictures.SetsOf(colouring).Select(pictured =>
                    Within(shade, first, first + EndingScreenInks.GroupColours, pictured * EndingScreenInks.GroupColours - first));
                IEnumerable<SheetShade> own = ownSets(colouring).Select(kept =>
                {
                    int keptFirst = EndingScreenInks.SpriteFirst + kept * EndingScreenInks.GroupColours;
                    return Within(shade, keptFirst, keptFirst + EndingScreenInks.GroupColours, -EndingScreenInks.SpriteFirst);
                });
                shades.Add(Merged(EndingShades.Under(EndingShades.Drawn(colouring), shade.Shade), [.. inherited, .. own]));
            }
        }
        return shades;
    }

    /// <summary>The shade <paramref name="shade"/> recolouring every ink of <paramref name="parts"/>, each once, in rising order.</summary>
    /// <exception cref="InvalidDataException">Two parts give one ink different colours.</exception>
    private static SheetShade Merged(PictureShade shade, SheetShade[] parts)
    {
        var colours = new SortedDictionary<int, RgbColor>();
        foreach (SheetShade part in parts)
        {
            for (int index = 0; index < part.Inks.Length; index++)
            {
                if (colours.TryGetValue(part.Inks[index], out RgbColor earlier) && earlier != part.Colours[index])
                    throw new InvalidDataException($"Shade {shade.Key} gives ink {part.Inks[index]} two colours.");
                colours[part.Inks[index]] = part.Colours[index];
            }
        }
        return new SheetShade(shade, [.. colours.Keys], [.. colours.Values]);
    }

    /// <summary>The colour each flash shows.</summary>
    private static readonly EndingFlashColour[] Flashes =
    [
        new(EndingFlash.PlanetGlow, new RgbColor(8, 0, 0)),
        new(EndingFlash.SilhouetteBackdrop, EndingScreenDefinitions.WhiteColor),
        new(EndingFlash.ExplosionWhiteout, EndingScreenDefinitions.WhiteColor),
        new(EndingFlash.WhiteoutCleared, RgbColor.Black),
        new(EndingFlash.LogoBackground, RgbColor.Black),
    ];

    /// <summary>The part of <paramref name="shade"/> on screen colours from <paramref name="first"/> to before <paramref name="end"/>, each moved by <paramref name="shift"/>.</summary>
    private static SheetShade Within(SheetShade shade, int first, int end, int shift)
    {
        int[] kept = [.. Enumerable.Range(0, shade.Inks.Length).Where(index => shade.Inks[index] >= first && shade.Inks[index] < end)];
        return new SheetShade(shade.Shade, [.. kept.Select(index => shade.Inks[index] + shift)], [.. kept.Select(index => shade.Colours[index])]);
    }

    /// <summary>The shade of <paramref name="load"/>, its colours those of the whole screen <paramref name="colours"/> it writes.</summary>
    private static SheetShade Loaded(EndingColourLoad load, RgbColor[] colours)
    {
        int[] inks = EndingScreenInks.Load(load);
        return new SheetShade(EndingShades.Loaded(load), inks, [.. inks.Select(ink => colours[ink])]);
    }

    /// <summary>Adds the shades of <paramref name="colours"/> at every level, scaling the whole screen <paramref name="source"/>'s colours.</summary>
    private static void AddFade(List<SheetShade> shades, EndingFade colours, RgbColor[] source)
    {
        int[] inks = EndingScreenInks.Fade(colours);
        for (int level = 0; level <= EndingShades.FadeLevels; level++)
        {
            var faded = new ColourLevel(level, EndingShades.FadeLevels);
            shades.Add(new SheetShade(EndingShades.Faded(colours, level), inks, [.. inks.Select(ink => faded.Apply(source[ink]))]));
        }
    }

    /// <summary>The screen colours of a whole screen's resource.</summary>
    private static RgbColor[] Whole(EndingPalette palette) => palette.IsScreen
        ? [.. palette.Colours]
        : throw new InvalidDataException("An ending screen palette is not a whole screen's colours.");

    /// <summary>The first <paramref name="count"/> colours of a run.</summary>
    private static RgbColor[] Run(EndingPalette palette, int count) => palette.Colours.Length >= count
        ? [.. palette.Colours.Take(count)]
        : throw new InvalidDataException($"An ending colour run holds fewer than {count} colours.");
}
