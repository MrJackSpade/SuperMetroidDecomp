namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// A palette-FX colour family as its presentation document lays it out: consecutive timed records from
/// <paramref name="FirstRecord"/>, each a row of <paramref name="ColorsPerFrame"/> written colours.
/// </summary>
/// <param name="FirstRecord">The record shown as the family's first row.</param>
/// <param name="FrameCount">The family's rows.</param>
/// <param name="ColorsPerFrame">The colours each row writes.</param>
public readonly record struct PaletteFxColorRows(int FirstRecord, int FrameCount, int ColorsPerFrame)
{
    /// <summary>The record shown as row <paramref name="frame"/>.</summary>
    public PaletteFxFrame Frame(int frame) => (uint)frame < FrameCount
        ? new(FirstRecord + frame)
        : throw new ArgumentOutOfRangeException(nameof(frame));

    /// <summary>Colour <paramref name="color"/> of row <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => (uint)color < ColorsPerFrame
        ? new(Frame(frame), color)
        : throw new ArgumentOutOfRangeException(nameof(color));

    /// <summary>The row and colour of <paramref name="key"/> when it belongs to this family.</summary>
    public bool TryLocate(PaletteFxColorKey key, out int frame, out int color)
    {
        frame = key.Frame.Record - FirstRecord;
        color = key.Color;
        if ((uint)frame < FrameCount && (uint)color < ColorsPerFrame)
            return true;
        frame = color = 0;
        return false;
    }
}
