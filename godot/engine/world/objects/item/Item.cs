using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Shared;
using Godot;

namespace Engine.World.Objects;

/// <summary>
/// A permanent upgrade lying in a room. It grants its item type once, when the player touches it
/// (<see cref="Player.Reaches"/>: by their body, or by a move of it tried into the item that something beside the item
/// stopped short), records that in progress, and asks for its message. An item is just an item: one held in a shell has a barrier around it, and one
/// hidden in the wall is behind a block, and in both cases it cannot be seen or touched until what covers it is gone.
/// While it lies there the block it lies in is a wall to what a way out of the room is a wall to
/// (<see cref="CollisionWorld.HoldBlock"/>): on the cartridge an item's block is solid to every enemy, and only the
/// player passes into it. It is so until the player takes the item.
/// <para>
/// Its pickup ends as its message closes, on the update it was touched: until then it is shown where it lies. Then
/// it tells what it has to tell, shows nothing for one update, and leaves the room on the next, keeping its place
/// in the room until it does.
/// </para>
/// </summary>
public partial class Item : RoomObject, IFinishesAfterMessages
{
    /// <summary>How far its pickup has gone.</summary>
    private enum Stage
    {
        Lying,
        AwaitingMessage,
        Taken,
    }

    private Stage _stage;

    /// <summary>Something of the room told as its pickup ends; nothing when it names nothing.</summary>
    [Export]
    public NodePath Tells { get; set; } = new();

    /// <summary>What <see cref="Tells"/> is told.</summary>
    [Export]
    public string TellsWhat { get; set; } = "";

    /// <summary>The data file of its item type.</summary>
    [Export(PropertyHint.File)]
    public string TypeFile { get; set; } = "";

    /// <summary>Its width and height, in pixels; it is centred on where it stands.</summary>
    [Export]
    public Vector2I Size { get; set; } = new(16, 16);

    private ItemType? _type;
    private TimelinePlayer? _timeline;
    private bool _wasCovered;

    /// <summary>The block it holds while it lies in the room; none once it is taken.</summary>
    private PixelRect? _held;

    /// <summary>The item's type.</summary>
    public ItemType Type => _type ?? throw new InvalidOperationException($"The item \"{Name}\" is not in a room.");

    /// <summary>The pixels the item covers, in the room.</summary>
    public PixelRect Area => PixelRect.Around(Position.X.Whole, Position.Y.Whole, Size.X / 2, Size.Y / 2);

    /// <summary>
    /// Whether something covers the item, so that it can be neither seen nor touched: terrain, or a barrier that
    /// stands over it and has not finished opening, however passable the barrier is as it opens.
    /// </summary>
    public bool IsCovered => Room.Terrain.IsSolidIn(Area, SlopeTreatment.Wall)
        || Room.Objects.OfType<Barrier>().Any(barrier => !barrier.HasOpened && barrier.Covers(Area));

    /// <inheritdoc/>
    public override bool MustBeRemembered => true;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        if (TypeFile.Length == 0)
            throw new InvalidOperationException($"The item \"{Name}\" names no item type.");
        if (RememberedAs.Length == 0)
            throw new InvalidOperationException($"The item \"{Name}\" has no name to be remembered by; every item needs one.");
        _type = ItemType.Read(Room.Content.ReadData(TypeFile));
        if (IsRemembered)
        {
            Room.MarkGone(Name);
            Room.Remove(this);
            return;
        }

        if ((_type.Picture.Length > 0 || _type.Timeline.Length > 0) && !GetChildren().OfType<ActorVisual>().Any())
        {
            var visual = new ActorVisual { Picture = _type.Picture, PictureOffset = new Vector2I(-Size.X / 2, -Size.Y / 2) };
            AddChild(visual);
            visual.Bind(Room.Content);
        }

        _timeline = _type.Timeline.Length > 0 ? new TimelinePlayer(FrameTimeline.Read(Room.Content.ReadData(_type.Timeline))) : null;
        _held = Area;
        Room.Collision.HoldBlock(Area);
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        // The update after its pickup ended it leaves, and gives up its place in the room with that.
        if (_stage == Stage.Taken)
        {
            Room.Remove(this);
            return;
        }

        if (_stage == Stage.AwaitingMessage)
            return;
        bool covered = IsCovered;

        // Uncovered, it starts its looks again from the first, which shows on that update.
        if (_timeline is not null && _wasCovered && !covered)
            _timeline.Play(_timeline.Timeline);
        else
            _timeline?.Step();
        _wasCovered = covered;
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
        {
            visual.SetShown(!covered);
            if (_timeline is not null)
                visual.Show(_timeline.Shown);
        }

        if (covered || !Room.Player.Reaches(Area))
            return;

        Inventory inventory = Room.Player.Inventory;
        if (Type.GrantsEquipment.Length > 0)
            inventory.Grant(Type.GrantsEquipment);
        foreach (string equipment in Type.SwitchesOff)
        {
            if (inventory.Owns(equipment))
                inventory.Switch(equipment, false);
        }

        if (Type.Expands.Length > 0)
        {
            inventory.SetCapacity(Type.Expands, inventory.Capacity(Type.Expands) + Type.ExpandsBy);
            if (Type.Fills)
                inventory.Fill(Type.Expands);
            else
                inventory.Add(Type.Expands, Type.Adds);
        }

        Remember();
        Room.MarkGone(Name);
        if (_held is { } block)
        {
            Room.Collision.LetBlockGo(block);
            _held = null;
        }

        if (Type.Sound.Length > 0)
            Room.Session.Sounds.Request(Type.Sound, channel: "objects");
        if (Type.Message.Length > 0)
            Room.Session.RequestMessage(Type.Message, Type.Fanfare, Type.MessageHoldUpdates);
        Room.Player.OnItemCollected();
        _stage = Stage.AwaitingMessage;
        Room.FinishAfterMessages(this);
    }

    /// <inheritdoc/>
    public void FinishAfterMessages()
    {
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.SetShown(false);
        Room.Player.OnItemPickupEnded();
        if (!Tells.IsEmpty)
        {
            (GetNode(Tells) as ITriggerTarget
                ?? throw new InvalidOperationException($"The item \"{Name}\" is to tell \"{Tells}\", which is not something that can be told."))
                .Triggered(TellsWhat);
        }

        _stage = Stage.Taken;
    }
}
