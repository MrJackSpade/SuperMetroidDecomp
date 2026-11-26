using Engine.Shared;

namespace Engine.World;

/// <summary>
/// What the edge a shot leads with met in the terrain (<see cref="Terrain.MeetShotEdge"/>): whether the terrain stops
/// the shot, and the part of the edge whose tiles the shot reached, which its hit lands on whether it is stopped or not.
/// </summary>
/// <param name="Stops">Whether the terrain stops the shot.</param>
/// <param name="Reached">The part of the edge whose tiles the shot reached; empty when it reached none.</param>
public readonly record struct ShotEdgeMeeting(bool Stops, PixelRect Reached)
{
    /// <summary>The edge met nothing: it lies outside the room, where nothing stops a shot and nothing answers one.</summary>
    public static ShotEdgeMeeting Nothing => default;
}
