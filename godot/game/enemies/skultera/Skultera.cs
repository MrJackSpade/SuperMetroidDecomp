using Engine.Actors;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Skultera: a fish that swims across the room, bobbing up and down in a wave, and turns round, taking a moment
/// over it, whenever anything is in its way.
/// </summary>
/// <remarks>
/// Its timelines are "swimming_left", "swimming_right", "turning_left" and "turning_right"; a turning timeline's cue
/// "turned" sets it swimming again.
/// </remarks>
public partial class Skultera : Enemy
{
    /// <summary>How far it swims across each update, as a number of pixels.</summary>
    [Export]
    public double Speed { get; set; } = 1;

    /// <summary>How far above and below its line it bobs, in pixels.</summary>
    [Export]
    public int Radius { get; set; } = 16;

    /// <summary>How many angle steps, of 256 to a full bob, it advances each update.</summary>
    [Export]
    public int AngleStep { get; set; } = 2;

    private int _angle;
    private int _lastBob;
    private bool _turning;
    private bool _turned;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _lastBob = Bob();
        Play(Facing == Facing.Left ? "swimming_left" : "swimming_right");
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue == "turned")
            _turned = true;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (_turning)
        {
            if (!_turned)
                return;
            _turning = false;
            Play(Facing == Facing.Left ? "swimming_left" : "swimming_right", restart: true);
            return;
        }

        if (MoveAcrossBy(Facing.Toward(Pixels.FromNumber(Speed))))
        {
            BeginTurn();
            return;
        }

        int bob = Bob();
        if (MoveDownBy(Pixels.FromWhole(bob - _lastBob)))
            BeginTurn();
        else
            _angle = Turns.Wrap(_angle + AngleStep);
        _lastBob = bob;
    }

    /// <summary>How far below its line the wave puts it now, in whole pixels.</summary>
    private int Bob() => -Turns.Sine(_angle, Pixels.FromWhole(Radius), Turns.Fineness.Coarse).Whole;

    /// <summary>Starts it turning to swim the other way.</summary>
    private void BeginTurn()
    {
        _turning = true;
        _turned = false;
        Facing = Facing.Opposite();
        Play(Facing == Facing.Left ? "turning_left" : "turning_right", restart: true);
    }
}
