namespace Engine.Collision;

/// <summary>
/// The ways one move may differ from the plain one. The plain move is stopped by terrain and by solids, climbs
/// slopes one pixel up for each pixel across, and does not follow the ground down.
/// </summary>
public sealed record MoveOptions
{
    /// <summary>The plain move.</summary>
    public static MoveOptions Plain { get; } = new();

    /// <summary>How sloped ground is treated.</summary>
    public SlopeTreatment Slopes { get; init; } = SlopeTreatment.Climb;

    /// <summary>The most pixels a mover climbs for each pixel it moves across a slope.</summary>
    public int ClimbPerPixel { get; init; } = 1;

    /// <summary>
    /// The most pixels a mover that started on the ground is brought down, after moving across, to stay on ground that
    /// falls away beneath it; zero leaves it in the air.
    /// </summary>
    public int FollowGroundDown { get; init; }

    /// <summary>How much of its way the move tries against what could stop it.</summary>
    public WayTried Tries { get; init; } = WayTried.EveryPixel;

    /// <summary>
    /// For a move up or down tried only where it ends: whether the tiles its leading edge would end in are tried from
    /// the edge's right end, and not from its left. The first found to stop the move decides where the body rests.
    /// </summary>
    public bool TriesFromRight { get; init; }

    /// <summary>
    /// For a move up tried only where it ends, that reaches a ceiling's slope which holds things by their middle:
    /// whether the body is brought up to rest against the slope's surface, however far that is short of the move, as
    /// the cartridge's enemies are, and not left where the move began, as the player is.
    /// </summary>
    public bool RestsUnderSlopes { get; init; }

    /// <summary>Whether solid colliders stop the move, other than those that stand as part of the room.</summary>
    public bool StoppedBySolids { get; init; } = true;

    /// <summary>Whether terrain stops the move, and with it the solid colliders that stand as part of the room (<see cref="Collider.StandsAsTerrain"/>).</summary>
    public bool StoppedByTerrain { get; init; } = true;
}
