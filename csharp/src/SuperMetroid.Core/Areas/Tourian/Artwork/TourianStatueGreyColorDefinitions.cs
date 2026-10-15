using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>
/// $8D:E240..E2DC Tourian statue grey-out colors: six intermediate RGB5 samples
/// between the first and last supplied rows, rounded to the nearest seventh.
/// Color zero changes immediately to its final value. Endpoints remain supplied
/// inputs; their independent artwork choices are outside this interpolation conversion.
/// </summary>
internal static class TourianStatueGreyColorDefinitions
{
    /// <summary>Maps an aligned interior palette FX operand to its grey-out frame and color index.</summary>
    internal static bool TryCoordinates(ushort pointer, out int frame, out int color)
    {
        frame = color = 0;
        int offset = TourianStatueGreyPaletteFxColorDefinitions.ColorOffset(pointer);
        if ((uint)offset >= TourianStatueGreyPaletteFxColorDefinitions.FrameCount *
            TourianStatueGreyPaletteFxColorDefinitions.FrameByteCount) return false;
        int within = offset % TourianStatueGreyPaletteFxColorDefinitions.FrameByteCount;
        if ((within & 1) != 0 || within >= TourianStatueGreyPaletteFxColorDefinitions.ColorsPerFrame * 2)
            return false;
        frame = offset / TourianStatueGreyPaletteFxColorDefinitions.FrameByteCount;
        color = within / 2;
        return true;
    }

    /// <summary>Explicit supplied edits take precedence; generated colors are never cached.</summary>
    internal static bool TryCalculatedColor(ushort pointer, IReadOnlyDictionary<ushort, Bgr555> colors, out Bgr555 value)
    {
        value = Bgr555.Black;
        if (!TryCoordinates(pointer, out int frame, out int color) || frame is 0 or 7 ||
            !colors.TryGetValue(TourianStatueGreyPaletteFxColorDefinitions.ColorPointer(0, color), out Bgr555 first) ||
            !colors.TryGetValue(TourianStatueGreyPaletteFxColorDefinitions.ColorPointer(7, color), out Bgr555 last))
            return false;
        if (color == 0)
        {
            value = last;
            return true;
        }
        int red = Channel(first.Red, last.Red, frame);
        int green = Channel(first.Green, last.Green, frame);
        int blue = Channel(first.Blue, last.Blue, frame);
        value = new Bgr555(red, green, blue);
        return true;
    }

    /// <summary>Interpolates one RGB5 channel toward the supplied final shade with nearest-seventh rounding.</summary>
    private static int Channel(int first, int last, int frame)
    {
        int delta = last - first;
        return first + Math.Sign(delta) * ((Math.Abs(delta) * frame + 3) / 7);
    }
}
