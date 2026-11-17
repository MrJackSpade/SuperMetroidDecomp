namespace SuperMetroid.Core.Spatial;

/// <summary>
/// A share of a move or a speed, from a 256th of it to the whole of it (#2356): how much of a grounded move crosses a
/// slope, how much of a crawler's speed carries along one, or one substep's part of a grapple extension. The game's
/// shares are whole 256ths, so a share is authored as a fraction whose denominator divides 256.
/// </summary>
public readonly record struct SpeedShare
{
    /// <summary>The 256ths in the whole of a move or a speed.</summary>
    private const int WholeParts = 256;

    /// <summary>Keeps a share of <paramref name="twoHundredFiftySixths"/> 256ths.</summary>
    private SpeedShare(int twoHundredFiftySixths) => TwoHundredFiftySixths = twoHundredFiftySixths;

    /// <summary>The whole of the move or the speed: nothing is scaled away.</summary>
    public static SpeedShare Whole { get; } = new(WholeParts);

    /// <summary>The share <paramref name="numerator"/>/<paramref name="denominator"/> of the move or the speed.</summary>
    /// <param name="numerator">The parts of the share, 1 through <paramref name="denominator"/>.</param>
    /// <param name="denominator">The parts of the whole, a divisor of 256.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The denominator does not divide 256, or the share is nothing or more than the whole.
    /// </exception>
    public static SpeedShare Of(int numerator, int denominator)
    {
        int parts = Ensure.BetweenInclusive(1, WholeParts, denominator);
        if (WholeParts % parts != 0)
            throw new ArgumentOutOfRangeException(nameof(denominator), denominator, "A share's denominator divides 256.");
        return new(Ensure.BetweenInclusive(1, parts, numerator) * (WholeParts / parts));
    }

    /// <summary>The share in 256ths of the whole, 1 through 256.</summary>
    [IntegerQuantity("a share of a move or a speed in 256ths of it")]
    public int TwoHundredFiftySixths { get; }
}
