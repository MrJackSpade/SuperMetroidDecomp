namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>The way an elevator's ride travels on the screen (#2569).</summary>
public static class ElevatorTravelDirections
{
    /// <summary>The vertical direction <paramref name="travel"/> rides toward.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="travel"/> is undefined.</exception>
    public static VerticalDirection Vertically(this ElevatorTravelDirection travel) => travel switch
    {
        ElevatorTravelDirection.Down => VerticalDirection.Down,
        ElevatorTravelDirection.Up => VerticalDirection.Up,
        _ => throw new ArgumentOutOfRangeException(nameof(travel), travel, $"Undefined {nameof(ElevatorTravelDirection)}."),
    };
}
