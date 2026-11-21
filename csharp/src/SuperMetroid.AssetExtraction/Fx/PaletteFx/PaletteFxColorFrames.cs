using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// A palette-FX colour family as its presentation document lays it out: consecutive timed records from
/// <paramref name="FirstRecord"/>, each an animation frame of <paramref name="ColorsPerFrame"/> written colours.
/// </summary>
/// <param name="FirstRecord">The record shown as the family's first frame.</param>
/// <param name="FrameCount">The family's frames.</param>
/// <param name="ColorsPerFrame">The colours each frame writes.</param>
public readonly partial record struct PaletteFxColorFrames(int FirstRecord, int FrameCount, int ColorsPerFrame)
{
    /// <summary>
    /// The frames <paramref name="effect"/> shows (#1739, #1982): the consecutive records of its frames, in the order it shows
    /// them.
    /// </summary>
    /// <param name="effect">The effect whose frames are the family's frames.</param>
    /// <param name="colorsPerFrame">The colours each frame writes.</param>
    /// <exception cref="InvalidOperationException">The effect shows no record, or its records do not lie one after another.</exception>
    public static PaletteFxColorFrames Of(ColourEffectId effect, int colorsPerFrame) =>
        Run(effect, 0, PaletteFxRecords.FrameCount(effect), colorsPerFrame);

    /// <summary>
    /// The <paramref name="count"/> frames <paramref name="effect"/> shows from its frame <paramref name="first"/>: the
    /// consecutive records of those frames, in the order it shows them (one suit's heat pulse among the heat shimmer's).
    /// </summary>
    /// <param name="effect">The effect whose frames are the family's frames.</param>
    /// <param name="first">The effect's frame that is the family's first.</param>
    /// <param name="count">The family's frames.</param>
    /// <param name="colorsPerFrame">The colours each frame writes.</param>
    /// <exception cref="InvalidOperationException">The run shows no record, or its records do not lie one after another.</exception>
    public static PaletteFxColorFrames Run(ColourEffectId effect, int first, int count, int colorsPerFrame)
    {
        if (count == 0)
            throw new InvalidOperationException($"Colour effect {effect} shows no record from frame {first}.");
        int firstRecord = PaletteFxRecord.Of(new ColourEffectFrame(effect, first)).Record;
        for (int frame = 1; frame < count; frame++)
        {
            if (PaletteFxRecord.Of(new ColourEffectFrame(effect, first + frame)).Record != firstRecord + frame)
                throw new InvalidOperationException($"Colour effect {effect} shows {new ColourEffectFrame(effect, first + frame)} out of its records' order.");
        }
        return new(firstRecord, count, colorsPerFrame);
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
