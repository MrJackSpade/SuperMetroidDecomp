namespace SuperMetroid.Core.Assets;

/// <summary>Program-order access to Phantoon's gaze directions.</summary>
internal static class PhantoonGazeDirections
{
    /// <summary>The gaze selected by one of the eight clockwise records starting at $A7:CCA7, from up.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The ordinal is outside the eight gaze records.</exception>
    internal static PhantoonGazeDirection At(int clockwiseOrdinal) => clockwiseOrdinal switch
    {
        0 => PhantoonGazeDirection.Up,
        1 => PhantoonGazeDirection.UpRight,
        2 => PhantoonGazeDirection.Right,
        3 => PhantoonGazeDirection.DownRight,
        4 => PhantoonGazeDirection.Down,
        5 => PhantoonGazeDirection.DownLeft,
        6 => PhantoonGazeDirection.Left,
        7 => PhantoonGazeDirection.UpLeft,
        _ => throw new ArgumentOutOfRangeException(nameof(clockwiseOrdinal), clockwiseOrdinal, "Phantoon has eight gaze records."),
    };
}
