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
        if (Type.Picture.Length > 0)
        {
            var visual = new ActorVisual { Picture = Type.Picture, PictureOffset = new Vector2I(-Type.Size / 2, -Type.Size / 2) };
            AddChild(visual);
            visual.Bind(room.Content);
        }
    }

    /// <summary>Plays one update: the pickup is collected if the player touches it, and otherwise ages.</summary>
    internal void Step()
    {
        Room room = _room ?? throw new InvalidOperationException("The pickup is not in a room.");
        if (Area.Overlaps(room.Player.BodyRect))
        {
            room.Player.Inventory.Add(Type.AddsTo, Type.Quantity);
            room.Remove(this);
            return;
        }

        _left = _left.Stepped();
        if (_left.IsOver)
            room.Remove(this);
    }

    /// <inheritdoc/>
    public void ShowPosition() => EntityNode.Show(this, Position);
}
