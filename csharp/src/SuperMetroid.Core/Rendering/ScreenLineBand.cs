namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A band of whole screen lines the full width of the screen (#1468), from <see cref="FirstLine"/> up to
/// <see cref="EndLine"/>; a band whose ends meet shows nothing.
/// </summary>
public readonly record struct ScreenLineBand
{
    /// <summary>The first line of the band.</summary>
    public int FirstLine { get; }

    /// <summary>The line after the band's last line.</summary>
    public int EndLine { get; }

    /// <summary>Creates a band of screen lines.</summary>
    /// <param name="firstLine">The first line of the band.</param>
    /// <param name="endLine">The line after the band's last line, at or below <paramref name="firstLine"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The band is not within the screen.</exception>
    public ScreenLineBand(int firstLine, int endLine)
    {
        FirstLine = Ensure.BetweenInclusive(0, DisplayScreen.Height, firstLine);
        EndLine = Ensure.BetweenInclusive(FirstLine, DisplayScreen.Height, endLine);
    }

    /// <summary>Every line below the HUD: the gameplay area.</summary>
    public static ScreenLineBand Gameplay { get; } = new(DisplayScreen.HudHeight, DisplayScreen.Height);
}
