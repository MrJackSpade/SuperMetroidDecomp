using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Effects;

/// <summary>
/// The dark of a Fireflea room. The room's light comes from its Firefleas: it pulses gently, and each Fireflea
/// that dies leaves the room darker, until with five gone it is as dark as it gets. It is told to "darken", and
/// shows the death from its next update on.
/// </summary>
public partial class FirefleaDarkness : RoomEffect
{
    /// <summary>How dark the room is with so many Firefleas gone, each out of thirty-one.</summary>
    private static readonly int[] DarknessLevels = [0, 6, 12, 18, 24, 25];

    /// <summary>How much darker the pulse makes it, step by step, each out of thirty-one.</summary>
    private static readonly int[] Pulse = [0, 1, 2, 3, 4, 5, 6, 5, 4, 3, 2, 1];

    /// <summary>How many updates each step of the pulse lasts.</summary>
    private const int PulseStepUpdates = 6;

    /// <summary>How many Firefleas have died, and how many of those deaths the room shows yet.</summary>
    private int _died;
    private int _killed;
    private int _pulse;
    private int _wait = PulseStepUpdates;
    private PixelRect _view;

    /// <inheritdoc/>
    protected override void OnSetup(Room room)
    {
        (_died, _killed, _pulse, _wait) = (0, 0, 0, PulseStepUpdates);
        ZIndex = 105;
    }

    /// <inheritdoc/>
    public override void Triggered(string what)
    {
        if (what != "darken")
        {
            base.Triggered(what);
            return;
        }

        _died = Math.Min(_died + 1, DarknessLevels.Length - 1);
    }

    /// <inheritdoc/>
    protected override void OnStep(Room room)
    {
        _view = room.Session.Camera.View;

        // The deaths are looked at here, once an update, before anything in the room acts.
        _killed = _died;

        // At its darkest the pulse stops at its darkest too.
        if (_killed >= DarknessLevels.Length - 1)
            _pulse = Array.IndexOf(Pulse, 6);
        else if (--_wait <= 0)
        {
            _wait = PulseStepUpdates;
            _pulse = (_pulse + 1) % Pulse.Length;
        }

        QueueRedraw();
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (!IsActive)
            return;
        float dark = Math.Min(31, DarknessLevels[_killed] + Pulse[_pulse]) / 31f;
        var topLeft = ToLocal(new Vector2(_view.Left, _view.Top));
        DrawRect(new Rect2(topLeft, new Vector2(_view.Width, _view.Height)), new Color(0f, 0f, 0f, dark));
    }
}
