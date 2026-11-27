using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>
/// Keeps something launched or left drawn where the timeline step it shows lies against the room's tiles
/// (<see cref="TimelineStep.Lies"/>), and back at its own depth when it shows a step that says nothing. The thing
/// shows each step through this as it shows the step's picture.
/// </summary>
public sealed class LaidDepth
{
    private int _own;
    private bool _laid;

    /// <summary>Draws <paramref name="node"/> where a step that lies so is drawn.</summary>
    public void Show(Node2D node, LiesAgainstTiles lies)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (lies == LiesAgainstTiles.WhereItStands)
        {
            if (_laid)
                (node.ZIndex, _laid) = (_own, false);
            return;
        }

        if (!_laid)
            (_own, _laid) = (node.ZIndex, true);
        node.ZIndex = lies == LiesAgainstTiles.InFrontOfTiles ? RoomDepth.InFrontOfRoom : RoomDepth.BehindRoom;
    }
}
