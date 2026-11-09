using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>
/// Drops the selected palette-effect colours that a shared frame alias or a calculation already reproduces, keeping every
/// differing player edit, before the game's palette-effect presentation is built from the rest.
/// </summary>
internal static class RoomPaletteFxColorRecognition
{
    /// <summary>Keeps every selected palette-effect colour except those a shared frame alias or calculation already reproduces.</summary>
    /// <param name="colors">Every selected colour, by the record colour it is.</param>
    internal static RoomPaletteFxPresentation Create(Dictionary<PaletteFxColorKey, RgbColor> colors)
    {
        // Collapse only values equal to the calculated frame alias. Differing player
        // edits keep their original per-frame identity, including edits to base frames.
        foreach (var program in PaletteFxHeatProgramMechanicsDefinitions.All)
        foreach (PaletteFxColorKey key in Keys(program.ColorFrames))
        {
            if (HeatPaletteColors.TryCanonical(key, out PaletteFxColorKey canonical) &&
                canonical != key && colors[key] == colors[canonical]) colors.Remove(key);
        }
        foreach (var program in PaletteFxHeatProgramMechanicsDefinitions.All)
        foreach (PaletteFxColorKey key in Keys(program.ColorFrames))
        {
            if (colors.TryGetValue(key, out RgbColor supplied) &&
                HeatPaletteColors.TryCalculatedColor(key, colors, out RgbColor calculated) && supplied == calculated)
                colors.Remove(key);
        }
        foreach (var program in SamusLoadingSuitPaletteFxColorDefinitions.All)
        foreach (PaletteFxColorKey key in Keys(program.ColorFrames))
        {
            if (LoadingPaletteColors.TryCanonical(key, out PaletteFxColorKey canonical) &&
                canonical != key && colors[key] == colors[canonical]) colors.Remove(key);
        }
        foreach (var program in SamusLoadingSuitPaletteFxColorDefinitions.All)
        foreach (PaletteFxColorKey key in Keys(program.ColorFrames))
        {
            if (colors.TryGetValue(key, out RgbColor supplied) &&
                LoadingPaletteColors.TryCalculatedColor(key, colors, out RgbColor calculated) && supplied == calculated)
                colors.Remove(key);
        }
        // Only matching samples are discarded. If the player edits a base color,
        // unchanged intermediate samples that no longer fit remain explicit values.
        foreach (PaletteFxColorKey key in Keys(PostCreditsIconGlarePaletteFxColorDefinitions.ColorFrames, lastFrame: PostCreditsIconGlarePaletteFxColorDefinitions.ColorFrames.FrameCount - 1))
        {
            if (LogoGlarePaletteColors.TryCalculatedColor(key, colors, out RgbColor calculated) &&
                colors[key] == calculated) colors.Remove(key);
        }
        foreach (PaletteFxColorKey key in Keys(ZebesExplosionGunshipPaletteFxColorDefinitions.ColorFrames, lastFrame: ZebesExplosionGunshipPaletteFxColorDefinitions.ColorFrames.FrameCount - 1))
        {
            if (EndingGunshipPaletteColors.TryCalculatedColor(key, colors, out RgbColor calculated) &&
                colors[key] == calculated) colors.Remove(key);
        }
        // Keep both endpoints and independent sample edits. Only values matching
        // their supplied endpoints can be reconstructed on demand.
        foreach (PaletteFxColorKey key in Keys(TourianStatueGreyPaletteFxColorDefinitions.ColorFrames, firstFrame: 1, lastFrame: TourianStatueGreyPaletteFxColorDefinitions.ColorFrames.FrameCount - 1))
        {
            if (TourianStatueGreyPaint.TryCalculatedColor(key, colors, out RgbColor calculated) &&
                colors[key] == calculated) colors.Remove(key);
        }
        foreach (PaletteFxColorKey key in Keys(CrateriaLightningColorDefinitions.SurfaceProgram.ColorFrames))
        {
            if (CrateriaLightningColors.TryCalculatedColor(key, out RgbColor calculated) &&
                colors[key] == calculated) colors.Remove(key);
        }
        foreach (PaletteFxColorKey key in Keys(CrateriaLightningColorDefinitions.DarkProgram.ColorFrames, firstFrame: 1))
        {
            if (CrateriaLightningColors.TryCalculatedDarkColor(key, colors, out RgbColor calculated) &&
                colors[key] == calculated) colors.Remove(key);
        }
        foreach (var program in PlanetZebesTextPaletteFxColorDefinitions.All)
        for (int frame = 0; frame < PlanetZebesTextPaletteFxColorDefinitions.FrameCount; frame++)
        for (int index = 0; index < PlanetZebesTextPaletteFxColorDefinitions.ColorsPerFrame; index++)
        {
            PaletteFxColorKey key = program.Color(frame, index);
            if (key != PlanetZebesTextColors.Endpoint(index)
                && PlanetZebesTextColors.TryCalculate(key, colors, out RgbColor calculated)
                && colors[key] == calculated) colors.Remove(key);
        }
        foreach (var program in MaridiaEnvironmentalPaletteFxColorDefinitions.All)
        foreach (PaletteFxColorKey key in Keys(program.ColorFrames))
        {
            if (MaridiaEnvironmentalColors.TrySource(key, out PaletteFxColorKey source)
                && source != key && colors[key] == colors[source]) colors.Remove(key);
        }
        return new RoomPaletteFxPresentation(colors);
    }

    /// <summary>Every color of <paramref name="colorFrames"/> from <paramref name="firstFrame"/> up to <paramref name="lastFrame"/>, by framethen color.</summary>
    private static IEnumerable<PaletteFxColorKey> Keys(PaletteFxColorFrames colorFrames, int firstFrame = 0, int? lastFrame = null)
    {
        for (int frame = firstFrame; frame < (lastFrame ?? colorFrames.FrameCount); frame++)
        for (int color = 0; color < colorFrames.ColorsPerFrame; color++)
            yield return colorFrames.Color(frame, color);
    }
}
