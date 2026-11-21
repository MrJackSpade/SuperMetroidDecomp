namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// One of the Speed Booster's two echoes (#2557): where it stands and how fast it departs toward Samus once her boost is
/// cancelled. The speed state keeps one per <see cref="SpeedEcho"/>.
/// </summary>
public sealed class SamusSpeedEcho
{
    /// <summary>Where the echo stands, or none while there is no echo.</summary>
    public Position16<RoomPixels>? Position { get; internal set; }

    /// <summary>
    /// The departing echo's horizontal speed in whole pixels an update, negative leftward. Cancellation sets
    /// <see cref="SamusMovementDefinitions.SpeedEchoDepartureSpeed"/> leftward while facing left or rightward otherwise.
    /// </summary>
    internal int DepartureSpeed { get; set; }

    /// <summary>Removes the echo and its departure speed.</summary>
    internal void Clear()
    {
        Position = null;
        DepartureSpeed = 0;
    }
}
