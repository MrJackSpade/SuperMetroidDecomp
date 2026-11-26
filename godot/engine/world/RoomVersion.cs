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
    /// The set of music this version's music is of, by name, for a game whose music comes in sets of which one is
    /// loaded at a time; empty when it names none, and the set already loaded stays. The engine's own flow of sound
    /// has no sets and does not read it.
    /// </summary>
    [Export]
    public string MusicSet { get; set; } = "";

    /// <summary>
    /// Whether this version asks for the theme of whatever set of music is loaded, not for a piece of its own.
    /// <see cref="Music"/> then names the theme of the set the rooms about it load, which is what plays where music
    /// has no sets.
    /// </summary>
    [Export]
    public bool MusicIsTheme { get; set; }

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

    /// <summary>
    /// Whether this version of the room shows none of its blocks to an ability that reveals by sight: nothing hidden
    /// is shown for what it is, and no tile shows as it looks seen.
    /// </summary>
    [Export]
    public bool HidesBlocksFromSight { get; set; }
}
