namespace Engine.Shared;

/// <summary>
/// Where a picture of a timeline lies against the tiles of the room it is shown in ("lies", on the timeline for all
/// its steps or on one step for that step), for what is launched or left and shows the timeline: a shot flying, its
/// ending, an effect, what a death leaves.
/// </summary>
public enum LiesAgainstTiles
{
    /// <summary>Where the thing that shows the picture stands: neither the step nor its timeline says anything.</summary>
    WhereItStands,

    /// <summary>In front of every tile of the room, the ones in front of the room's things too.</summary>
    InFrontOfTiles,

    /// <summary>Behind the room's tiles.</summary>
    BehindRoom,
}
