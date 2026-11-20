using Engine.Actors.Components;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Engine.Actors;

/// <summary>
/// A short-lived visual: an explosion, dust, a splash. It plays its timeline and is gone. It does not collide, deal
/// damage, change anything in play or draw on the gameplay random generator; if it did any of those it would be a
/// projectile or an enemy.
/// </summary>
public partial class Effect : Node2D, IRoomEntity
{
    private FrameTimeline? _timeline;
    private TimelinePlayer? _player;
    private ActorVisual? _visual;
    private Room? _room;

    /// <summary>The effect's velocity, for one that drifts or falls.</summary>
    public PixelVelocity Velocity { get; set; }

    /// <summary>Where each picture's top-left corner lies from the effect's place, in pixels.</summary>
    public Vector2I PictureOffset { get; set; }

    /// <summary>An effect playing <paramref name="timeline"/> once at <paramref name="at"/>.</summary>
    public static Effect Of(FrameTimeline timeline, PixelPosition at)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        return new Effect { _timeline = timeline, Position = at, Name = "Effect" };
    }

    /// <inheritdoc/>
    public new PixelPosition Position { get; set; }

    /// <inheritdoc/>
    public void EnterRoom(Room room)
    {
        _room = room;
        FrameTimeline timeline = _timeline ?? throw new InvalidOperationException("The effect has no timeline.");
        _player = new TimelinePlayer(timeline);
        _visual = new ActorVisual { PictureOffset = PictureOffset };
        AddChild(_visual);
        _visual.Bind(room.Content);
        _visual.Show(_player.Current.Picture);
    }

    /// <summary>Plays one update of the effect's timeline; when a timeline that does not loop ends, the effect is gone.</summary>
    internal void Step()
    {
        Room room = _room ?? throw new InvalidOperationException("The effect is not in a room.");
        TimelinePlayer player = _player ?? throw new InvalidOperationException("The effect is not in a room.");
        Position += Velocity.OverOneUpdate;
        player.Step();
        if (player.IsOver)
        {
            room.Remove(this);
            return;
        }

        _visual?.Show(player.Current.Picture);
    }

    /// <inheritdoc/>
    public void ShowPosition() => EntityNode.Show(this, Position);
}
