namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>The elevator journey state shared by the departure and destination rooms' elevator actors.</summary>
internal sealed partial class ElevatorJourneyState
{
    /// <summary>What has armed the elevator's pseudo-door: nothing, a collision with it, or an elevator door load.</summary>
    internal ElevatorArming DoorArming { get; set; }

    /// <summary>The elevator status, shared across the departure and destination room actors.</summary>
    internal ElevatorActorStatus Status { get; set; }

    /// <summary>The way the active elevator travels (#2277).</summary>
    internal VerticalDirection Travel { get; set; }
}
