namespace SuperMetroid.Core;

/// <summary>
/// A place in an authored sequence of values the game takes in turn (#2367): the value the place stands at, and the next
/// place, which returns to the first value after the last. A rumble's rhythm of delays is one.
/// </summary>
/// <typeparam name="T">The values taken in turn.</typeparam>
internal readonly record struct CyclingSequence<T>
{
    /// <summary>The values, in the order they are taken.</summary>
    private readonly IReadOnlyList<T> values;

    /// <summary>Stands at <paramref name="position"/> of <paramref name="values"/>, as <see cref="StartOf"/> and <see cref="At"/> place it.</summary>
    private CyclingSequence(IReadOnlyList<T> values, int position)
    {
        this.values = values;
        Position = position;
    }

    /// <summary>The place of the value taken next, counted from the first.</summary>
    internal int Position { get; }

    /// <summary>The value the place stands at: the one taken next.</summary>
    internal T Current => values[Position];

    /// <summary>The place after this one: the next value, or the first after the last.</summary>
    internal CyclingSequence<T> Next => new(values, Position + 1 == values.Count ? 0 : Position + 1);

    /// <summary>The first place of <paramref name="values"/>.</summary>
    /// <exception cref="ArgumentException">There are no values to take in turn.</exception>
    internal static CyclingSequence<T> StartOf(IReadOnlyList<T> values) => At(values, 0);

    /// <summary>The place <paramref name="position"/> of <paramref name="values"/>, counted from the first.</summary>
    /// <exception cref="ArgumentException">There are no values to take in turn.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The position lies outside the values.</exception>
    internal static CyclingSequence<T> At(IReadOnlyList<T> values, int position)
    {
        if (Ensure.NotNull(values).Count == 0)
            throw new ArgumentException("A cycling sequence takes at least one value in turn.", nameof(values));
        return new(values, Ensure.BetweenInclusive(0, values.Count - 1, position));
    }
}
