using Engine.Shared;

namespace Engine.Collision;

/// <summary>What happened when a body was moved along one axis.</summary>
/// <param name="Moved">How far it actually went.</param>
/// <param name="Blocked">Whether something stopped it short.</param>
/// <param name="BlockedByTerrain">Whether what stopped it was terrain.</param>
/// <param name="BlockedBy">The solid collider that stopped it, when one did.</param>
public readonly record struct MoveResult(Pixels Moved, bool Blocked, bool BlockedByTerrain, Collider? BlockedBy)
{
    /// <summary>A move that went the whole way.</summary>
    public static MoveResult Free(Pixels moved) => new(moved, false, false, null);
}
