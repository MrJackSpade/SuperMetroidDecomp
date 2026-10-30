namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A colour that strengthens in equal steps down the screen (#1468), such as the Ceres haze: no colour above
/// <see cref="FirstLine"/>, one step of <see cref="Steps"/> from there, and one step more every
/// <see cref="LinesPerStep"/> lines until the colour is whole. <see cref="Shift"/> steps are then added to every line,
/// never leaving less than none, as the colour fades in or out. A line's colour is each channel scaled by its strength
/// in steps, rounded and held at the channel's maximum.
/// </summary>
public sealed record LineColourSteps
{
    /// <summary>The first screen line with any colour.</summary>
    public int FirstLine { get; }

    /// <summary>The lines each step covers; one or more.</summary>
    public int LinesPerStep { get; }

    /// <summary>The steps from no colour to the whole colour; one or more.</summary>
    public int Steps { get; }

    /// <summary>
    /// The steps added to every line's strength, from minus <see cref="Steps"/> to <see cref="Steps"/>: negative while
    /// the colour is faded.
    /// </summary>
    public int Shift { get; }

    /// <summary>Describes a stepped colour.</summary>
    /// <param name="firstLine">The first screen line with any colour.</param>
    /// <param name="linesPerStep">The lines each step covers; one or more.</param>
    /// <param name="steps">The steps from no colour to the whole colour; one or more.</param>
    /// <param name="shift">The steps added to every line's strength, from minus <paramref name="steps"/> to <paramref name="steps"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">A value is out of range.</exception>
    public LineColourSteps(int firstLine, int linesPerStep, int steps, int shift)
    {
        FirstLine = Ensure.BetweenInclusive(0, DisplayScreen.Height, firstLine);
        LinesPerStep = Ensure.BetweenInclusive(1, DisplayScreen.Height, linesPerStep);
        Steps = Ensure.BetweenInclusive(1, DisplayScreen.Height, steps);
        Shift = Ensure.BetweenInclusive(-Steps, Steps, shift);
    }
}
