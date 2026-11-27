using Engine.Shared;

namespace Engine.Collision;

/// <summary>What happened when a body was moved along one axis.</summary>
/// <param name="Moved">How far it actually went.</param>
/// <param name="Blocked">Whether something stopped it short.</param>
/// <param name="BlockedByTerrain">Whether what stopped it was terrain.</param>
/// <param name="BlockedBy">The solid collider that stopped it, when one did.</param>
public readonly record struct MoveResult(Pixels Moved, bool Blocked, bool BlockedByTerrain, Collider? BlockedBy)
{
    /// <summary>
    /// Whether a move across ended with the body put where sloped ground holds it under its middle: lifted out of a
    /// floor, or brought down from a ceiling it was in or against. Only a move tried where it ends says so.
    /// </summary>
    public bool PutOnSlope { get; init; }

    /// <summary>
    /// The tile of terrain that stopped the move, in room pixels, for a move tried only where it ends that a tile
    /// stopped; otherwise null.
    /// </summary>
    public PixelRect? StoppedByTile { get; init; }

    /// <summary>
    /// For a move tried only where it ends that asked the terrain: the stretch of the body's leading edge, where the
    /// move was tried, that came before whatever stopped it, in the order the edge is asked about; all of the edge
    /// when nothing stopped it. What lies there the move reached, though the body may have been stopped short of it.
    /// Null for any other move.
    /// </summary>
    public PixelRect? Reached { get; init; }

    /// <summary>A move that went the whole way.</summary>
    public static MoveResult Free(Pixels moved) => new(moved, false, false, null);
}
