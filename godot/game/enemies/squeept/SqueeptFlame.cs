using Engine.Actors.Components;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// The flame a Squeept trails as it rises (<see cref="Squeept"/>), a thing of the room in its own right as the
/// cartridge has it: placed right after its Squeept, it takes its turn among the enemies after it, and only within
/// the window enemies take their turns in, by where the flame itself last stood. On a turn it draws a random number
/// nothing uses and then goes to its Squeept: unseen while the Squeept is frozen or not rising, and otherwise shown
/// as far down the room as the Squeept is. Out of the window it stays where it was and draws nothing, though its
/// Squeept leaps on. Nothing hits it and it touches nothing.
/// </summary>
/// <remarks>
/// It is never frozen itself. It has no data of its own: its pictures are its Squeept's, the timeline "flame" of
/// its data file, drawn at the offset its own are.
/// </remarks>
public partial class SqueeptFlame : RoomObject
{
    /// <summary>The Squeept it is the flame of.</summary>
    [Export]
    public NodePath Leader { get; set; } = new();

    private Squeept? _squeept;
    private TimelinePlayer? _playing;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _squeept = GetNodeOrNull<Squeept>(Leader)
            ?? throw new InvalidOperationException($"The flame \"{Name}\" names no Squeept it is the flame of.");

        // It is a second of its Squeept's kind in the room, and as wide.
        TurnHalfWidth = _squeept.BodyHalfWidth;
        _playing = new TimelinePlayer(_squeept.Timelines.Get("flame"));
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
        {
            visual.PictureOffset = _squeept.PictureOffset;
            visual.SetShown(false);
        }
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        Squeept squeept = _squeept ?? throw new InvalidOperationException($"The flame \"{Name}\" is in no room.");
        TimelinePlayer playing = _playing ?? throw new InvalidOperationException($"The flame \"{Name}\" is in no room.");
        Room.Session.Random.Next();
        if (squeept.IsDead)
        {
            Room.Remove(this);
            return;
        }

        playing.Step();
        bool shown = !squeept.IsFrozen && squeept.IsRising;
        if (shown)
            Position = new PixelPosition(Position.X, Position.Y.WithWhole(squeept.Position.Y.Whole));
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
        {
            visual.SetShown(shown);
            visual.Show(playing.Shown);
        }
    }
}
