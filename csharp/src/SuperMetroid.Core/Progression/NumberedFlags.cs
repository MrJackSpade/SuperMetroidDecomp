namespace SuperMetroid.Core.Progression;

/// <summary>
/// A fixed range of numbered progress flags, each on or off (#1475): the story events by event number, the Chozo orbs,
/// item locations and doors by the number room data gives them, an area's save stations by station number and an area's
/// explored map cells by cell. Numbers run from zero to <see cref="Capacity"/> minus one; a number outside that range is
/// rejected rather than reaching a neighbouring flag.
/// </summary>
public sealed class NumberedFlags
{
    /// <summary>Whether each numbered flag is on.</summary>
    private readonly bool[] _on;

    /// <summary>Creates the flags numbered zero through <paramref name="capacity"/> minus one, all off.</summary>
    /// <param name="capacity">How many flags the range numbers.</param>
    public NumberedFlags(int capacity) => _on = new bool[Ensure.AtLeastZero(capacity)];

    /// <summary>How many flags the range numbers.</summary>
    public int Capacity => _on.Length;

    /// <summary>Whether flag <paramref name="number"/> is on.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is outside the range.</exception>
    public bool Contains(int number) => _on[Require(number)];

    /// <summary>Turns flag <paramref name="number"/> on.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is outside the range.</exception>
    public void Add(int number) => _on[Require(number)] = true;

    /// <summary>Turns flag <paramref name="number"/> off.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is outside the range.</exception>
    public void Remove(int number) => _on[Require(number)] = false;

    /// <summary>Turns flag <paramref name="number"/> on or off.</summary>
    /// <param name="number">The flag's number.</param>
    /// <param name="on">Whether the flag is on afterwards.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is outside the range.</exception>
    public void Set(int number, bool on) => _on[Require(number)] = on;

    /// <summary>The numbers of the flags that are on, ascending.</summary>
    public IEnumerable<int> Members
    {
        get
        {
            for (int number = 0; number < _on.Length; number++)
            {
                if (_on[number])
                    yield return number;
            }
        }
    }

    /// <summary>Makes every flag match <paramref name="source"/>'s.</summary>
    /// <exception cref="ArgumentException"><paramref name="source"/> numbers a different range.</exception>
    public void CopyFrom(NumberedFlags source)
    {
        Ensure.NotNull(source);
        RequireSameRange(source);
        source._on.CopyTo(_on, 0);
    }

    /// <summary>A detached copy of these flags.</summary>
    public NumberedFlags Copy()
    {
        var copy = new NumberedFlags(Capacity);
        copy.CopyFrom(this);
        return copy;
    }

    /// <summary>
    /// Applies <paramref name="overwrite"/>: every flag it decides is turned on when it lists it and off otherwise, and
    /// every flag past those it decides keeps its state.
    /// </summary>
    /// <exception cref="ArgumentException">The overwrite decides more flags than the range numbers, or lists one it does not decide.</exception>
    public void Overwrite(FlagOverwrite overwrite)
    {
        Ensure.NotNull(overwrite);
        if (overwrite.Decided > Capacity)
            throw new ArgumentException($"The overwrite decides {overwrite.Decided} flags, not at most {Capacity}.", nameof(overwrite));
        for (int number = 0; number < overwrite.Decided; number++)
            _on[number] = false;
        foreach (int number in overwrite.Set)
        {
            if ((uint)number >= (uint)overwrite.Decided)
                throw new ArgumentException($"The overwrite turns on flag {number}, which it does not decide.", nameof(overwrite));
            _on[number] = true;
        }
    }

    /// <summary>Rejects a number outside the range.</summary>
    private int Require(int number) => (uint)number < (uint)_on.Length
        ? number
        : throw new ArgumentOutOfRangeException(nameof(number), number, $"Flag numbers run from 0 through {_on.Length - 1}.");

    /// <summary>Rejects flags that number a different range.</summary>
    private void RequireSameRange(NumberedFlags other)
    {
        if (other.Capacity != Capacity)
            throw new ArgumentException($"Flags numbering {other.Capacity} cannot stand for flags numbering {Capacity}.", nameof(other));
    }
}
