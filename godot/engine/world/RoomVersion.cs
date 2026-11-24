using Godot;

namespace Engine.World;

/// <summary>
/// One version of a room's own settings, used when its condition holds. A room that differs by what has happened lists
/// its versions in order; the first whose condition holds is used, and the last has none.
/// </summary>
[GlobalClass]
public partial class RoomVersion : Resource
{
    /// <summary>The condition under which this version is used; null for the version used when no other is.</summary>
    [Export]
    public Condition? When { get; set; }

    /// <summary>The terrain's layout file in this version.</summary>
    [Export(PropertyHint.File)]
    public string LayoutFile { get; set; } = "";

    /// <summary>The music played in this version, by name; empty for silence.</summary>
    [Export]
    public string Music { get; set; } = "";

    /// <summary>Whether this version asks for no music of its own: whatever is playing as the room is entered goes on.</summary>
    [Export]
    public bool KeepsMusic { get; set; }

    /// <summary>
    /// How each screen of the room lets the view in, in this version: a <see cref="ScreenScroll"/> for each, row by row
    /// and each row from the left. Empty for a room every screen of which is open.
    /// </summary>
    [Export]
    public int[] Screens { get; set; } = [];

    /// <summary>The name of this version's scroll layout, for a game that keeps more about one than the room does; empty when it has none.</summary>
    [Export]
    public string ScrollLayout { get; set; } = "";

    /// <summary>The names of the layout's patches in place in this version.</summary>
    [Export]
    public string[] Patches { get; set; } = [];
}
