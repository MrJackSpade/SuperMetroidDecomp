namespace SuperMetroid.Core;

/// <summary>
/// An alternation paced by a count (#2563): a first half of <see cref="CountsEachWay"/> counts, then a second half of as
/// many, then the first again. A catalog names the alternation each flash, flicker, sway or shake keeps, stated as its
/// rhythm (white for two updates of every four, hidden on alternate updates); the count it is read on is the owner's
/// own update count, timer or tally.
/// </summary>
/// <remarks>
/// A count read here may run up or down and below zero: its half is the count's place in the alternation counted from
/// below, so a count of -1 sits on the second half's last count.
/// </remarks>
public readonly partial record struct UpdateAlternation
{
    /// <summary>An alternation whose halves each last <paramref name="countsEachWay"/> counts.</summary>
    /// <param name="countsEachWay">How many counts each half lasts, one or more.</param>
    /// <exception cref="ArgumentOutOfRangeException">A half would last no count.</exception>
    public UpdateAlternation(int countsEachWay)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(countsEachWay, 1);
        CountsEachWay = countsEachWay;
    }

    /// <summary>How many counts each half of the alternation lasts.</summary>
    public int CountsEachWay { get; }

    /// <summary>Whether the alternation is in its second half when its count reads <paramref name="count"/>.</summary>
    /// <param name="count">The count that paces the alternation.</param>
    public bool IsInSecondHalfAt(int count)
    {
        int length = 2 * CountsEachWay;
        int place = ((count % length) + length) % length;
        return place >= CountsEachWay;
    }
}
