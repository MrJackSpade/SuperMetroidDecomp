using Engine.Actors.Components;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Engine.Actors;

/// <summary>
/// A temporary drop lying in a room. It gives itself to the player when they touch it, and is gone when its time runs
/// out. It records nothing in progress; a permanent upgrade is an item, not a pickup.
/// </summary>
public partial class Pickup : Node2D, IRoomEntity
{
    private PickupType? _type;
    private Room? _room;
    private Countdown _left;
    private TimelinePlayer? _timeline;
    private ActorVisual? _visual;

    /// <summary>A pickup of <paramref name="type"/> lying centred on <paramref name="at"/>.</summary>
    public static Pickup Of(PickupType type, PixelPosition at)
    {
        ArgumentNullException.ThrowIfNull(type);
        return new Pickup { _type = type, Position = at, Name = "Pickup" };
    }

    /// <summary>The pickup's type.</summary>
    public PickupType Type => _type ?? throw new InvalidOperationException("The pickup has no type.");

    /// <inheritdoc/>
    public new PixelPosition Position { get; set; }

    /// <summary>The pixels the pickup covers, in the room.</summary>
    public PixelRect Area => PixelRect.Around(Position.X.Whole, Position.Y.Whole, Type.Size / 2, Type.Size / 2);

    /// <inheritdoc/>
    public void EnterRoom(Room room)
    {
        _room = room;
        _left = Countdown.Of(Type.LastsUpdates);
        string picture = Type.Picture;
        if (Type.Timeline.Length > 0)
        {
            _timeline = new TimelinePlayer(FrameTimeline.Read(room.Content.ReadData(Type.Timeline)));
            picture = _timeline.Current.Picture;
        }

        if (picture.Length == 0 && _timeline is null)
            return;
        _visual = new ActorVisual { Picture = picture, PictureOffset = new Vector2I(-Type.Size / 2, -Type.Size / 2) };
        AddChild(_visual);
        _visual.Bind(room.Content);
    }

    /// <summary>Gives the pickup to the player now, as something that reaches out for it does.</summary>
    public void Collect()
    {
        Room room = _room ?? throw new InvalidOperationException("The pickup is not in a room.");
        if (room.IsLeaving(this))
            return;
        Type.GiveTo(room.Player.Inventory);
        if (Type.Sound.Length > 0)
            room.Session.Sounds.Request(Type.Sound, channel: "pickups");
        room.Remove(this);
    }

    /// <summary>Plays one update: the pickup is collected if the player touches it, and otherwise ages.</summary>
    internal void Step()
    {
        Room room = _room ?? throw new InvalidOperationException("The pickup is not in a room.");
        if (Area.Overlaps(room.Player.BodyRect))
        {
            Collect();
            return;
        }

        if (_timeline is not null)
        {
            _timeline.Step();
            _visual?.Show(_timeline.Current.Picture);
        }

        _left = _left.Stepped();
        if (_left.IsOver)
            room.Remove(this);
    }

    /// <inheritdoc/>
    public void ShowPosition() => EntityNode.Show(this, Position);
}
