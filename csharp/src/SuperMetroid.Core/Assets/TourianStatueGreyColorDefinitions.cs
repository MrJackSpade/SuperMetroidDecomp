using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// $8D:E240..E2DC Tourian statue grey-out colors: six intermediate RGB5 samples
/// between the first and last supplied rows, rounded to the nearest seventh.
/// Color zero changes immediately to its final value. Endpoints remain supplied
/// inputs; their independent artwork choices are outside this interpolation conversion.
/// </summary>
internal static class TourianStatueGreyColorDefinitions
{
    internal static bool TryCoordinates(ushort pointer, out int frame, out int color)
    {
        frame = color = 0;
        int offset = pointer - (TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FirstFramePointer + 2);
        if ((uint)offset >= TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameCount *
            TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameByteCount) return false;
        int within = offset % TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameByteCount;
        if ((within & 1) != 0 || within >= TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame * 2)
            return false;
        frame = offset / TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameByteCount;
        color = within / 2;
        return true;
    }

    /// <summary>Explicit supplied edits take precedence; generated colors are never cached.</summary>
    internal static bool TryCalculatedColor(ushort pointer, IReadOnlyDictionary<ushort, ushort> colors, out ushort value)
    {
        value = 0;
        if (!TryCoordinates(pointer, out int frame, out int color) || frame is 0 or 7 ||
            !colors.TryGetValue(TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorPointer(0, color), out ushort first) ||
            !colors.TryGetValue(TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorPointer(7, color), out ushort last))
            return false;
        if (color == 0)
        {
            value = last;
            return true;
        }
        int red = Channel(first & 31, last & 31, frame);
        int green = Channel((first >> 5) & 31, (last >> 5) & 31, frame);
        int blue = Channel((first >> 10) & 31, (last >> 10) & 31, frame);
        value = (ushort)(red | green << 5 | blue << 10);
        return true;
    }

    private static int Channel(int first, int last, int frame)
    {
        int delta = last - first;
        return first + Math.Sign(delta) * ((Math.Abs(delta) * frame + 3) / 7);
    }
}
