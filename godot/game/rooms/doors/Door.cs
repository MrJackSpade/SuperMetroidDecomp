using Engine.World;
using Godot;

namespace Game;

/// <summary>
/// A door of this game: a way out of a room that knows what the cartridge's door knows of the passage through it.
/// Samus is carried through by <see cref="DoorTransitionMode"/>, which reads these.
/// </summary>
[Tool]
public partial class Door : RoomExit
{
    /// <summary>The way Samus travels through a door.</summary>
    public enum Way
    {
        /// <summary>To the right.</summary>
        Right,

        /// <summary>To the left.</summary>
        Left,

        /// <summary>Downward.</summary>
        Down,

        /// <summary>Upward.</summary>
        Up,
    }

    /// <summary>The way Samus travels through it.</summary>
    [Export]
    public Way Travel { get; set; }

    /// <summary>The screen of the room it leads to that the view comes to rest on: its column and row.</summary>
    [Export]
    public Vector2I ToScreen { get; set; }

    /// <summary>How far Samus is moved along the way of travel on each update of the door's scroll, in pixels.</summary>
    [Export]
    public double SamusStep { get; set; }

    /// <summary>
    /// Whether the passage is the door's own scroll. A door whose arrival something else makes, an elevator's or the
    /// tube's, is passed by the engine's plain change of rooms, with the arrival its exit gives.
    /// </summary>
    [Export]
    public bool Scrolls { get; set; } = true;
}
