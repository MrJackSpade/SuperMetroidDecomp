using Engine.Actors.Components;
using Engine.Shared;
using Godot;

namespace Engine.World.Objects;

/// <summary>
/// A fixture that reacts visibly to the player or to being told to, with no effect on play beyond an optional solid
/// body. It shows one timeline at rest and another while the player is near or once it is told to "react".
/// </summary>
public partial class Scenery : RoomObject, ITriggerTarget
{
    /// <summary>The data file of the timeline shown at rest; empty for none.</summary>
    [Export(PropertyHint.File)]
    public string RestTimeline { get; set; } = "";

    /// <summary>The data file of the timeline shown while reacting; empty for none.</summary>
    [Export(PropertyHint.File)]
    public string ReactTimeline { get; set; } = "";

    /// <summary>How near the player must come, in pixels, for it to react by itself; zero when it reacts only when told.</summary>
    [Export]
    public int ReactsWithin { get; set; }

    private TimelinePlayer? _player;
    private FrameTimeline? _rest;
    private FrameTimeline? _react;
    private bool _told;

    /// <summary>Whether it is reacting.</summary>
    public bool IsReacting { get; private set; }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _rest = RestTimeline.Length > 0 ? FrameTimeline.Read(Room.Content.ReadData(RestTimeline)) : null;
        _react = ReactTimeline.Length > 0 ? FrameTimeline.Read(Room.Content.ReadData(ReactTimeline)) : null;
        _told = false;
        IsReacting = false;
        _player = _rest is not null ? new TimelinePlayer(_rest) : null;
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        bool near = ReactsWithin > 0 &&
            (Room.Player.Position.X - Position.X).Abs() <= Pixels.FromWhole(ReactsWithin) &&
            (Room.Player.Position.Y - Position.Y).Abs() <= Pixels.FromWhole(ReactsWithin);
        bool reacting = _told || near;
        if (reacting != IsReacting)
        {
            IsReacting = reacting;
            FrameTimeline? timeline = reacting ? _react : _rest;
            _player = timeline is not null ? new TimelinePlayer(timeline) : null;
        }

        if (_player is null)
            return;
        _player.Step();
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Show(_player.Current);
    }

    /// <inheritdoc/>
    public void Triggered(string what)
    {
        _told = what switch
        {
            "react" => true,
            "rest" => false,
            _ => throw new ArgumentException($"Scenery can be told to \"react\" or \"rest\", not \"{what}\".", nameof(what)),
        };
    }
}
