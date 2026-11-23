using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>
/// A room-wide change of colour: a colour laid over the whole view that steps through a list, once or round and
/// round. Alarm lights, a room going dark and coming back, a flash. It can be told to "start" and "stop" like any
/// room effect; started, it begins its colours again.
/// </summary>
public partial class TintEffect : RoomEffect
{
    /// <summary>The colours it steps through; how solid each shows is the colour's own.</summary>
    [Export]
    public Color[] Colours { get; set; } = [];

    /// <summary>How many updates each colour shows.</summary>
    [Export]
    public int StepUpdates { get; set; } = 4;

    /// <summary>Whether it goes round again after the last colour; one that does not stays on the last.</summary>
    [Export]
    public bool Repeats { get; set; } = true;

    private int _step;
    private int _wait;
    private PixelRect _view;

    /// <summary>Whether it has reached its last colour and stays there.</summary>
    public bool IsDone => !Repeats && _step >= Colours.Length - 1;

    /// <inheritdoc/>
    protected override void OnSetup(Room room)
    {
        (_step, _wait) = (0, StepUpdates);
        ZIndex = 110;
    }

    /// <inheritdoc/>
    public override void Triggered(string what)
    {
        bool wasActive = IsActive;
        base.Triggered(what);
        if (IsActive && !wasActive)
            (_step, _wait) = (0, StepUpdates);
        QueueRedraw();
    }

    /// <inheritdoc/>
    protected override void OnStep(Room room)
    {
        _view = room.Session.Camera.View;
        if (Colours.Length > 0 && --_wait <= 0)
        {
            _wait = StepUpdates;
            if (_step + 1 < Colours.Length)
                _step++;
            else if (Repeats)
                _step = 0;
        }

        QueueRedraw();
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (!IsActive || Colours.Length == 0)
            return;
        var topLeft = ToLocal(new Vector2(_view.Left, _view.Top));
        DrawRect(new Rect2(topLeft, new Vector2(_view.Width, _view.Height)), Colours[Math.Min(_step, Colours.Length - 1)]);
    }
}
