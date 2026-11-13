using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The installed colour tables Samus's sheets are pre-coloured from (#1512), loaded once for all her sheets.</summary>
/// <param name="Starting">Gameplay's starting colours, which hold her suit's and beams' before any is loaded.</param>
/// <param name="IntroSuit">Her suit's colours in the opening's own colours.</param>
/// <param name="IntroBeams">Her beams' colours in the opening's own colours.</param>
/// <param name="Suits">The three suits' normal colours.</param>
/// <param name="Charge">The charged-beam and pseudo Screw Attack colours.</param>
/// <param name="HyperBeamSuit">The Hyper Beam's full-body glow.</param>
/// <param name="FullBodyCycles">The Speed Booster, Screw Attack and shinespark cycles.</param>
/// <param name="Hurt">The hurt flash's and the cinematic restoration's colours.</param>
/// <param name="Visor">The visor cycle's shades.</param>
/// <param name="BeamPalettes">The retail beam combinations' colours.</param>
/// <param name="CrystalFlash">The Crystal Flash's body and bubble colours.</param>
/// <param name="HyperBeamCycle">The Hyper Beam's own beam colour cycle.</param>
/// <param name="Death">The death sequence's suited, suitless and whiteout colours.</param>
/// <param name="PaletteFx">The palette-FX records' colours, whose heat glow and loading suit recolour her suit (#1512).</param>
internal sealed record SamusShadeSources(GameplayStartingColours Starting, RgbColor[] IntroSuit, RgbColor[] IntroBeams,
    InstalledSamusSuitColors Suits, SamusChargeColours Charge, SamusHyperBeamColours HyperBeamSuit,
    SamusFullBodyCycleColours FullBodyCycles,
    InstalledSamusHurtColors Hurt, InstalledSamusVisorColors Visor, InstalledBeamPalettes BeamPalettes,
    InstalledCrystalFlashColors CrystalFlash, HyperBeamFxColours HyperBeamCycle,
    InstalledSamusDeathPalettes Death, IPaletteFxColorSource PaletteFx)
{
    /// <summary>The visible colours of an ordinary charged shot's glow, every one but the transparent first.</summary>
    private static readonly RgbColor ChargedShotGlowColour = new(255, 255, 0);

    /// <summary>The visor's green as a door transition finishes loading the next room.</summary>
    private static readonly RgbColor DoorCompletionVisorColour = new(0, 255, 115);

    /// <summary>The grapple flare's colour once the grapple's firing colours are loaded.</summary>
    private static readonly RgbColor GrappleFiringFlareColour = new(140, 231, 255);

    /// <summary>
    /// Every shade Samus's sheets are pre-coloured in (#1512): one per colouring her colour state can name, each recolouring
    /// the inks of the colours it changes with the cartridge's colours for it. Her draws name the shades through
    /// <see cref="SamusShades"/>; where each colouring lies among her inks is the import's alone.
    /// </summary>
    internal IReadOnlyList<SheetShade> Shades()
    {
        var shades = new List<SheetShade>
        {
            Suit(SamusShades.StartingSuit, Starting.SamusSuit),
            Beams(SamusShades.StartingBeams, Starting.SamusBeams),
            Suit(SamusShades.IntroSuit, IntroSuit),
            Beams(SamusShades.IntroBeams, IntroBeams),
            new(SamusShades.ChargedShotGlow, [.. Enumerable.Range(SamusSheetInks.Suit + 1, SamusColourSets.ColoursPerSet - 1)],
                Enumerable.Repeat(ChargedShotGlowColour, SamusColourSets.ColoursPerSet - 1).ToArray()),
            new(SamusShades.DoorCompletionVisor, [SamusSheetInks.Suit + SamusSheetInks.VisorColour], [DoorCompletionVisorColour]),
            new(SamusShades.GrappleFiringFlare, [SamusSheetInks.GrappleFlare], [GrappleFiringFlareColour]),
        };
        foreach (SamusSuit suit in Enum.GetValues<SamusSuit>())
            shades.Add(Suit(SamusShades.Suit(suit), Colours(colour => Suits.Resolve(suit, colour))));
        foreach (SamusChargePalette palette in Charge.Palettes)
            shades.Add(Suit(SamusShades.Charge(palette), Colours(colour => Charge.Resolve(palette, colour))));
        for (int frame = 0; frame < SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount; frame++)
        {
            int shown = frame;
            shades.Add(Suit(SamusShades.HyperBeamSuit(shown), Colours(colour => HyperBeamSuit.Resolve(shown, colour))));
        }
        for (int frame = 0; frame < SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount; frame++)
        {
            // The charge colours hold the Hyper shot's own copy of the glow in playback order, the cycle's last frame first.
            int playback = SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount - 1 - frame;
            shades.Add(Suit(SamusShades.HyperShot(frame), Colours(colour => Charge.ResolveHyper(playback, colour))));
        }
        foreach (SamusFullBodyCyclePalette palette in FullBodyCycles.Palettes)
            shades.Add(Suit(SamusShades.FullBodyCycle(palette), Colours(colour => FullBodyCycles.Resolve(palette, colour))));
        foreach (SamusHurtColorVariant variant in Enum.GetValues<SamusHurtColorVariant>())
            shades.Add(Suit(SamusShades.Hurt(variant), Colours(colour => Hurt.Resolve(variant, colour))));
        for (int shade = 0; shade < SamusVisorColorFormat.ColorCount; shade++)
        {
            shades.Add(new(SamusShades.Visor(shade), [SamusSheetInks.Suit + SamusSheetInks.VisorColour],
                [Visor.Resolve(shade)]));
        }
        foreach (SamusBeamCombination combination in SamusBeamColours.Shown)
            shades.Add(Beams(SamusShades.Beam(combination), SamusBeamColours.Of(combination, BeamPalettes)));
        for (int step = 0; step < CrystalFlashColorFormat.BodyFrameCount; step++)
        {
            int shown = step;
            shades.Add(SheetShade.Run(SamusShades.CrystalFlashBody(shown), SamusSheetInks.Beams,
                Enumerable.Range(0, CrystalFlashColorFormat.BodyColorCount).Select(colour => CrystalFlash.ResolveBody(shown, colour)).ToArray()));
        }
        for (int step = 0; step < CrystalFlashColorFormat.BubbleFrameCount; step++)
        {
            int shown = step;
            shades.Add(SheetShade.Run(SamusShades.CrystalFlashBubble(shown), SamusSheetInks.Beams + SamusSheetInks.CrystalFlashBubbleFirstColour,
                Enumerable.Range(0, CrystalFlashColorFormat.BubbleColorCount).Select(colour => CrystalFlash.ResolveBubble(shown, colour)).ToArray()));
        }
        for (int frame = 0; frame < SamusColourSets.HyperBeamCycleFrames; frame++)
        {
            shades.Add(SheetShade.Run(SamusShades.HyperBeamCycle(frame), SamusSheetInks.Beams + SamusSheetInks.HyperBeamFirstColour,
                HyperBeamCycle.Frame(frame).AsSpan()));
        }
        for (int step = 0; step < SamusPaletteDefinitions.Death.PaletteCount; step++)
        {
            int shown = step;
            foreach (SamusSuit suit in Enum.GetValues<SamusSuit>())
            {
                shades.Add(Suit(SamusShades.DeathSuit(suit, shown),
                    Colours(colour => Death.SuitedColor(suit.ListPlace(), shown, colour))));
            }
            shades.Add(SheetShade.Run(SamusShades.DeathSuitless(shown), SamusSheetInks.Suitless,
                Colours(colour => Death.SuitlessColor(shown, colour))));
        }
        for (int step = 0; step < SamusPaletteDefinitions.Death.WhiteoutShadeCount; step++)
        {
            // The whiteout fills her beams' colours and her grapple flare's.
            RgbColor white = Death.WhiteoutColor(step);
            shades.Add(new(SamusShades.DeathWhiteout(step),
                [SamusSheetInks.GrappleFlare, .. Enumerable.Range(SamusSheetInks.Beams, SamusColourSets.ColoursPerSet)],
                Enumerable.Repeat(white, SamusColourSets.ColoursPerSet + 1).ToArray()));
        }
        // Her heat glow and her suit's loading are palette-FX records she lays over her suit.
        shades.AddRange(PaletteFxSamusShades.Of(PaletteFx));
        return shades;
    }

    /// <summary>The shade recolouring her suit's colours with <paramref name="colours"/>.</summary>
    private static SheetShade Suit(PictureShade shade, ReadOnlySpan<RgbColor> colours)
    {
        Ensure.LengthEqual(colours, SamusColourSets.ColoursPerSet);
        return SheetShade.Run(shade, SamusSheetInks.Suit, colours);
    }

    /// <summary>The shade recolouring her beams' colours with <paramref name="colours"/>.</summary>
    private static SheetShade Beams(PictureShade shade, ReadOnlySpan<RgbColor> colours)
    {
        Ensure.LengthEqual(colours, SamusColourSets.ColoursPerSet);
        return SheetShade.Run(shade, SamusSheetInks.Beams, colours);
    }

    /// <summary>One colouring's sixteen colours, each from <paramref name="colour"/>.</summary>
    private static RgbColor[] Colours(Func<int, RgbColor> colour) =>
        [.. Enumerable.Range(0, SamusColourSets.ColoursPerSet).Select(colour)];
}
