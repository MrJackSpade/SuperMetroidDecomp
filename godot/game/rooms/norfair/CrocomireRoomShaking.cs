using Engine.World;
using Game.Enemies;
using Godot;

namespace Game.Rooms.Norfair;

/// <summary>
/// The wall of Crocomire's room rattling as his skeleton gathers itself behind it: while he rumbles there, the
/// room's terrain is shown shifted up and down by as much as he says, from where it is shown at rest.
/// </summary>
public partial class CrocomireRoomShaking : RoomSequence
{
    /// <summary>Crocomire.</summary>
    [Export]
    public NodePath Crocomire { get; set; } = new();

    /// <summary>How far the terrain is shown from where it is while nothing rattles it, as the room has it.</summary>
    private Vector2I _rest;

    /// <inheritdoc/>
    protected override void OnStarted() => _rest = Room.Terrain.ShownShift;

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        // Once he is gone from the room there is nothing left to rattle it.
        int rumble = GetNodeOrNull(Crocomire) is Crocomire crocomire && !crocomire.IsQueuedForDeletion() ? crocomire.WallRumble : 0;
        room.Terrain.ShownShift = _rest + new Vector2I(0, -rumble);
        return false;
    }
}
