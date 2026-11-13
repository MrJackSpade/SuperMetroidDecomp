namespace SuperMetroid.Core.Spatial;

/// <summary>
/// A turn divided into equal arcs, each holding one value: the first arc opens at <see cref="Start"/> and the rest follow
/// it in the direction the angle grows. An angle chooses the value of the arc it lies in, as a facing, a pose or a
/// picture is chosen by which part of the turn a heading points into. A <see cref="RotationAngle"/> chooses by its whole
/// steps.
/// </summary>
/// <typeparam name="T">The value each arc holds.</typeparam>
internal sealed class TurnArcs<T>
{
    /// <summary>The steps of a full turn.</summary>
    private const int StepsPerTurn = RotationAngle.StepsPerTurn;

    /// <summary>The arcs' values in turn order from <see cref="Start"/>.</summary>
    private readonly T[] _values;

    /// <summary>Divides the turn into one equal arc for each of <paramref name="values"/>, the first opening at <paramref name="start"/>.</summary>
    /// <param name="start">Where the first arc opens.</param>
    /// <param name="values">The arcs' values in turn order; their count divides the turn's 256 steps evenly.</param>
    /// <exception cref="ArgumentException">The count does not divide the turn into equal whole-step arcs.</exception>
    internal TurnArcs(TrigonometryAngle start, params T[] values)
    {
        Ensure.NotNull(values);
        if (values.Length == 0 || StepsPerTurn % values.Length != 0)
            throw new ArgumentException($"{values.Length} arcs do not divide a turn of {StepsPerTurn} steps evenly.", nameof(values));
        Start = start;
        _values = [.. values];
    }

    /// <summary>Where the first arc opens.</summary>
    internal TrigonometryAngle Start { get; }

    /// <summary>The arcs' values in turn order from <see cref="Start"/>.</summary>
    internal IReadOnlyList<T> InTurnOrder => _values;

    /// <summary>The value of the arc <paramref name="angle"/> lies in.</summary>
    internal T At(TrigonometryAngle angle) => _values[(angle - Start).ToAngle().Sector(_values.Length)];

    /// <summary>The value of the arc the whole steps of <paramref name="angle"/> lie in.</summary>
    internal T At(RotationAngle angle) => At(angle.ToTrigonometryAngle());

    /// <summary>Where the one arc holding <paramref name="value"/> opens.</summary>
    /// <exception cref="ArgumentException">No arc, or more than one, holds the value.</exception>
    internal TrigonometryAngle StartOf(T value)
    {
        int[] arcs = [.. Enumerable.Range(0, _values.Length).Where(arc => EqualityComparer<T>.Default.Equals(_values[arc], value))];
        return arcs.Length == 1
            ? Start + TrigonometryAngle.Wrapped(arcs[0] * (StepsPerTurn / _values.Length))
            : throw new ArgumentException($"{arcs.Length} arcs hold {value}; exactly one must.", nameof(value));
    }
}
