namespace SuperMetroid.Core.Fx;

/// <summary>One band of the scrolling sky as a frame shows it (#1687): the room rows it covers and how far it has drifted.</summary>
/// <param name="FirstRow">The first room row the band covers.</param>
/// <param name="EndRow">The room row below the band's last.</param>
/// <param name="Offset">How many whole pixels the band has drifted sideways.</param>
public readonly record struct ShownSkyBand(int FirstRow, int EndRow, int Offset)
{
    /// <summary>The room row below the band's last, below its first row.</summary>
    public int EndRow { get; init; } = Ensure.BetweenInclusive(FirstRow + 1, int.MaxValue, EndRow);

    /// <summary>Whether the band covers room row <paramref name="row"/>.</summary>
    /// <param name="row">A room row; rows above the room are negative.</param>
    public bool Covers(int row) => row >= FirstRow && row < EndRow;
}
