namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The updates on which the Mama Turtle climbs out of her shell (#1606), and the way each step jitters (#1619).</summary>
internal static partial class MamaTurtleCadenceDefinitions
{
    /// <summary>
    /// Whether a Mama Turtle's step out of her shell at room height <paramref name="height"/> sidesteps left (#1619,
    /// #2439): on odd heights, so each step a pixel up alternates her sidestep between right and left.
    /// </summary>
    internal static bool LeavingShellSidestepsLeftAt(int height) => int.IsOddInteger(height);

    /// <summary>
    /// The updates on which a Mama Turtle leaving her shell steps up and sideways, counted on the frame counter at the
    /// period's even place.
    /// </summary>
    internal static readonly UpdateCadence LeavingShellStep = new(2, 0);
}
