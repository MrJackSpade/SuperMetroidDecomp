using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Kzan: a spiked crusher that hangs from the ceiling. It waits, drops faster and faster to the bottom of its run,
/// rests there, and climbs slowly back. Its top can be stood on, and carries whoever stands there; its underside
/// hurts.
/// </summary>
/// <remarks>Its timeline is "idle". Its solid collider is its top and its hitbox its spiked underside.</remarks>
public partial class Kzan : Enemy
{
    /// <summary>The speed its drop starts at, as a number of pixels an update.</summary>
    [Export]
    public double FallSpeed { get; set; } = 1;

    /// <summary>How many updates it waits at the top before dropping.</summary>
    [Export]
    public int FallWait { get; set; } = 64;

    /// <summary>How far it drops, in pixels.</summary>
    [Export]
    public int FallDistance { get; set; } = 64;

    /// <summary>How many updates it rests at the bottom.</summary>
    [Export, DataValue("rise_wait")]
    public int RiseWait { get; set; } = 64;

    /// <summary>How far it climbs each update, as a number of pixels.</summary>
    [Export, DataValue("rise_speed")]
    public double RiseSpeed { get; set; } = 0.5;

    /// <summary>The speed its drop gains each update.</summary>
    [Export, DataValue("fall_quickening")]
    public double FallQuickening { get; set; } = 0.0625;

    /// <summary>The fastest it drops.</summary>
    [Export, DataValue("fastest_fall")]
    public double FastestFall { get; set; } = 4;

    /// <summary>The sound of it reaching the bottom, by name.</summary>
    [Export, DataValue("landing_sound")]
    public string LandingSound { get; set; } = "";

    private enum Doing
    {
        WaitingToFall,
        Falling,
        WaitingToRise,
        Rising,
    }

    private Doing _doing;
    private int _wait;
    private Countdown _rest;
    private Pixels _speed;
    private int _top;
    private int _bottom;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        Play("idle");
        _speed = Pixels.FromNumber(FallSpeed);
        _top = Position.Y.Whole;
        _bottom = _top + FallDistance;
        _wait = FallWait;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.WaitingToFall:
                // The wait counts down past nothing and round again, so a wait of none is the longest of all.
                _wait = (_wait - 1) & 0xFFFF;
                if (_wait != 0)
                    return;
                _wait = FallWait;
                _speed = Pixels.FromNumber(FallSpeed);
                _doing = Doing.Falling;
                return;

            case Doing.Falling:
                Pixels fallen = Position.Y + _speed;
                if (fallen.Whole >= _bottom)
                {
                    fallen = fallen.WithWhole(_bottom);
                    _rest = Countdown.Of(RiseWait);
                    _doing = Doing.WaitingToRise;
                    Sound(LandingSound);
                }

                // It carries Samus when she stands on it, and that is all it does to her as it moves: one it comes down
                // on is not pushed ahead of it, as the cartridge's moves her only when she stands on it.
                MoveCarrying(new PixelOffset(Pixels.Zero, fallen - Position.Y), pushesAhead: false);
                Pixels faster = _speed + Pixels.FromNumber(FallQuickening), fastest = Pixels.FromNumber(FastestFall);
                _speed = faster < fastest ? faster : fastest;
                return;

            case Doing.WaitingToRise:
                if (_rest.IsOver)
                    return;
                _rest = _rest.Stepped();
                if (_rest.IsOver)
                    _doing = Doing.Rising;
                return;

            case Doing.Rising:
                Pixels risen = Position.Y - Pixels.FromNumber(RiseSpeed);
                if (risen.Whole < _top)
                {
                    risen = risen.WithWhole(_top);
                    _doing = Doing.WaitingToFall;
                }

                MoveCarrying(new PixelOffset(Pixels.Zero, risen - Position.Y), pushesAhead: false);
                return;

            default:
                throw new InvalidOperationException($"Undefined Kzan stage {_doing}.");
        }
    }
}
