using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Rooms;

/// <summary>
/// What a room's ground is, for a room whose ground raises something where Samus lands or runs: wet or dusty. It does
/// nothing by itself; Samus finds it among her room's children and asks it. A room without one has plain ground, which
/// raises nothing. Ground that is wet only at times says what makes it so: the Landing Site's while its rain shows, the
/// West Ocean's at and below its waterline.
/// </summary>
public partial class Ground : Node, IPlaced
{
    /// <summary>What the ground is, while it is so at all.</summary>
    [Export]
    public GroundKind Kind { get; set; }

    /// <summary>The room effect the ground is so only while it is in the room and showing, as rain wets it; none for always.</summary>
    [Export]
    public NodePath WhileShowing { get; set; } = new();

    /// <summary>The height in the room at and below which Samus's middle must lie for the ground to be so; 0 for everywhere.</summary>
    [Export]
    public int FromHeight { get; set; }

    /// <inheritdoc/>
    [Export]
    public Condition? PresentWhen { get; set; }

    /// <summary>What the ground is under one whose middle is at <paramref name="middle"/>: its kind, or none where it is plain.</summary>
    public GroundKind? Under(PixelPosition middle)
    {
        if (!WhileShowing.IsEmpty && GetNodeOrNull<RoomEffect>(WhileShowing) is not { IsActive: true })
            return null;
        return middle.Y.Whole >= FromHeight ? Kind : null;
    }
}
