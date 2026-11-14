using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// A palette-FX colour family as its presentation document lays it out: consecutive timed records from
/// <paramref name="FirstRecord"/>, each an animation frame of <paramref name="ColorsPerFrame"/> written colours.
/// </summary>
/// <param name="FirstRecord">The record shown as the family's first frame.</param>
/// <param name="FrameCount">The family's frames.</param>
/// <param name="ColorsPerFrame">The colours each frame writes.</param>
public readonly record struct PaletteFxColorFrames(int FirstRecord, int FrameCount, int ColorsPerFrame)
{
    /// <summary>
    /// The frames <paramref name="animation"/> shows (#1739): the consecutive records of its frames, in the order it shows
    /// them.
    /// </summary>
    /// <param name="animation">The animation whose frames are the family's frames.</param>
    /// <param name="colorsPerFrame">The colours each frame writes.</param>
    /// <exception cref="InvalidOperationException">The animation shows no record, or its records do not lie one after another.</exception>
    public static PaletteFxColorFrames Of(ColourEffectAnimation animation, int colorsPerFrame)
    {
        int count = PaletteFxRecords.FrameCount(animation);
        if (count == 0)
            throw new InvalidOperationException($"Colour effect animation {animation} shows no record.");
        int first = PaletteFxRecord.Of(new ColourEffectFrame(animation, 0)).Record;
        for (int frame = 1; frame < count; frame++)
        {
            if (PaletteFxRecord.Of(new ColourEffectFrame(animation, frame)).Record != first + frame)
                throw new InvalidOperationException($"Colour effect animation {animation} shows {new ColourEffectFrame(animation, frame)} out of its records' order.");
        }
        return new(first, count, colorsPerFrame);
    }

    /// <summary>The record shown as frame <paramref name="frame"/>.</summary>
    public PaletteFxRecord Frame(int frame) => (uint)frame < FrameCount
        ? new(FirstRecord + frame)
        : throw new ArgumentOutOfRangeException(nameof(frame));

    /// <summary>Colour <paramref name="color"/> of frame <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => (uint)color < ColorsPerFrame
        ? new(Frame(frame), color)
        : throw new ArgumentOutOfRangeException(nameof(color));
}
