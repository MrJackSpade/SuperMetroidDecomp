using Engine.Actors;
using Engine.Actors.Components;
using Engine.Content;
using Engine.Input;
using Engine.Shared;
using Godot;

namespace Engine.World.Objects;

/// <summary>
/// Something that takes hold of the player and moves them along a path, then lets go. An elevator is a carrier whose
/// path ends at a room exit: the player is let go inside the exit, and the room transition follows. A player who
/// arrives in the room where its path ends is brought back along it to where it rests.
/// </summary>
/// <remarks>Its data may name the timelines that show it: "rest", and "moving" while it carries.</remarks>
public partial class Carrier : RoomObject
{
    /// <summary>The points of the path, in pixels from where the carrier stands, visited in order. The carrier starts at the first.</summary>
    [Export]
    public Godot.Collections.Array<Vector2I> Path { get; set; } = [];

    /// <summary>How far the carrier moves each update, as a number of pixels.</summary>
    [Export]
    public double Speed { get; set; } = 1;

    /// <summary>How much faster it goes each update it is carrying, as a number of pixels; zero for one that keeps its speed.</summary>
    [Export]
    public double SpeedGain { get; set; }

    /// <summary>The speed it gains no more beyond; zero for no limit.</summary>
    [Export]
    public double TopSpeed { get; set; }

    /// <summary>The width and height of the region in which it takes hold of the player, in pixels; its top-left is where the carrier stands.</summary>
    [Export]
    public Vector2I Size { get; set; } = new(32, 16);

    /// <summary>Where the player is held, in pixels from where the carrier is.</summary>
    [Export]
    public Vector2I HoldsAt { get; set; }

    /// <summary>The action whose press starts it while the player stands in its region; empty when standing there is enough.</summary>
    [Export]
    public string StartsOn { get; set; } = "";

    /// <summary>Whether it brings a player who arrives where its path ends back to where it rests.</summary>
    [Export]
    public bool BringsArrivals { get; set; } = true;

    /// <summary>The sound of it setting off, by name; empty for none.</summary>
    [Export]
    public string DepartureSound { get; set; } = "";

    /// <summary>The sound of it coming to rest, by name; empty for none.</summary>
    [Export]
    public string ArrivalSound { get; set; } = "";

    private PixelPosition _home;
    private PathFollower? _path;
    private bool _mustLeave;
    private bool _arriving;
    private bool _lookedForArrival;
    private TimelineSet _timelines = TimelineSet.None;
    private TimelinePlayer? _showing;
    private string _showingName = "";

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _timelines = TimelineSet.ReadFrom(data);
    }

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
        (_path, _mustLeave, _arriving, _lookedForArrival) = (null, false, false, false);
        (_showing, _showingName) = (null, "");
        ShowLook();
    }

    /// <summary>Shows the timeline for what it is doing, where its data has one, and moves it on an update.</summary>
    private void ShowLook()
    {
        string name = _path is not null && _timelines.Has("moving") ? "moving" : "rest";
        if (!_timelines.Has(name))
            return;
        if (_showing is null || _showingName != name)
            (_showing, _showingName) = (new TimelinePlayer(_timelines.Get(name)), name);
        else
            _showing.Step();
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Show(_showing.Current);
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        Player player = Room.Player;
        ShowLook();
        if (!_lookedForArrival)
        {
            _lookedForArrival = true;
            if (BringsArrivals && TakeArrival(player))
                return;
        }

        if (_path is not null)
        {
            _path.Step();
            if (SpeedGain > 0)
            {
                Pixels faster = _path.Speed + Pixels.FromNumber(SpeedGain);
                if (TopSpeed <= 0 || faster <= Pixels.FromNumber(TopSpeed))
                    _path.Speed = faster;
            }

            Position = _path.Position;
            player.Position = Position + PixelOffset.FromWhole(HoldsAt.X, HoldsAt.Y);
            if (!_path.IsFinished)
                return;
            _path = null;
            _mustLeave = true;
            if (_arriving)
                Sound(ArrivalSound);
            _arriving = false;
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
        if (StartsOn.Length > 0)
        {
            // Having stood on it and done something else, the player must step off before it will listen again.
            if (!player.Input.WasPressed(new GameAction(StartsOn)))
                return;
        }

        _path = new PathFollower(Points(), Pixels.FromNumber(Speed), PathFollower.AtEnd.Stops);
        Sound(DepartureSound);
        player.Carry();
    }

    /// <summary>
    /// Takes hold of a player who has arrived where the path ends, to bring them back along it. Returns whether it
    /// did.
    /// </summary>
    private bool TakeArrival(Player player)
    {
        List<PixelPosition> points = Points();
        PixelPosition end = points[^1];
        var endArea = new PixelRect(end.X.Whole, end.Y.Whole, Size.X, Size.Y);
        if (!endArea.Overlaps(player.BodyRect))
            return false;
        points.Reverse();
        _path = new PathFollower(points, Pixels.FromNumber(Speed), PathFollower.AtEnd.Stops);
        _arriving = true;
        Position = end;
        player.Position = Position + PixelOffset.FromWhole(HoldsAt.X, HoldsAt.Y);
        player.Carry();
        return true;
    }

    /// <summary>The points of its path, in the room.</summary>
    private List<PixelPosition> Points() => [.. Path.Select(point => _home + PixelOffset.FromWhole(point.X, point.Y))];

    private void Sound(string sound)
    {
        if (sound.Length > 0)
            Room.Session.Sounds.Request(sound, channel: "objects");
    }
}
