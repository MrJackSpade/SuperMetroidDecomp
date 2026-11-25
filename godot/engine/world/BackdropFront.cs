using Godot;

namespace Engine.World;

/// <summary>
/// The part of a backdrop's picture that lies in front of the things standing in the room: the near wall of a
/// chamber, drawn by the cartridge over whoever stands in it. Its backdrop makes it, keeps it at its own depth
/// (<see cref="RoomDepth.BackdropInFront"/>) and draws it; it moves and changes colour with the backdrop.
/// </summary>
[Tool]
public partial class BackdropFront : Node2D
{
    /// <summary>The backdrop whose picture's front part it shows.</summary>
    internal Backdrop? Of { get; set; }

    /// <inheritdoc/>
    public override void _Draw() => Of?.DrawFront(this);
}
