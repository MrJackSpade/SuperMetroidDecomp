using Godot;

namespace Engine.World;

/// <summary>
/// The part of a room's tiles that lies in front of the things standing in the room: grass she walks behind, the near
/// side of a pipe. Its terrain makes it, keeps it at its own depth (<see cref="RoomDepth.TilesInFront"/>) and draws
/// it; it shows the terrain's changes of colour with it.
/// </summary>
[Tool]
public partial class TerrainFront : Node2D
{
    /// <summary>The terrain whose tiles' front parts it shows.</summary>
    internal Terrain? Of { get; set; }

    /// <inheritdoc/>
    public override void _Draw() => Of?.DrawFront(this);
}
