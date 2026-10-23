namespace SuperMetroid.Core.Rendering;

/// <summary>A run of a screen's colours (#1404): a row, part of one, or several rows, read and written in place.</summary>
public readonly struct ScreenColours
{
    /// <summary>The screen whose colours these are.</summary>
    private readonly ScreenPalette palette;

    /// <summary>Takes <paramref name="count"/> of <paramref name="palette"/>'s colours from <paramref name="first"/>.</summary>
    internal ScreenColours(ScreenPalette palette, int first, int count)
    {
        this.palette = palette;
        First = Ensure.BetweenInclusive(0, ScreenPalette.ColorCount, first);
        Count = Ensure.BetweenInclusive(0, ScreenPalette.ColorCount - first, count);
    }

    /// <summary>The position of the run's first colour among the screen's colours.</summary>
    internal int First { get; }

    /// <summary>The number of colours in the run.</summary>
    public int Count { get; }

    /// <summary>The run's colour <paramref name="colour"/>.</summary>
    public Bgr555 this[int colour] => palette.Colors[First + Check(colour)];

    /// <summary>Changes the run's colour <paramref name="colour"/>.</summary>
    public void Write(int colour, Bgr555 value) => palette.SetColor(First + Check(colour), value);

    /// <summary>Changes every colour of the run to <paramref name="value"/>.</summary>
    public void Fill(Bgr555 value)
    {
        for (int colour = 0; colour < Count; colour++)
            palette.SetColor(First + colour, value);
    }

    /// <summary>Changes the run's colours to <paramref name="source"/>, which holds exactly as many.</summary>
    public void CopyFrom(ReadOnlySpan<Bgr555> source)
    {
        Ensure.LengthEqual(source, Count);
        for (int colour = 0; colour < Count; colour++)
            palette.SetColor(First + colour, source[colour]);
    }

    /// <summary>The <paramref name="count"/> colours of the run from its colour <paramref name="start"/>.</summary>
    public ScreenColours Slice(int start, int count)
    {
        Ensure.BetweenInclusive(0, Count, start);
        Ensure.BetweenInclusive(0, Count - start, count);
        return new ScreenColours(palette, First + start, count);
    }

    /// <summary>The colours at the run's positions in <paramref name="screenColours"/>, a whole screen's colours.</summary>
    public ReadOnlySpan<Bgr555> SamePositionsIn(ReadOnlySpan<Bgr555> screenColours)
    {
        Ensure.LengthEqual(screenColours, ScreenPalette.ColorCount);
        return screenColours.Slice(First, Count);
    }

    /// <summary>Whether the screen's colour at <paramref name="position"/> is in the run.</summary>
    internal bool Covers(int position) => position >= First && position < First + Count;

    /// <summary>Checks that <paramref name="colour"/> is one of the run's.</summary>
    private int Check(int colour) => Ensure.BetweenInclusive(0, Count - 1, colour);
}
