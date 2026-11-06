using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The state of the elevator enemy. Its resting height and direction are values the game keeps in the enemy's
/// common record, so they are emptied with that record when the slot is cleared (<see cref="ClearWithRecord"/>);
/// the global elevator status survives travel to the destination room.
/// </summary>
public sealed class ElevatorEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<ElevatorProgram> Program { get; set; }

    /// <summary>The room-side resting height, captured before an arriving actor is moved to parameter 2; only Y is meaningful.</summary>
    public Position16<RoomPixels> RestingHeight { get; internal set; }

    /// <summary>Which way the elevator departs, which input starts it, and which way an arrival returns to rest.</summary>
    public ElevatorTravelDirection Direction { get; internal set; }

    /// <summary>Empties the resting height and direction, when the slot's common record is emptied.</summary>
    internal void ClearWithRecord()
    {
        RestingHeight = Position16<RoomPixels>.Zero;
        Direction = ElevatorTravelDirection.Down;
    }
}
