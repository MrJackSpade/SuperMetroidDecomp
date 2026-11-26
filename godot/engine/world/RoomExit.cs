using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>
/// A way out of a room: a region at its edge. When the player enters it, the room transition to the room and exit it
/// leads to begins. It has no body and no look, and knows nothing of what may stand in front of it.
/// </summary>
[Tool]
public partial class RoomExit : Node2D
{
    /// <summary>The region's width and height, in pixels; its top-left is where this node stands.</summary>
    [Export]
    public Vector2I Size { get; set; } = new(16, 64);

    /// <summary>The scene of the room this exit leads to.</summary>
    [Export(PropertyHint.File, "*.tscn")]
    public string LeadsToRoom { get; set; } = "";

    /// <summary>The name of the exit in that room the player arrives at.</summary>
    [Export]
    public string LeadsToExit { get; set; } = "";

    /// <summary>Where a player arriving by this exit appears, in pixels from this node.</summary>
    [Export]
    public Vector2I ArrivalOffset { get; set; }

    /// <summary>The region of the room the exit covers.</summary>
    public PixelRect Area => Region.Of(this, Size);

    /// <summary>Where a player arriving by this exit appears.</summary>
    public PixelPosition ArrivalPoint => PixelPosition.FromWhole(Area.Left + ArrivalOffset.X, Area.Top + ArrivalOffset.Y);

    /// <inheritdoc/>
    public override void _Draw() => Region.DrawInEditor(this, Size, Colors.Magenta);
}
