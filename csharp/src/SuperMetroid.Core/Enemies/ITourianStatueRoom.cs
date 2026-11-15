namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What the Tourian statue room's enemies read from the room as it stands (#2033): the water the released statues'
/// particles splash into and how far the entrance statue has sunk.
/// </summary>
public interface ITourianStatueRoom
{
    /// <summary>
    /// The room line of the room effect's water surface, which the released statues' particles splash through and their
    /// splashes ride; null while the room's effect names no surface.
    /// </summary>
    int? WaterSurface { get; }

    /// <summary>
    /// The entrance statue's vertical offset as it sinks: minus the whole pixels of its descent, zero until it starts and
    /// minus the descent distance once it has finished. The statue's decorations follow it.
    /// </summary>
    int StatueOffset { get; }
}
