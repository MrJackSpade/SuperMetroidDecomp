namespace Engine.World;

/// <summary>
/// Something of a room's with a part to play as each update of the room opens: after the room's liquid has moved
/// and before the session services that open the update, the player's input and everything else. A running
/// <see cref="RoomSequence"/> or a live projectile that is one is called there by the room, the sequences first, in
/// the order they joined the room. The rest of its update comes where its kind's does.
/// </summary>
public interface IOpeningStep
{
    /// <summary>Plays its part of the opening of one update of <paramref name="room"/>.</summary>
    void StepOpening(Room room);
}
