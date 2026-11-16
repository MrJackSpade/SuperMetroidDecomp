namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// How far a link of Ridley's tail turns, in 256ths of a turn and never negative (#2238): the step a swinging link turns
/// each update, how far a link swings before it starts the next one, and how far apart the tail's links are kept. It counts
/// without wrapping, as the swings it measures stay far inside a turn.
/// </summary>
public readonly record struct RidleyTailTurn : IComparable<RidleyTailTurn>
{
    /// <summary>A turn of <paramref name="steps"/> 256ths of a turn.</summary>
    private RidleyTailTurn(int steps) => Steps = Ensure.AtLeastZero(steps);

    /// <summary>The turn in 256ths of a turn, zero or more.</summary>
    [IntegerQuantity("turn of a Ridley tail link in 256ths of a turn")]
    public int Steps { get; }

    /// <summary>No turn: a link given it does not swing.</summary>
    public static RidleyTailTurn None => default;

    /// <summary>A turn of <paramref name="steps"/> 256ths of a turn, zero or more.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The turn is negative.</exception>
    public static RidleyTailTurn OfSteps(int steps) => new(steps);

    /// <summary>The turn <paramref name="left"/> and <paramref name="right"/> make together.</summary>
    public static RidleyTailTurn operator +(RidleyTailTurn left, RidleyTailTurn right) => new(checked(left.Steps + right.Steps));

    /// <summary>Whether <paramref name="left"/> turns less than <paramref name="right"/>.</summary>
    public static bool operator <(RidleyTailTurn left, RidleyTailTurn right) => left.CompareTo(right) < 0;

    /// <summary>Whether <paramref name="left"/> turns more than <paramref name="right"/>.</summary>
    public static bool operator >(RidleyTailTurn left, RidleyTailTurn right) => left.CompareTo(right) > 0;

    /// <inheritdoc />
    public int CompareTo(RidleyTailTurn other) => Steps.CompareTo(other.Steps);

    /// <inheritdoc />
    public override string ToString() => $"{Steps}/256 turn";
}
