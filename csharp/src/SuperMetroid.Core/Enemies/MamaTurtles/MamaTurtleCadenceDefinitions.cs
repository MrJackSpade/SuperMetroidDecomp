namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The updates on which the Mama Turtle climbs out of her shell (#1606).</summary>
internal static class MamaTurtleCadenceDefinitions
{
    /// <summary>
    /// The updates on which a Mama Turtle leaving her shell steps up and sideways: every second update of the frame
    /// counter of 0 through 255, on its even place.
    /// </summary>
    internal static readonly UpdateCadence LeavingShellStepEverySecondUpdate = new(2, 0);
}
