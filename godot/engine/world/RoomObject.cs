using Engine.Actors.Components;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>
/// A placed, interactive fixture of a room that is not an enemy. It owns its place, its colliders, its own state and
/// its settings, and manages its own reaction to the player, to hits and to progress. It changes nothing else except
/// through that thing's own methods.
/// </summary>
/// <remarks>
/// Its data may give "picture_offset": where the top-left corner of its pictures lies from each of its visuals, as an
/// enemy's does.
/// <code>
/// "picture_offset": { "x": -8, "y": -8 }
/// </code>
/// </remarks>
public abstract partial class RoomObject : Node2D, IRoomEntity, IPlaced, IHasValues, IRemembered
{
    /// <inheritdoc/>
    public virtual bool MustBeRemembered => false;

    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <inheritdoc/>
    [Export]
    public Condition? PresentWhen { get; set; }

    /// <summary>
    /// The name this thing is remembered by in progress, when what happens to it must outlast the room; empty when
    /// nothing about it is remembered. Unique across the whole game.
    /// </summary>
    [Export]
    public string RememberedAs { get; set; } = "";

    private Room? _room;

    /// <inheritdoc/>
    public new PixelPosition Position { get; set; }

    /// <summary>The room it is in.</summary>
    public Room Room => _room ?? throw new InvalidOperationException($"The room object \"{Name}\" is not in a room.");

    /// <summary>Whether progress remembers this thing's fact.</summary>
    protected bool IsRemembered => RememberedAs.Length > 0 && Room.Session.Progress.Has(RememberedAs);

    /// <inheritdoc/>
    public void EnterRoom(Room room)
    {
        _room = room;
        Position = EntityNode.StartingPlace(this, Position);
        DataValues.Fill(this, room.Content);
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Bind(room.Content);
        OnEnterRoom();
    }

    /// <inheritdoc/>
    public virtual void ReadValues(DataNode data)
    {
        EntityNode.ReadColliders(this, data);
        if (!data.Has("picture_offset"))
            return;
        var offset = new Vector2I(data.Get("picture_offset").Int("x"), data.Get("picture_offset").Int("y"));
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.PictureOffset = offset;
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <summary>Called once as the object enters its room, after its values are loaded: it asks progress about itself and sets itself up.</summary>
    protected virtual void OnEnterRoom()
    {
    }

    /// <summary>Whether this one takes its turn among the enemies, as where it is placed says.</summary>
    [Export]
    public bool TurnsWithEnemies { get; set; }

    /// <summary>
    /// Whether, taking its turn among the enemies, it takes one only while its place lies within the window the
    /// room's enemies take their turns in (the view widened as the game's definition says, and to either side by
    /// <see cref="TurnHalfWidth"/>), as an enemy that is not always active does: outside it, it stands as it was.
    /// </summary>
    [Export]
    public bool TurnsOnlyInView { get; set; }

    /// <summary>For one that takes its turn only in view: how far to either side of its place the window is widened for it.</summary>
    [Export]
    public int TurnHalfWidth { get; set; }

    /// <summary>
    /// Whether it takes its turn among the enemies, in the order all joined the room: before the player moves, and
    /// held still with the enemies. Every other object takes its turn with the room's objects, after the player and
    /// the projectiles. A kind that always does says so itself; any other does where its placing says so.
    /// </summary>
    public virtual bool ActsWithEnemies => TurnsWithEnemies;

    /// <summary>Plays one update of the object.</summary>
    internal void Step() => OnStep();

    /// <summary>This object's own behaviour for one update.</summary>
    protected virtual void OnStep()
    {
    }

    /// <summary>Records this thing's fact in progress, if it is remembered.</summary>
    protected void Remember()
    {
        if (RememberedAs.Length > 0)
            Room.Session.Progress.Record(RememberedAs);
    }

    /// <inheritdoc/>
    public void ShowPosition() => EntityNode.Show(this, Position);
}
