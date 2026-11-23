using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>
/// A region of a room the view may be kept out of. While closed, the camera does not show it; once opened, it scrolls
/// in like the rest of the room. It can be told to "open" or "close".
/// </summary>
[Tool]
public partial class CameraZone : Node2D, IPlaced, ITriggerTarget
{
    /// <summary>The region's width and height, in pixels; its top-left is where this node stands.</summary>
    [Export]
    public Vector2I Size { get; set; } = new(256, 224);

    /// <summary>Whether the view may show the region when the room is set up.</summary>
    [Export]
    public bool StartsOpen { get; set; }

    /// <inheritdoc/>
    [Export]
    public Condition? PresentWhen { get; set; }

    /// <summary>Whether the view may show the region right now.</summary>
    public bool IsOpen { get; private set; }

    /// <summary>The region of the room the zone covers.</summary>
    public PixelRect Area => Region.Of(this, Size);

    /// <summary>Puts the zone in its starting state.</summary>
    internal void Setup() => IsOpen = StartsOpen;

    /// <inheritdoc/>
    public void Triggered(string what)
    {
        IsOpen = what switch
        {
            "open" => true,
            "close" => false,
            _ => throw new ArgumentException($"A camera zone can be told to \"open\" or \"close\", not \"{what}\".", nameof(what)),
        };
    }

    /// <inheritdoc/>
    public override void _Draw() => Region.DrawInEditor(this, Size, Colors.Orange);
}
