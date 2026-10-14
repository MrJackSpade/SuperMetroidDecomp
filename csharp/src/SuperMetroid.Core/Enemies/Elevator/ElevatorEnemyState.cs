using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// Typed projection of the one private word used by elevator enemy <c>$D73F</c>. Native
/// variable A retains the room-side resting Y coordinate while the global status survives
/// travel to the destination room.
/// </summary>
public sealed class ElevatorEnemyState
{
    /// <summary>Physical elevator enemy slot retaining its room-side resting position.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects the elevator actor's private variables from its occupied slot.</summary>
    internal ElevatorEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<ElevatorProgram> Program { get; set; }

    /// <summary>Enemy variable A, captured before an arriving actor is moved to parameter 2.</summary>
    public ushort RestingYPosition
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>
    /// Parameter 1 after the initializer doubles it. This is a byte offset into the two-word
    /// input-mask table at <c>$A3:94E2</c>, not an invented direction boolean.
    /// </summary>
    public ushort DirectionTableByteOffset => _slot.Parameter1;
}
