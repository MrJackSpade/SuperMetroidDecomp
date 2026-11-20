namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Where the gunship's shake stands in one of its authored sequences (<see cref="EndingShipShake"/>, #1902): the
/// displacement the next update moves the ship by, and the cursor after it, which starts the sequence again after its
/// last displacement.
/// </summary>
internal readonly record struct EndingShipShakeCursor
{
    /// <summary>Holds a cursor at <paramref name="place"/> in <paramref name="sequence"/>.</summary>
    private EndingShipShakeCursor(IReadOnlyList<SignedSixteenSixteen> sequence, int place)
    {
        Sequence = sequence;
        Place = place;
    }

    /// <summary>The shake sequence the cursor plays.</summary>
    private IReadOnlyList<SignedSixteenSixteen> Sequence { get; }

    /// <summary>How many displacements of <see cref="Sequence"/> the cursor has played since the sequence last started.</summary>
    private int Place { get; }

    /// <summary>A cursor at the start of <paramref name="sequence"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="sequence"/> is empty.</exception>
    internal static EndingShipShakeCursor StartOf(IReadOnlyList<SignedSixteenSixteen> sequence)
    {
        if (Ensure.NotNull(sequence).Count == 0)
            throw new ArgumentException("A ship shake plays at least one displacement.", nameof(sequence));
        return new(sequence, 0);
    }

    /// <summary>The displacement the ship moves by on the update the cursor stands at.</summary>
    /// <exception cref="InvalidOperationException">The cursor was never started on a sequence.</exception>
    internal SignedSixteenSixteen Displacement => StartedSequence[Place];

    /// <summary>The cursor after this update's displacement: the next one, or the sequence's first after its last.</summary>
    /// <exception cref="InvalidOperationException">The cursor was never started on a sequence.</exception>
    internal EndingShipShakeCursor Advanced()
    {
        IReadOnlyList<SignedSixteenSixteen> sequence = StartedSequence;
        return new(sequence, Place + 1 == sequence.Count ? 0 : Place + 1);
    }

    /// <summary>The sequence a started cursor plays.</summary>
    private IReadOnlyList<SignedSixteenSixteen> StartedSequence =>
        Sequence ?? throw new InvalidOperationException("The ship shake has not started a sequence.");
}
