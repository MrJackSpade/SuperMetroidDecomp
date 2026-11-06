namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>Which way an elevator carries Samus out of its room.</summary>
public enum ElevatorTravelDirection
{
    /// <summary>Down: pressing down departs, and an arriving elevator rises back to its resting height.</summary>
    Down,
    /// <summary>Up: pressing up departs, and an arriving elevator descends back to its resting height.</summary>
    Up,
}
