namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The updates on which the Mama Turtle climbs out of her shell (#1606), and the way each step jitters (#1619).</summary>
internal static class MamaTurtleCadenceDefinitions
{
    /// <summary>
    /// The phase of a Mama Turtle's height, as she climbs out of her shell, on which her step sidesteps left (#1619): set on
    /// odd heights, so each step a pixel up alternates her sidestep between right and left.
    /// </summary>
    internal static readonly UpdateCounterField LeavingShellSidestepsLeft = new(0, 1);

    /// <summary>
    /// The updates on which a Mama Turtle leaving her shell steps up and sideways: every second update of the frame
    /// counter of 0 through 255, on its even place.
    /// </summary>
    internal static readonly UpdateCadence LeavingShellStepEverySecondUpdate = new(2, 0);
}
