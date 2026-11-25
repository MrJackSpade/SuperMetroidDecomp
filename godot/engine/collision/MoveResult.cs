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

    /// <summary>A move that went the whole way.</summary>
    public static MoveResult Free(Pixels moved) => new(moved, false, false, null);
}
