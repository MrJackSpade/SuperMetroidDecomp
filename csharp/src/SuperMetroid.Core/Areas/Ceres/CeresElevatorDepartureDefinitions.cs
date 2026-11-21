namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Where Samus must stand on the Ceres elevator for it to carry her away: a room rectangle whose left and bottom edges
/// lie just outside it and whose right and top edges lie inside it, as the elevator's trigger tests them; and how long she
/// waits on it before the station fades out.
/// </summary>
internal static partial class CeresElevatorDepartureDefinitions
{
    /// <summary>The updates Samus holds on the elevator once she reaches it, before the station fades to black.</summary>
    internal const int DepartureHoldFrames = 60;

    /// <summary>The room X, in whole pixels, just left of the trigger: Samus must stand right of it.</summary>
    internal const int LeftOfTriggerX = 112;

    /// <summary>The trigger's rightmost room X, in whole pixels.</summary>
    internal const int TriggerRightX = 144;

    /// <summary>The trigger's topmost room Y, in whole pixels.</summary>
    internal const int TriggerTopY = 75;

    /// <summary>The room Y, in whole pixels, just below the trigger: Samus must stand above it.</summary>
    internal const int BelowTriggerY = 128;
}
