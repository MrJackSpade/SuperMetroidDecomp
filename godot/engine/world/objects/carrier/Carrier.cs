using Engine.Actors;
using Engine.Shared;
using Godot;

namespace Engine.World.Objects;

/// <summary>
/// Something that takes hold of the player and moves them along a path, then lets go. An elevator is a carrier whose
/// path ends at a room exit: the player is let go inside the exit, and the room transition follows.
/// </summary>
public partial class Carrier : RoomObject
{
    /// <summary>The points of the path, in pixels from where the carrier stands, visited in order. The carrier starts at the first.</summary>
    [Export]
    public Godot.Collections.Array<Vector2I> Path { get; set; } = [];

    /// <summary>How far the carrier moves each update, as a number of pixels.</summary>
    [Export]
    public double Speed { get; set; } = 1;

    /// <summary>The width and height of the region in which it takes hold of the player, in pixels; its top-left is where the carrier stands.</summary>
    [Export]
    public Vector2I Size { get; set; } = new(32, 16);

    /// <summary>Where the player is held, in pixels from where the carrier is.</summary>
    [Export]
    public Vector2I HoldsAt { get; set; }

    private PixelPosition _home;
    private PathFollower? _path;
    private bool _mustLeave;

    /// <summary>Whether the carrier is carrying the player.</summary>
    public bool IsCarrying => _path is not null;

    /// <summary>The region in which it takes hold of the player.</summary>
    public PixelRect Area => new(Position.X.Whole, Position.Y.Whole, Size.X, Size.Y);

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        if (Path.Count < 2)
            throw new InvalidOperationException($"The carrier \"{Name}\" needs a path of at least two points.");
        _home = Position;
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        Player player = Room.Player;
        if (_path is not null)
        {
            _path.Step();
            Position = _path.Position;
            player.Position = Position + PixelOffset.FromWhole(HoldsAt.X, HoldsAt.Y);
            if (!_path.IsFinished)
                return;
            _path = null;
            _mustLeave = true;
            player.Release();
            return;
        }

        bool inside = Area.Overlaps(player.BodyRect);
        if (!inside)
        {
            _mustLeave = false;
            return;
        }

        if (_mustLeave || !player.State.InControl || !player.IsOnGround)
            return;
        var points = Path.Select(point => _home + PixelOffset.FromWhole(point.X, point.Y)).ToList();
        _path = new PathFollower(points, Pixels.FromNumber(Speed), PathFollower.AtEnd.Stops);
        player.Carry();
    }
}
