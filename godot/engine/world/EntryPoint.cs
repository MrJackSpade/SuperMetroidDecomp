using Godot;

namespace Engine.World;

/// <summary>
/// A place in a room where play may begin, as at a save point or a landing: where the player is put, and, when it
/// says so, where the view is put when play begins there. A room's exits and anything else with a place may be
/// entered at too; this is for a place that is nothing else.
/// </summary>
public partial class EntryPoint : Marker2D
{
    /// <summary>Whether play that begins here puts the view at <see cref="ViewAt"/>; otherwise the view is put about the player.</summary>
    [Export]
    public bool PlacesView { get; set; }

    /// <summary>Where the view's top-left corner is put, in the room's pixels, kept inside the room.</summary>
    [Export]
    public Vector2I ViewAt { get; set; }
}
