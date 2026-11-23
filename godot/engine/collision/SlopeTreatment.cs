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
}
