using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The state of the elevator enemy. Its resting height and direction are values the cartridge keeps in the enemy's
/// common record, so they are emptied with that record when the slot is cleared (<see cref="ClearWithRecord"/>);
/// the global elevator status survives travel to the destination room.
/// </summary>
public sealed class ElevatorEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<ElevatorProgram> Program { get; set; }

    /// <summary>The room-side resting height, captured before an arriving actor is moved to parameter 2; only Y is meaningful.</summary>
    public Position16<RoomPixels> RestingHeight { get; internal set; }

    /// <summary>
    /// Population parameter 1 doubled, wrapping at sixteen bits (0 through 65534): the byte offset of the direction's
    /// input mask in a two-entry table, zero for down and two for up; any nonzero offset travels upward.
    /// </summary>
    public int DirectionTableByteOffset { get; internal set; }

    /// <summary>Empties the resting height and direction, when the slot's common record is emptied.</summary>
    internal void ClearWithRecord()
    {
        RestingHeight = Position16<RoomPixels>.Zero;
        DirectionTableByteOffset = 0;
    }
}
