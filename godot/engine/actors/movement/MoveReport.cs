namespace Engine.Actors.Movement;

/// <summary>What happened in one update of a shared way of moving. The mover's owner decides what to do about it.</summary>
/// <param name="HitWall">It was stopped moving across.</param>
/// <param name="HitFloor">It was stopped moving down.</param>
/// <param name="HitCeiling">It was stopped moving up.</param>
/// <param name="ReachedEdge">It came to the end of the ground it was on.</param>
/// <param name="Finished">It reached the end of what it was doing: a path walked, a dive completed.</param>
public readonly record struct MoveReport(bool HitWall, bool HitFloor, bool HitCeiling, bool ReachedEdge, bool Finished)
{
    /// <summary>
    /// Whether sloped ground moved it up or down this update to hold it under its middle: a crawl that went up a
    /// floor's slope, or came down under a ceiling's.
    /// </summary>
    public bool MovedBySlope { get; init; }

    /// <summary>Nothing happened.</summary>
    public static MoveReport Nothing => default;
}
