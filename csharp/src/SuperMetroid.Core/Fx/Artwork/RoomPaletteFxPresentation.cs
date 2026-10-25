using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// Editable colors for room and cinematic palette animations. Native timing, destinations,
/// instructions, and side effects remain compiled engine mechanics.
/// </summary>
public sealed class RoomPaletteFxPresentation : IPaletteFxColorSource
{
    /// <summary>Explicit RGB5 samples by the record color each one is.</summary>
    internal readonly Dictionary<PaletteFxColorKey, RgbColor> colors;

    /// <summary>Canonical and derived heat-palette samples shared across Samus suits.</summary>
    internal readonly HeatPaletteInputView heatInputs;
    /// <summary>Editable and derived Samus loading-palette samples.</summary>
    internal readonly LoadingPaletteInputView loadingInputs;
    /// <summary>Endpoint samples used to reconstruct the ending gunship fade.</summary>
    private readonly EndingGunshipPaletteInputView gunshipInputs;

    /// <summary>Builds calculated-color views over validated, explicitly authored samples.</summary>
    private RoomPaletteFxPresentation(Dictionary<PaletteFxColorKey, RgbColor> colors)
    {
        this.colors = colors;
        heatInputs = new HeatPaletteInputView(colors);
        loadingInputs = new LoadingPaletteInputView(colors);
        gunshipInputs = new EndingGunshipPaletteInputView(colors);
    }

    /// <summary>Keeps every selected palette-effect colour except those a shared row alias or calculation already reproduces.</summary>
    /// <param name="colors">Every selected colour, by the record colour it is.</param>
    internal static RoomPaletteFxPresentation Create(Dictionary<PaletteFxColorKey, RgbColor> colors)
    {
        // Collapse only values equal to the calculated row alias. Differing player
        // edits keep their original per-frame identity, including edits to base rows.
        foreach (var program in PaletteFxHeatProgramMechanicsDefinitions.All)
        foreach (PaletteFxColorKey key in Keys(program.Rows))
        {
            if (HeatPaletteColorDefinitions.TryCanonical(key, out PaletteFxColorKey canonical) &&
                canonical != key && colors[key] == colors[canonical]) colors.Remove(key);
        }
        foreach (var program in PaletteFxHeatProgramMechanicsDefinitions.All)
        foreach (PaletteFxColorKey key in Keys(program.Rows))
        {
            if (colors.TryGetValue(key, out RgbColor supplied) &&
                HeatPaletteColorDefinitions.TryCalculatedColor(key, colors, out RgbColor calculated) && supplied == calculated)
                colors.Remove(key);
        }
        foreach (var program in SamusLoadingSuitPaletteFxColorDefinitions.All)
        foreach (PaletteFxColorKey key in Keys(program.Rows))
        {
            if (LoadingPaletteColorDefinitions.TryCanonical(key, out PaletteFxColorKey canonical) &&
                canonical != key && colors[key] == colors[canonical]) colors.Remove(key);
        }
        foreach (var program in SamusLoadingSuitPaletteFxColorDefinitions.All)
        foreach (PaletteFxColorKey key in Keys(program.Rows))
        {
            if (colors.TryGetValue(key, out RgbColor supplied) &&
                LoadingPaletteColorDefinitions.TryCalculatedColor(key, colors, out RgbColor calculated) && supplied == calculated)
                colors.Remove(key);
        }
        // Only matching samples are discarded. If the player edits a base color,
        // unchanged intermediate samples that no longer fit remain explicit values.
        foreach (PaletteFxColorKey key in Keys(PostCreditsIconGlarePaletteFxColorDefinitions.Rows, lastFrame: PostCreditsIconGlarePaletteFxColorDefinitions.FrameCount - 1))
        {
            if (LogoGlarePaletteColorDefinitions.TryCalculatedColor(key, colors, out RgbColor calculated) &&
                colors[key] == calculated) colors.Remove(key);
        }
        foreach (PaletteFxColorKey key in Keys(ZebesExplosionGunshipPaletteFxColorDefinitions.Rows, lastFrame: ZebesExplosionGunshipPaletteFxColorDefinitions.FrameCount - 1))
        {
            if (EndingGunshipPaletteColorDefinitions.TryCalculatedColor(key, colors, out RgbColor calculated) &&
                colors[key] == calculated) colors.Remove(key);
        }
        // Keep both endpoints and independent sample edits. Only values matching
        // their supplied endpoints can be reconstructed on demand.
        foreach (PaletteFxColorKey key in Keys(TourianStatueGreyPaletteFxColorDefinitions.Rows, firstFrame: 1, lastFrame: TourianStatueGreyPaletteFxColorDefinitions.FrameCount - 1))
        {
            if (TourianStatueGreyColorDefinitions.TryCalculatedColor(key, colors, out RgbColor calculated) &&
                colors[key] == calculated) colors.Remove(key);
        }
        foreach (PaletteFxColorKey key in Keys(CrateriaLightningColorDefinitions.SurfaceProgram.Rows))
        {
            if (CrateriaLightningColorDefinitions.TryCalculatedColor(key, out RgbColor calculated) &&
                colors[key] == calculated) colors.Remove(key);
        }
        foreach (PaletteFxColorKey key in Keys(CrateriaLightningColorDefinitions.DarkProgram.Rows, firstFrame: 1))
        {
            if (CrateriaLightningColorDefinitions.TryCalculatedDarkColor(key, colors, out RgbColor calculated) &&
                colors[key] == calculated) colors.Remove(key);
        }
        foreach (var program in PlanetZebesTextPaletteFxColorDefinitions.All)
        for (int frame = 0; frame < PlanetZebesTextPaletteFxColorDefinitions.FrameCount; frame++)
        for (int index = 0; index < PlanetZebesTextPaletteFxColorDefinitions.ColorsPerFrame; index++)
        {
            PaletteFxColorKey key = program.Color(frame, index);
            if (key != PlanetZebesTextColorDefinitions.Endpoint(index)
                && PlanetZebesTextColorDefinitions.TryCalculate(key, colors, out RgbColor calculated)
                && colors[key] == calculated) colors.Remove(key);
        }
        foreach (var program in MaridiaEnvironmentalPaletteFxColorDefinitions.All)
        foreach (PaletteFxColorKey key in Keys(program.Rows))
        {
            if (MaridiaEnvironmentalColorDefinitions.TrySource(key, out PaletteFxColorKey source)
                && source != key && colors[key] == colors[source]) colors.Remove(key);
        }
        return new RoomPaletteFxPresentation(colors);
    }

    /// <inheritdoc />
    public bool TryReadColor(PaletteFxColorKey key, out RgbColor color) =>
        gunshipInputs.TryGetValue(key, out color) ||
        PlanetZebesTextColorDefinitions.TryCalculate(key, colors, out color) ||
        MaridiaEnvironmentalColorDefinitions.TryReadColor(key, colors, out color) ||
        CrateriaLightningColorDefinitions.TryCalculatedColor(key, out color) ||
        CrateriaLightningColorDefinitions.TryCalculatedDarkColor(key, colors, out color) ||
        TourianStatueGreyColorDefinitions.TryCalculatedColor(key, colors, out color) ||
        LoadingPaletteColorDefinitions.TryReadColor(key, loadingInputs, out color) ||
        LogoGlarePaletteColorDefinitions.TryCalculatedColor(key, colors, out color) ||
        EndingGunshipPaletteColorDefinitions.TryCalculatedColor(key, gunshipInputs, out color) ||
        (HeatPaletteColorDefinitions.TryCanonical(key, out PaletteFxColorKey canonical) &&
         (heatInputs.TryGetValue(canonical, out color) ||
          HeatPaletteColorDefinitions.TryCalculatedColor(canonical, heatInputs, out color)));

    /// <summary>Every color of <paramref name="rows"/> from <paramref name="firstFrame"/> up to <paramref name="lastFrame"/>, by row then color.</summary>
    private static IEnumerable<PaletteFxColorKey> Keys(PaletteFxColorRows rows, int firstFrame = 0, int? lastFrame = null)
    {
        for (int frame = firstFrame; frame < (lastFrame ?? rows.FrameCount); frame++)
        for (int color = 0; color < rows.ColorsPerFrame; color++)
            yield return rows.Color(frame, color);
    }
}
