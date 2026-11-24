namespace Engine.Collision;

/// <summary>How a move treats sloped ground.</summary>
public enum SlopeTreatment
{
    /// <summary>A slope is solid along its surface, and a mover walking into one climbs it.</summary>
    Climb,

    /// <summary>A slope is solid along its surface, and a mover walking into one is stopped as by a wall.</summary>
    Wall,

    /// <summary>A slope is not there at all.</summary>
    Ignore,

    /// <summary>
    /// A slope that holds things up by their middle is solid only to the middle column of the mover's body: the
    /// body's edges pass into it, and the mover stands where its surface is under their middle, climbing it as they
    /// walk. Any other slope is as for <see cref="Climb"/>.
    /// </summary>
    ClimbUnderMiddle,

    /// <summary>A slope that holds things up by their middle is not there at all; any other slope is as for <see cref="Wall"/>.</summary>
    IgnoreMiddleHeld,
}
