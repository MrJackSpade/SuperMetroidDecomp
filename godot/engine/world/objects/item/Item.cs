using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Shared;
using Godot;

namespace Engine.World.Objects;

/// <summary>
/// A permanent upgrade lying in a room. It grants its item type once, when the player touches it, records that in
/// progress, and asks for its message. An item is just an item: one held in a shell has a barrier around it, and one
/// hidden in the wall is behind a block, and in both cases it cannot be seen or touched until what covers it is gone.
/// </summary>
public partial class Item : RoomObject
{
    /// <summary>The data file of its item type.</summary>
    [Export(PropertyHint.File)]
    public string TypeFile { get; set; } = "";

    /// <summary>Its width and height, in pixels; it is centred on where it stands.</summary>
    [Export]
    public Vector2I Size { get; set; } = new(16, 16);

    private ItemType? _type;

    /// <summary>The item's type.</summary>
    public ItemType Type => _type ?? throw new InvalidOperationException($"The item \"{Name}\" is not in a room.");

    /// <summary>The pixels the item covers, in the room.</summary>
    public PixelRect Area => PixelRect.Around(Position.X.Whole, Position.Y.Whole, Size.X / 2, Size.Y / 2);

    /// <summary>Whether terrain covers the item, so that it can be neither seen nor touched.</summary>
    public bool IsCovered => Room.Terrain.IsSolidIn(Area, SlopeTreatment.Wall);

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

        if (_type.Picture.Length > 0 && !GetChildren().OfType<ActorVisual>().Any())
        {
            var visual = new ActorVisual { Picture = _type.Picture, PictureOffset = new Vector2I(-Size.X / 2, -Size.Y / 2) };
            AddChild(visual);
            visual.Bind(Room.Content);
        }
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        bool covered = IsCovered;
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.SetShown(!covered);
        if (covered || !Area.Overlaps(Room.Player.BodyRect))
            return;

        Inventory inventory = Room.Player.Inventory;
        if (Type.GrantsEquipment.Length > 0)
            inventory.Grant(Type.GrantsEquipment);
        if (Type.Expands.Length > 0)
            inventory.Expand(Type.Expands, Type.ExpandsBy);
        Remember();
        Room.MarkGone(Name);
        if (Type.Message.Length > 0)
            Room.Session.RequestMessage(Type.Message);
        Room.Remove(this);
    }
}
