namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>What has armed an elevator's pseudo-door for the coming door transition.</summary>
public enum ElevatorJourneyFlag
{
    /// <summary>Nothing: an ordinary door finishes at once, and the elevator waits for a collision.</summary>
    None,

    /// <summary>Samus's collision with the elevator's pseudo-door, published before the next enemy pass.</summary>
    DoorContact,

    /// <summary>An elevator door load, carried into the destination room so its elevator starts the arrival.</summary>
    DoorLoad,
}
