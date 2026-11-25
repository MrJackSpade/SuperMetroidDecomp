using Engine.Actors.Components;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Engine.Actors;

/// <summary>
/// What a destroyed enemy leaves where it died. The timeline of its death plays; what the enemy drops appears when
/// the timeline says, or as it ends; and the enemy that takes its place, if one does, comes once the drop is gone.
/// Unlike an effect it is part of play: it decides the drop, and it places what it scatters by the gameplay random
/// generator.
/// </summary>
/// <remarks>
/// Its timeline's cues: "sound:" and a sound's name asks for that sound. "drop" leaves the drop, after which the
/// remains show nothing. "scatter:", the data file of an effect's timeline, ":" and a reach in pixels leaves that
/// effect near by, as far as the reach to either side and above or below. One number is drawn for each: its lowest
/// digits settle how far across and its second eight how far down. The reach is a power of two.
/// <code>
/// { "steps": [ { "updates": 8, "cues": ["scatter:effects/small_explosion.json:16", "sound:enemy_small_explosion"] },
///              { "updates": 1, "cues": ["drop"] } ] }
/// </code>
/// </remarks>
public partial class Remains : Node2D, IRoomEntity
{
    private FrameTimeline? _timeline;
    private TimelinePlayer? _player;
    private ActorVisual? _visual;
    private Room? _room;
    private DropTable _drops = DropTable.Nothing;
    private Node2D? _replacement;
    private int _replacementUpdates;
    private bool _dropped;

    /// <summary>
    /// The remains of something destroyed at <paramref name="at"/>, whose death plays <paramref name="timeline"/> and
    /// which drops from <paramref name="drops"/>.
    /// </summary>
    public static Remains Of(FrameTimeline timeline, PixelPosition at, DropTable drops)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(drops);
        return new Remains { _timeline = timeline, _drops = drops, Position = at, Name = "Remains" };
    }

    /// <summary>
    /// Gives the remains what takes the destroyed thing's place: it joins the room <paramref name="updates"/> updates
    /// after the drop is gone, or after the death has played when nothing is dropped.
    /// </summary>
    public void ReplaceWith(Node2D replacement, int updates)
    {
        ArgumentNullException.ThrowIfNull(replacement);
        ArgumentOutOfRangeException.ThrowIfNegative(updates);
        _replacement = replacement;
        _replacementUpdates = updates;
    }

    /// <inheritdoc/>
    public new PixelPosition Position { get; set; }

    /// <inheritdoc/>
    public void EnterRoom(Room room)
    {
        _room = room;
        FrameTimeline timeline = _timeline ?? throw new InvalidOperationException("The remains have no timeline.");
        _player = new TimelinePlayer(timeline);
        _visual = new ActorVisual();
        AddChild(_visual);
        _visual.Bind(room.Content);
        _visual.Show(_player.Current);
    }

    /// <summary>Plays one update of the death; as it ends, the drop appears and the remains are gone.</summary>
    internal void Step()
    {
        Room room = _room ?? throw new InvalidOperationException("The remains are not in a room.");
        TimelinePlayer player = _player ?? throw new InvalidOperationException("The remains are not in a room.");
        foreach (string cue in player.Step())
        {
            if (cue.StartsWith("sound:", StringComparison.Ordinal))
                room.Session.Sounds.Request(cue["sound:".Length..], channel: "enemies");
            else if (cue == "drop")
                LeaveDrop(room);
            else if (cue.StartsWith("scatter:", StringComparison.Ordinal))
                Scatter(room, cue["scatter:".Length..]);
            else
                throw new InvalidOperationException($"The timeline of a death gives the cue \"{cue}\", which a death does not know.");
        }

        if (!player.IsOver)
        {
            if (!_dropped)
                _visual?.Show(player.Shown);
            return;
        }

        LeaveDrop(room);
        if (!room.IsLeaving(this))
            room.Remove(this);
    }

    /// <summary>Leaves the drop, once, and hands what takes the destroyed thing's place to the room.</summary>
    private void LeaveDrop(Room room)
    {
        if (_dropped)
            return;
        _dropped = true;
        _visual?.Show("");

        // The drop is rolled wherever the remains lie, and the pickup takes the place they held. Remains in a place
        // the game says leaves no drop have rolled for nothing.
        Inventory inventory = room.Player.Inventory;
        string? drop = _drops.Roll(room.Session.Random, inventory, file => PickupType.Read(room.Content.ReadData(file)).IsUsefulTo(inventory));
        Pickup? pickup = null;
        if (drop is not null && (room.PlaceOf(this) is not { } place || room.Session.Definition.RemainsLeaveDrop(place)))
        {
            pickup = Pickup.Of(PickupType.Read(room.Content.ReadData(drop)), Position);
            room.SpawnInPlaceOf(this, pickup);
        }
        else
        {
            // Remains that leave nothing go on holding their place for the pause of a spent drop.
            room.HoldPlaceOf(this, Position);
        }

        if (_replacement is { } replacement)
        {
            _replacement = null;
            room.SpawnLater(replacement, pickup is null ? [] : [pickup], _replacementUpdates);
        }
    }

    /// <summary>Leaves an effect near by, as a "scatter:" cue asks.</summary>
    private void Scatter(Room room, string asked)
    {
        int split = asked.LastIndexOf(':');
        if (split <= 0 || !int.TryParse(asked[(split + 1)..], out int reach) || reach < 1 || !int.IsPow2(reach))
            throw new InvalidOperationException($"The cue \"scatter:{asked}\" does not name an effect and a reach that is a power of two.");
        // Twice the reach is the width of the span it may land in; the digits that count it are one more than the reach's.
        int digits = int.Log2(reach) + 1;
        if (digits > 8)
            throw new InvalidOperationException($"The cue \"scatter:{asked}\" reaches further than one draw settles.");
        RandomDraw draw = room.Session.Random.Draw();
        PixelPosition at = Position + PixelOffset.FromWhole(draw.Part(0, digits) - reach, draw.Part(8, digits) - reach);
        room.Spawn(Effect.Of(FrameTimeline.Read(room.Content.ReadData(asked[..split])), at));
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        // What was to take the destroyed thing's place goes with the remains when the room is left first.
        if (_replacement is { } unused && IsInstanceValid(unused))
            unused.Free();
        _replacement = null;
    }

    /// <inheritdoc/>
    public void ShowPosition() => EntityNode.Show(this, Position);
}
