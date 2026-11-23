using Engine.World;
using Godot;

namespace Game.Sequences;

/// <summary>
/// The view in the Dachora's shaft: once the top of the shaft has been opened to the view, the part of the room
/// beside it is closed to it, so the view settles on the shaft and stays there.
/// </summary>
public partial class DachoraShaftCamera : RoomSequence
{
    /// <summary>The camera zone over the top of the shaft.</summary>
    [Export]
    public NodePath ShaftTop { get; set; } = new();

    /// <summary>The camera zone beside the shaft, closed once the shaft's is open.</summary>
    [Export]
    public NodePath Beside { get; set; } = new();

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        CameraZone top = Zone(ShaftTop), beside = Zone(Beside);
        if (!top.IsOpen)
            return false;
        if (beside.IsOpen)
            beside.Triggered("close");
        return true;
    }

    private CameraZone Zone(NodePath path) => GetNode(path) as CameraZone
        ?? throw new InvalidOperationException($"The shaft's view \"{Name}\" is given \"{path}\", which is no camera zone.");
}
