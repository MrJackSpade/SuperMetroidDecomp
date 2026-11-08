using SuperMetroid.Core.Fx.PaletteFx;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// Tourian statue grey-out colors: six intermediate colour samples
/// between the first and last supplied frames, rounded to the nearest seventh.
/// Color zero changes immediately to its final value. Endpoints remain supplied
/// inputs; their independent artwork choices are outside this interpolation conversion.
/// </summary>
internal static class TourianStatueGreyPaint
{
    /// <summary>Explicit supplied edits take precedence; generated colors are never cached.</summary>
    internal static bool TryCalculatedColor(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
    {
        value = RgbColor.Black;
        if (!TourianStatueGreyPaletteFxColorDefinitions.ColorFrames.TryLocate(key, out int frame, out int color) || frame is 0 or 7 ||
            !colors.TryGetValue(TourianStatueGreyPaletteFxColorDefinitions.Color(0, color), out RgbColor first) ||
            !colors.TryGetValue(TourianStatueGreyPaletteFxColorDefinitions.Color(7, color), out RgbColor last))
            return false;
        if (color == 0)
        {
            value = last;
            return true;
        }
        int red = Channel(first.FadeLevels.Red, last.FadeLevels.Red, frame);
        int green = Channel(first.FadeLevels.Green, last.FadeLevels.Green, frame);
        int blue = Channel(first.FadeLevels.Blue, last.FadeLevels.Blue, frame);
        value = new FadeLevelColour(red, green, blue).ToColour();
        return true;
    }

    /// <summary>Interpolates one level toward the supplied final shade with nearest-seventh rounding.</summary>
    private static int Channel(int first, int last, int frame)
    {
        int delta = last - first;
        return first + Math.Sign(delta) * ((Math.Abs(delta) * frame + 3) / 7);
    }
}
