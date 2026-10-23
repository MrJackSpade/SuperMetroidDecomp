namespace SuperMetroid.Core.Assets;

/// <summary>
/// A run of some owner's colours (#1404, #1405): a row, part of one, or several rows of a screen's colours or of an
/// actor's, read and written in place.
/// </summary>
public readonly struct ColourRun
{
    /// <summary>The owner's colours, which the run reads and writes.</summary>
    private readonly Bgr555[] colours;

    /// <summary>Takes <paramref name="count"/> of <paramref name="colours"/> from <paramref name="first"/>.</summary>
    internal ColourRun(Bgr555[] colours, int first, int count)
    {
        this.colours = Ensure.NotNull(colours);
        First = Ensure.BetweenInclusive(0, colours.Length, first);
        Count = Ensure.BetweenInclusive(0, colours.Length - first, count);
    }

    /// <summary>The position of the run's first colour among its owner's colours.</summary>
    internal int First { get; }

    /// <summary>The number of colours in the run.</summary>
    public int Count { get; }

    /// <summary>The run's colours.</summary>
    public ReadOnlySpan<Bgr555> Colors => colours.AsSpan(First, Count);

    /// <summary>The run's colour <paramref name="colour"/>.</summary>
    public Bgr555 this[int colour] => colours[First + Check(colour)];

    /// <summary>Changes the run's colour <paramref name="colour"/>.</summary>
    public void Write(int colour, Bgr555 value) => colours[First + Check(colour)] = value;

    /// <summary>Changes every colour of the run to <paramref name="value"/>.</summary>
    public void Fill(Bgr555 value) => colours.AsSpan(First, Count).Fill(value);

    /// <summary>Changes the run's colours to <paramref name="source"/>, which holds exactly as many.</summary>
    public void CopyFrom(ReadOnlySpan<Bgr555> source)
    {
        Ensure.LengthEqual(source, Count);
        source.CopyTo(colours.AsSpan(First, Count));
    }

    /// <summary>The <paramref name="count"/> colours of the run from its colour <paramref name="start"/>.</summary>
    public ColourRun Slice(int start, int count)
    {
        Ensure.BetweenInclusive(0, Count, start);
        Ensure.BetweenInclusive(0, Count - start, count);
        return new ColourRun(colours, First + start, count);
    }

    /// <summary>The colours at the run's positions in <paramref name="ownerColours"/>, colours laid out as its owner's.</summary>
    public ReadOnlySpan<Bgr555> SamePositionsIn(ReadOnlySpan<Bgr555> ownerColours)
    {
        Ensure.LengthEqual(ownerColours, colours.Length);
        return ownerColours.Slice(First, Count);
    }

    /// <summary>Whether <paramref name="other"/>'s colours are all among this run's, of the same owner.</summary>
    internal bool Contains(ColourRun other) =>
        ReferenceEquals(colours, other.colours) && other.First >= First && other.First + other.Count <= First + Count;

    /// <summary>Whether the owner's colour at <paramref name="position"/> is in the run.</summary>
    internal bool Covers(int position) => position >= First && position < First + Count;

    /// <summary>Checks that <paramref name="colour"/> is one of the run's.</summary>
    private int Check(int colour) => Ensure.BetweenInclusive(0, Count - 1, colour);
}
