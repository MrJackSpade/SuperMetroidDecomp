namespace Engine.Collision;

/// <summary>
/// The ways one move may differ from the plain one. The plain move is stopped by terrain and by solids, climbs
/// slopes one pixel up for each pixel across, and does not follow the ground down.
/// </summary>
public sealed record MoveOptions
{
    /// <summary>The plain move.</summary>
    public static MoveOptions Plain { get; } = new();

    /// <summary>
    /// The plain move of anything that is not the player, an enemy or a creature: stopped by terrain, by the solids
    /// that stand as part of the room and by the room's ways out, and by no other solid. On the cartridge a solid
    /// enemy is solid to the player alone; an enemy meets the room's blocks and nothing else, a doorway's blocks
    /// among them, which only the player passes into.
    /// </summary>
    public static MoveOptions AmongBlocks { get; } = new() { StoppedBySolids = false, StoppedByWaysOut = true };

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
    /// For a move up or down tried only where it ends, that is stopped by sloped ground which holds things by their
    /// middle: whether the body is brought to rest against the slope's surface, wherever that lies from where the
    /// move began, at the end of its pixel that lies against the slope as against any other terrain; so the
    /// cartridge's enemies rest. A move that does not say so is the player's: stopped going down the body goes the
    /// whole pixels of its move, less what the slope takes back, and keeps its part of a pixel, and stopped going
    /// up it stays where it began.
    /// </summary>
    public bool RestsAgainstSlopes { get; init; }

    /// <summary>
    /// For a move tried only where it ends: whether, stopped by terrain going right or down, the body rests at the
    /// start of its pixel, its part of a pixel dropped as it is going left or up, and not at the pixel's far end.
    /// </summary>
    public bool RestsAtPixelStart { get; init; }

    /// <summary>Whether solid colliders stop the move, other than those that stand as part of the room.</summary>
    public bool StoppedBySolids { get; init; } = true;

    /// <summary>Whether terrain stops the move, and with it the solid colliders that stand as part of the room (<see cref="Collider.StandsAsTerrain"/>).</summary>
    public bool StoppedByTerrain { get; init; } = true;

    /// <summary>
    /// Whether the room's ways out (<see cref="CollisionWorld.WaysOut"/>) stop the move as terrain does, for a move
    /// terrain stops. They do not, unless the move says so: a way out is open to whoever leaves by it.
    /// </summary>
    public bool StoppedByWaysOut { get; init; }
}
