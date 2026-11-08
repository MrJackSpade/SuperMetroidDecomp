using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// Compares Mother Brain's supplied death fades with their calculated paint: a fade that follows the stock interpolation
/// keeps only its endpoints.
/// </summary>
internal static class MotherBrainDeathColorEdits
{
    /// <summary>Selects the body, leg and corpse fades.</summary>
    internal static MotherBrainDeathColorSelection Select(RgbColor[][] bodyFade, RgbColor[][] legFade,
        RgbColor[][] corpseFade) =>
        new(Fade(bodyFade, last: null, MotherBrainPalettePart.Body),
            Fade(legFade, last: null, MotherBrainPalettePart.BackLeg),
            Fade(corpseFade, MotherBrainRainbowPaletteRecognition.Drained(corpseFade[^1]), MotherBrainPalettePart.Body));

    /// <summary>
    /// Keeps a fade as its edited starting colours and endpoint when every stage follows the calculated interpolation,
    /// and keeps every supplied stage when one does not.
    /// </summary>
    private static MotherBrainDeathFade Fade(RgbColor[][] frames, InstalledMotherBrainRainbowPalette.DrainedBodyColors? last,
        MotherBrainPalettePart part)
    {
        int colorCount = frames[0].Length;
        RgbColor[]? first = null;
        for (int color = 0; color < colorCount; color++)
            if (frames[0][color] != MotherBrainHealthPalettePresentation.StockDeathStartColor(part, color))
            { first = frames[0]; break; }
        var calculated = new MotherBrainDeathFade(first, last, supplied: null, frames.Length, colorCount, part);
        for (int frame = 0; frame < frames.Length; frame++)
        for (int color = 0; color < colorCount; color++)
            if (frames[frame][color] != calculated.Resolve(frame, color))
                return new MotherBrainDeathFade(first, last, frames, frames.Length, colorCount, part);
        return calculated;
    }
}
