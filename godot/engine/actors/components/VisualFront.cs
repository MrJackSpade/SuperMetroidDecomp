using Godot;

namespace Engine.Actors.Components;

/// <summary>
/// The part of a visual's picture that lies in front of the things standing in a room: a picture made from the
/// room's blocks has that part beside it, as a tile's has. Its visual makes it, keeps it at the depth of the tiles'
/// own such part (<see cref="World.RoomDepth.TilesInFront"/>) and draws it; it lies, turns and changes colour with
/// the visual.
/// </summary>
[Tool]
public partial class VisualFront : Node2D
{
    /// <summary>The visual whose picture's front part it draws.</summary>
    internal ActorVisual? Of { get; set; }

    /// <inheritdoc/>
    public override void _Draw() => Of?.DrawFront(this);
}
