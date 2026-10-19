namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>The elevator journey words shared by the departure and destination rooms' elevator actors.</summary>
internal sealed class ElevatorJourneyState
{
    /// <summary>What has armed the elevator's pseudo-door: nothing, a collision with it, or an elevator door load.</summary>
    internal ElevatorJourneyFlag Flags { get; set; }

    /// <summary>WRAM $0E18, shared across the departure and destination room actors.</summary>
    internal ElevatorActorStatus Status { get; set; }

    /// <summary>Whether the active elevator is travelling upward; otherwise it travels downward.</summary>
    internal bool TravelsUpward { get; set; }
}
