using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// The shades the import pre-colours the ending's sheets in (#1512), over the screen's colours: each colour load, each fade
/// level of each faded group, each flash, each logo crossfade step and each palette-effect record the ending's objects show.
/// Each ending sheet takes the part of each that falls on its inks: a picture its background colours, a floor all of them,
/// and a sprite sheet its own-palette parts' sprite colours and, under each actor colouring, its colouring's group over the
/// palette-inheriting parts' inks.
/// </summary>
internal static class EndingScreenShades
{
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
            Loaded(EndingColourLoad.ExplosionSprites, explosion),
            Loaded(EndingColourLoad.ExplosionBackgrounds, explosion),
            Loaded(EndingColourLoad.CreditsBackgrounds, credits),
            Loaded(EndingColourLoad.PostCredits, postCredits),
            Loaded(EndingColourLoad.RestoredShot, postCredits),
            new(EndingShades.Loaded(EndingColourLoad.FinalGunship), EndingScreenInks.Load(EndingColourLoad.FinalGunship),
                Run(palettes[EndingPaletteId.FinalGunship], EndingScreenInks.Load(EndingColourLoad.FinalGunship).Length)),
            new(EndingShades.Loaded(EndingColourLoad.LogoInitial), EndingScreenInks.Load(EndingColourLoad.LogoInitial),
                Run(palettes[EndingPaletteId.LogoInitial], EndingScreenInks.Load(EndingColourLoad.LogoInitial).Length)),
        };
        // The explosion's crossfade scales the colours it captured as it began: the escape's far background, which no
        // effect had recoloured, and the explosion's sprite colours it had just loaded. The post-credits fades scale the
        // post-credits colours, which nothing recolours before them.
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
        foreach ((PaletteFxColours group, PaletteFxFrame record) in PaletteFxRecordShades.ShownBy(EndingPaletteFxDefinitions.Objects))
            shades.Add(PaletteFxRecordShades.Shade(group, record, EndingScreenInks.PaletteFx(group), paletteFx));
        return shades;
    }

    /// <summary>The part of each of <paramref name="screen"/> that falls on a picture's background colours, whose inks are the screen's.</summary>
    internal static IReadOnlyList<SheetShade> Pictures(IReadOnlyList<SheetShade> screen) =>
        [.. Ensure.NotNull(screen).Select(shade => Within(shade, 0, EndingScreenInks.SpriteFirst, 0))];

    /// <summary>
    /// A sprite sheet's shades from <paramref name="screen"/> (#1513): each colouring's drawn shade, its own sprite set's
    /// colours over every set its frames are pictured in, as <paramref name="pictures"/> records them; the part of each
    /// screen shade on the sprite colours, except the sets some colouring's shade shows in another set's colours; and under
    /// each colouring, the part of each screen shade on its set, over every set its frames are pictured in.
    /// </summary>
    /// <param name="screen">The ending's screen shades.</param>
    /// <param name="sprites">The sheet's sprite colours.</param>
    /// <param name="pictures">The sets each colouring's frames are pictured in.</param>
    internal static IReadOnlyList<SheetShade> Sprites(IReadOnlyList<SheetShade> screen, IReadOnlyList<RgbColor> sprites,
        ColouringPictures<EndingSpriteColouring> pictures)
    {
        Ensure.NotNull(screen);
        Ensure.NotNull(sprites);
        Ensure.NotNull(pictures);
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
            shades.Add(Outside(Within(shade, EndingScreenInks.SpriteFirst, EndingScreenInks.ScreenColourCount, -EndingScreenInks.SpriteFirst),
                pictures.Remapped));
            foreach (EndingSpriteColouring colouring in colourings)
            {
                int first = EndingScreenInks.SpriteFirst + EndingScreenInks.SpriteSet(colouring) * EndingScreenInks.GroupColours;
                SheetShade[] parts = [.. pictures.SetsOf(colouring).Select(pictured =>
                    Within(shade, first, first + EndingScreenInks.GroupColours, pictured * EndingScreenInks.GroupColours - first))];
                shades.Add(new SheetShade(EndingShades.Under(EndingShades.Drawn(colouring), shade.Shade),
                    [.. parts.SelectMany(part => part.Inks)], [.. parts.SelectMany(part => part.Colours)]));
            }
        }
        return shades;
    }

    /// <summary><paramref name="shade"/> without the inks of the sprite sets <paramref name="sets"/> holds.</summary>
    private static SheetShade Outside(SheetShade shade, SortedSet<int> sets)
    {
        int[] kept = [.. Enumerable.Range(0, shade.Inks.Length).Where(index => !sets.Contains(shade.Inks[index] / EndingScreenInks.GroupColours))];
        return new SheetShade(shade.Shade, [.. kept.Select(index => shade.Inks[index])], [.. kept.Select(index => shade.Colours[index])]);
    }

    /// <summary>The colour each flash shows.</summary>
    private static readonly EndingFlashColour[] Flashes =
    [
        new(EndingFlash.PlanetGlow, new RgbColor(1, 0, 0)),
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
