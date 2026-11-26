using Engine.World;
using Godot;

namespace Game.Rooms.Brinstar;

/// <summary>
/// The view in the Dachora's room, drawn toward the shaft that drops from its top corridor: on every update the
/// screen over the top of the shaft stands open, with the view's corner above and left of the shaft's bounds, the
/// view is moved right a few pixels, and the player's next following starts from there. A screen open only with the
/// view resting at its top does not move it. No screen is opened or closed by it.
/// </summary>
public partial class DachoraShaftCamera : RoomSequence
{
    /// <summary>The camera zone over the top of the shaft.</summary>
    [Export]
    public NodePath ShaftTop { get; set; } = new();

    /// <summary>The view's top must lie above this, in the room's pixels, for it to be moved.</summary>
    [Export]
    public int ViewAbove { get; set; } = 1280;

    /// <summary>The view's left edge must lie left of this for it to be moved.</summary>
    [Export]
    public int ViewLeftOf { get; set; } = 896;

    /// <summary>How far right the view is moved on each update it is moved, in pixels.</summary>
    [Export]
    public int StepPixels { get; set; } = 3;

    /// <summary>It goes on for as long as the room is played.</summary>
    protected override bool Play(Room room, int elapsed)
    {
        CameraZone top = GetNode(ShaftTop) as CameraZone
            ?? throw new InvalidOperationException($"The shaft's view \"{Name}\" is given \"{ShaftTop}\", which is no camera zone.");
        Camera view = room.Session.Camera;
        if (room.Session.ScrollCells.Read(top.Column, top.Row) == ScreenScroll.Open && view.Top < ViewAbove && view.Left < ViewLeftOf)
            view.MoveLeftEdgeTo(view.Left + StepPixels);
        return false;
    }
}
