using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The shades every one of Samus's sheets is pre-coloured in (#1512): one per colouring her colour state can name, each
/// recolouring the inks of the colours it changes with the cartridge's colours for it. Her draws name the shades through
/// <see cref="SamusShades"/>; the colour tables and where each colouring lies among her inks are the import's alone.
/// </summary>
internal static class SamusSheetShades
{
    /// <summary>The visible colours of an ordinary charged shot's glow, every one but the transparent first.</summary>
    private static readonly RgbColor ChargedShotGlowColour = new(31, 31, 0);

    /// <summary>The visor's green as a door transition finishes loading the next room.</summary>
    private static readonly RgbColor DoorCompletionVisorColour = new(0, 31, 14);

    /// <summary>The grapple flare's colour once the grapple's firing colours are loaded.</summary>
    private static readonly RgbColor GrappleFiringFlareColour = new(17, 28, 31);

    /// <summary>Every shade Samus's sheets are pre-coloured in, from the installed colour tables.</summary>
    internal static IReadOnlyList<SheetShade> Of(SamusShadeSources sources)
    {
        Ensure.NotNull(sources);
        var shades = new List<SheetShade>
        {
            Suit(SamusShades.StartingSuit, sources.Starting.SamusSuit),
            Beams(SamusShades.StartingBeams, sources.Starting.SamusBeams),
            Suit(SamusShades.IntroSuit, sources.IntroSuit),
            Beams(SamusShades.IntroBeams, sources.IntroBeams),
            new(SamusShades.ChargedShotGlow, [.. Enumerable.Range(SamusSheetInks.Suit + 1, SamusColourDefinitions.SuitColours - 1)],
                Enumerable.Repeat(ChargedShotGlowColour, SamusColourDefinitions.SuitColours - 1).ToArray()),
            new(SamusShades.DoorCompletionVisor, [SamusSheetInks.Suit + SamusSheetInks.VisorColour], [DoorCompletionVisorColour]),
            new(SamusShades.GrappleFiringFlare, [SamusSheetInks.GrappleFlare], [GrappleFiringFlareColour]),
        };
        foreach (SamusSuit suit in Enum.GetValues<SamusSuit>())
            shades.Add(Suit(SamusShades.Suit(suit), Colours(colour => sources.Suits.Resolve(suit, colour))));
        foreach (SamusChargePalette palette in sources.Charge.Palettes)
            shades.Add(Suit(SamusShades.Charge(palette), Colours(colour => sources.Charge.Resolve(palette, colour))));
        for (int frame = 0; frame < SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount; frame++)
        {
            int shown = frame;
            shades.Add(Suit(SamusShades.HyperBeamSuit(shown), Colours(colour => sources.HyperBeamSuit.Resolve(shown, colour))));
        }
        for (int frame = 0; frame < SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount; frame++)
        {
            // The charge colours hold the Hyper shot's own copy of the glow in playback order, the cycle's last frame first.
            int playback = SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount - 1 - frame;
            shades.Add(Suit(SamusShades.HyperShot(frame), Colours(colour => sources.Charge.ResolveHyper(playback, colour))));
        }
        foreach (SamusFullBodyCyclePalette palette in sources.FullBodyCycles.Palettes)
            shades.Add(Suit(SamusShades.FullBodyCycle(palette), Colours(colour => sources.FullBodyCycles.Resolve(palette, colour))));
        foreach (SpeedBoosterPaletteOverrun overrun in Enum.GetValues<SpeedBoosterPaletteOverrun>())
            shades.Add(Suit(SamusShades.SpeedBoosterOverrun(overrun), [.. sources.SpeedBoosterOverrun.Colours(overrun)]));
        foreach (SamusHurtColorVariant variant in Enum.GetValues<SamusHurtColorVariant>())
            shades.Add(Suit(SamusShades.Hurt(variant), Colours(colour => sources.Hurt.Resolve(variant, colour))));
        for (int shade = 0; shade < SamusVisorColorFormat.ColorCount; shade++)
        {
            shades.Add(new(SamusShades.Visor(shade), [SamusSheetInks.Suit + SamusSheetInks.VisorColour],
                [sources.Visor.Resolve(shade)]));
        }
        foreach (SamusBeamCombination combination in SamusBeamColours.Shown)
            shades.Add(Beams(SamusShades.Beam(combination), SamusBeamColours.Of(combination, sources.BeamPalettes, sources.Chainsaw)));
        for (int step = 0; step < CrystalFlashColorFormat.BodyFrameCount; step++)
        {
            int shown = step;
            shades.Add(SheetShade.Run(SamusShades.CrystalFlashBody(shown), SamusSheetInks.Beams,
                Enumerable.Range(0, CrystalFlashColorFormat.BodyColorCount).Select(colour => sources.CrystalFlash.ResolveBody(shown, colour)).ToArray()));
        }
        for (int step = 0; step < CrystalFlashColorFormat.BubbleFrameCount; step++)
        {
            int shown = step;
            shades.Add(SheetShade.Run(SamusShades.CrystalFlashBubble(shown), SamusSheetInks.Beams + SamusSheetInks.CrystalFlashBubbleFirstColour,
                Enumerable.Range(0, CrystalFlashColorFormat.BubbleColorCount).Select(colour => sources.CrystalFlash.ResolveBubble(shown, colour)).ToArray()));
        }
        for (int frame = 0; frame < SamusPaletteDefinitions.HyperBeamFx.CycleFrameCount; frame++)
        {
            shades.Add(SheetShade.Run(SamusShades.HyperBeamCycle(frame), SamusSheetInks.Beams + SamusSheetInks.HyperBeamFirstColour,
                sources.HyperBeamCycle.Frame(frame).AsSpan()));
        }
        for (int step = 0; step < SamusPaletteDefinitions.Death.PaletteCount; step++)
        {
            int shown = step;
            foreach (SamusSuit suit in Enum.GetValues<SamusSuit>())
            {
                shades.Add(Suit(SamusShades.DeathSuit(suit, shown),
                    Colours(colour => sources.Death.SuitedColor(suit.ListPlace(), shown, colour))));
            }
            shades.Add(SheetShade.Run(SamusShades.DeathSuitless(shown), SamusSheetInks.Suitless,
                Colours(colour => sources.Death.SuitlessColor(shown, colour))));
        }
        for (int step = 0; step < SamusPaletteDefinitions.Death.WhiteoutShadeCount; step++)
        {
            // The whiteout fills her beams' colours and her grapple flare's.
            RgbColor white = sources.Death.WhiteoutColor(step);
            shades.Add(new(SamusShades.DeathWhiteout(step),
                [SamusSheetInks.GrappleFlare, .. Enumerable.Range(SamusSheetInks.Beams, SamusColourDefinitions.SuitColours)],
                Enumerable.Repeat(white, SamusColourDefinitions.SuitColours + 1).ToArray()));
        }
        // Her heat glow and her suit's loading are palette-FX records she lays over her suit.
        shades.AddRange(PaletteFxSamusShades.Of(sources.PaletteFx));
        return shades;
    }

    /// <summary>The shade recolouring her suit's colours with <paramref name="colours"/>.</summary>
    private static SheetShade Suit(PictureShade shade, ReadOnlySpan<RgbColor> colours)
    {
        Ensure.LengthEqual(colours, SamusColourDefinitions.SuitColours);
        return SheetShade.Run(shade, SamusSheetInks.Suit, colours);
    }

    /// <summary>The shade recolouring her beams' colours with <paramref name="colours"/>.</summary>
    private static SheetShade Beams(PictureShade shade, ReadOnlySpan<RgbColor> colours)
    {
        Ensure.LengthEqual(colours, SamusColourDefinitions.SuitColours);
        return SheetShade.Run(shade, SamusSheetInks.Beams, colours);
    }

    /// <summary>One colouring's sixteen colours, each from <paramref name="colour"/>.</summary>
    private static RgbColor[] Colours(Func<int, RgbColor> colour) =>
        [.. Enumerable.Range(0, SamusColourDefinitions.SuitColours).Select(colour)];
}
