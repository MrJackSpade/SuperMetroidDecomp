using Engine.Actors.Components;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Engine.Actors;

/// <summary>
/// A temporary drop lying in a room. It gives itself to the player when they touch it, and is gone when its time runs
/// out; taken or run out, the place it held is held a pause longer (<see cref="Room.HoldPlaceOf"/>). It records nothing in progress; a permanent upgrade is an item, not a pickup.
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

    /// <summary>How many more updates the pickup lies before it is gone.</summary>
    public int UpdatesLeft => _left.Remaining;

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

        // A pickup lies in front of every tile of the room, the ones in front of the room's things too: nothing of
        // the room hides what is there to be taken.
        ZIndex = RoomDepth.InFrontOfRoom;
        _left = Countdown.Of(Type.LastsUpdates);
        string picture = Type.Picture;
        if (Type.Timeline.Length > 0)
        {
            _timeline = new TimelinePlayer(FrameTimeline.Read(room.Content.ReadData(Type.Timeline)));
            picture = _timeline.Current.Picture;
        }

        if (picture.Length == 0 && _timeline is null)
            return;
        // A lone picture is centred on the pickup; a timeline's pictures lie where the timeline puts them.
        _visual = new ActorVisual { Picture = picture };
        if (_timeline is null)
            _visual.PictureOffset = new Vector2I(-Type.Size / 2, -Type.Size / 2);
        AddChild(_visual);
        _visual.Bind(room.Content);
        if (_timeline is not null)
            _visual.Show(_timeline.Current);
    }

    /// <summary>
    /// Rolls <paramref name="table"/> for the player of <paramref name="room"/> and leaves what it gives at
    /// <paramref name="at"/>. Returns the pickup left, if any.
    /// </summary>
    public static Pickup? Dropped(Room room, DropTable table, PixelPosition at)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(table);

        // With no place free for it the drop is not made, and not rolled either.
        if (!room.HasFreePlace)
            return null;
        Inventory inventory = room.Player.Inventory;
        string? drop = table.Roll(room.Session.Random, inventory, file => PickupType.Read(room.Content.ReadData(file)).IsUsefulTo(inventory));
        if (drop is null)
        {
            // A drop that comes to nothing still holds the place found for it, for the pause of a spent drop.
            if (room.Session.Definition.SpentDropUpdates > 0)
                room.Spawn(Effect.HoldingPlace(at, room.Session.Definition.SpentDropUpdates));
            return null;
        }

        Pickup pickup = Of(PickupType.Read(room.Content.ReadData(drop)), at);
        room.Spawn(pickup);
        return pickup;
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
        room.HoldPlaceOf(this, Position);
    }

    /// <summary>
    /// Plays one update: the pickup ages first, and is gone on the update its time ends, touched or not; otherwise it
    /// is collected if the player touches it.
    /// </summary>
    internal void Step()
    {
        Room room = _room ?? throw new InvalidOperationException("The pickup is not in a room.");
        _left = _left.Stepped();
        if (_left.IsOver)
        {
            room.HoldPlaceOf(this, Position);
            return;
        }

        if (Area.Overlaps(room.Player.BodyRect))
        {
            Collect();
            return;
        }

        if (_timeline is not null)
        {
            _timeline.Step();
            _visual?.Show(_timeline.Shown);
        }
    }

    /// <inheritdoc/>
    public void ShowPosition() => EntityNode.Show(this, Position);
}
