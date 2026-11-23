using Engine.Collision;
using Engine.Shared;
using Godot;

namespace Engine.World.Objects;

/// <summary>
/// A solid surface that moves along a path, and so carries what stands on it. The carrying is the mover's: a platform
/// is only a solid that moves. It can be told to "start" and "stop".
/// </summary>
public partial class Platform : RoomObject, ITriggerTarget
{
    /// <summary>The points of the path, in pixels from where the platform stands, visited in order. It starts at the first.</summary>
    [Export]
    public Godot.Collections.Array<Vector2I> Path { get; set; } = [];

    /// <summary>How far the platform moves each update, as a number of pixels.</summary>
    [Export]
    public double Speed { get; set; } = 1;

    /// <summary>What the platform does at the end of its path.</summary>
    [Export]
    public PathFollower.AtEnd AtEnd { get; set; } = PathFollower.AtEnd.Returns;

    /// <summary>Whether the platform is moving when its room is set up.</summary>
    [Export]
    public bool StartsMoving { get; set; } = true;

    private PathFollower? _path;
    private Collider? _solid;
    private bool _moving;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        if (Path.Count < 2)
            throw new InvalidOperationException($"The platform \"{Name}\" needs a path of at least two points.");
        _solid = EntityNode.ColliderOf(this, ColliderRole.Solid)
            ?? throw new InvalidOperationException($"The platform \"{Name}\" has no solid collider.");
        var points = Path.Select(point => Position + PixelOffset.FromWhole(point.X, point.Y)).ToList();
        _path = new PathFollower(points, Pixels.FromNumber(Speed), AtEnd);
        _moving = StartsMoving;
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        if (!_moving || _path is null || _solid is null)
            return;
        PixelOffset moved = _path.Step();
        if (moved != PixelOffset.Zero)
            Room.Mover.MoveSolid(_solid, moved);
    }

    /// <inheritdoc/>
    public void Triggered(string what)
    {
        _moving = what switch
        {
            "start" => true,
            "stop" => false,
            _ => throw new ArgumentException($"A platform can be told to \"start\" or \"stop\", not \"{what}\".", nameof(what)),
        };
    }
}
