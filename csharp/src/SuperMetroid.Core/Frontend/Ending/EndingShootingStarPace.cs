namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The segment a shooting star's flight plays (#2359), named for the pace its picture changes at: an animated star's
/// pictures after the lead-in each hold for its period until the full star, and a still star keeps its first visible
/// picture.
/// </summary>
internal readonly record struct EndingShootingStarPace
{
    /// <summary>The updates each picture of an animated star holds; a still star has none.</summary>
    private readonly int period;

    /// <summary>An animated pace of <paramref name="period"/> updates a picture.</summary>
    private EndingShootingStarPace(int period)
    {
        this.period = Ensure.BetweenInclusive(1, int.MaxValue, period);
        Animates = true;
    }

    /// <summary>A still star's pace, which keeps its first visible picture.</summary>
    internal static EndingShootingStarPace Still => default;

    /// <summary>An animated star's pace, each picture holding <paramref name="period"/> updates.</summary>
    internal static EndingShootingStarPace Animated(int period) => new(period);

    /// <summary>Whether the star's picture animates.</summary>
    internal bool Animates { get; }

    /// <summary>The updates each picture of an animated star holds.</summary>
    /// <exception cref="InvalidOperationException">The star is still.</exception>
    internal int Period => Animates ? period : throw new InvalidOperationException("A still shooting star's picture holds no period.");
}
