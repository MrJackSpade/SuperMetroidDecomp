using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>The state of the elevator enemy.</summary>
public sealed class ElevatorEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<ElevatorProgram> Program { get; set; }

    /// <summary>The room-side resting height, captured before an arriving actor is moved to parameter 2; only Y is meaningful.</summary>
    public Position16<RoomPixels> RestingHeight { get; internal set; }

    /// <summary>Which way the elevator departs, which input starts it, and which way an arrival returns to rest.</summary>
    public ElevatorTravelDirection Direction { get; internal set; }
}
