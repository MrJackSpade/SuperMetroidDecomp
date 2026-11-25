namespace Engine.World;

/// <summary>
/// A live projectile or enemy with a part to play as each update of the room ends: once the room's objects have
/// played and the shots have been tried against the player and against one another, before the room's shaking, its
/// sequences and the view. The room calls it there, the projectiles first and then the enemies, each in the order
/// they joined the room, unless its kind is stopped; an enemy only when it is one that takes its turn. The rest of
/// its update comes where its kind's does. What it launches there first takes a turn on the next update, and the
/// last random number it finds is the update's last.
/// </summary>
public interface IClosingStep
{
    /// <summary>Plays its part of the end of one update of <paramref name="room"/>.</summary>
    void StepClosing(Room room);
}
