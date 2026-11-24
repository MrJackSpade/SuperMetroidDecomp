using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>
/// One screen of a room, by where this node stands, as something that can be told how to let the view in. The
/// screen's state is the room's (<see cref="IScrollCells"/>); this is the handle a trigger, a boss or a sequence
/// tells: "close" keeps the view out; "open" opens the screen the way this zone opens it (<see cref="OpensTo"/>);
/// "open_fully" and "open_to_top" open it one way or the other whatever the zone's own way.
/// </summary>
[Tool]
public partial class CameraZone : Node2D, IPlaced, ITriggerTarget
{
    /// <summary>How the screen stands once told to "open".</summary>
    [Export]
    public ScreenScroll OpensTo { get; set; } = ScreenScroll.Open;

    /// <summary>What the zone is told as the room is set up, for a screen something present in the room starts otherwise than its layout has it; empty for nothing.</summary>
    [Export]
    public string ToldAtSetup { get; set; } = "";

    /// <inheritdoc/>
    [Export]
    public Condition? PresentWhen { get; set; }

    private Room? _room;

    /// <summary>The screen's column in the room.</summary>
    public int Column => Mathf.FloorToInt(Position.X / Camera.Screen);

    /// <summary>The screen's row in the room. A zone may stand below the room's last row: a game whose scroll cells outlive a room keeps such a screen's state too.</summary>
    public int Row => Mathf.FloorToInt(Position.Y / Camera.Screen);

    /// <summary>Whether the view may show the screen right now.</summary>
    public bool IsOpen => Told.Session.ScrollCells.Read(Column, Row) != ScreenScroll.Closed;

    /// <summary>The room the zone is in.</summary>
    private Room Told => _room ?? throw new InvalidOperationException($"The camera zone \"{Name}\" is not in a room that has been set up.");

    /// <summary>Takes up the room the zone is in.</summary>
    internal void Setup(Room room)
    {
        _room = room;
        if (ToldAtSetup.Length > 0)
            Triggered(ToldAtSetup);
    }

    /// <inheritdoc/>
    public void Triggered(string what)
    {
        ScreenScroll state = what switch
        {
            "close" => ScreenScroll.Closed,
            "open" => OpensTo,
            "open_fully" => ScreenScroll.Open,
            "open_to_top" => ScreenScroll.OpenAlignedToTop,
            _ => throw new ArgumentException(
                $"A camera zone can be told to \"close\", \"open\", \"open_fully\" or \"open_to_top\", not \"{what}\".", nameof(what)),
        };
        Told.Session.ScrollCells.Write(Column, Row, state);
    }

    /// <inheritdoc/>
    public override void _Draw() => Region.DrawInEditor(this, new Vector2I(Camera.Screen, Camera.Screen), Colors.Orange);
}
