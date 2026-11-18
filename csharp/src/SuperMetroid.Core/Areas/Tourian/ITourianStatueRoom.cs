namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// What the Tourian statue room's enemies read from the room as it stands (#2033): the water the released statues'
/// particles splash into, how far the entrance statue has sunk, and the statues' shared animation state, which the room
/// holds from its setup so that its grey door can read whether the statue has finished (#2486).
/// </summary>
public interface ITourianStatueRoom
{
    /// <summary>
    /// The statues' shared animation state, set and cleared by the statue animations, the descent and the decorations,
    /// whether the statue's sinking has finished among it.
    /// </summary>
    TourianStatueAnimation Animation { get; }

    /// <summary>
    /// The room line of the room effect's water surface, which the released statues' particles splash through and their
    /// splashes ride.
    /// </summary>
    int WaterSurface { get; }

    /// <summary>
    /// The entrance statue's vertical offset as it sinks: minus the whole pixels of its descent, zero until it starts and
    /// minus the descent distance once it has finished. The statue's decorations follow it.
    /// </summary>
    int StatueOffset { get; }
}
