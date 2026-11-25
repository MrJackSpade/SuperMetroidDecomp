namespace Engine.World;

/// <summary>
/// A live projectile with a part to play as each update of the room ends: once the room's objects have played and
/// the shots have been tried against the player and against one another, before the room's shaking, its sequences
/// and the view. The room calls it there, in the order the projectiles joined the room, unless projectiles are
/// stopped. The rest of its update comes where its kind's does.
/// </summary>
public interface IClosingStep
{
    /// <summary>Plays its part of the end of one update of <paramref name="room"/>.</summary>
    void StepClosing(Room room);
}
