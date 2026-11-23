using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Bull: a floating bag of gas that waits a moment, takes aim at Samus, and drifts toward where she was, gathering
/// speed; once she is well off its line it slows to a stop and starts again. A shot that cannot hurt it knocks it
/// away the way the shot was going. Nothing in the room stops it.
/// </summary>
/// <remarks>Its timelines are "idle" and "shot".</remarks>
public partial class Bull : Enemy
{
    /// <summary>How many updates pass between one gain of speed and the next.</summary>
    [Export]
    public int AccelerationInterval { get; set; } = 1;

    /// <summary>How many updates pass between one loss of speed and the next.</summary>
    [Export]
    public int DecelerationInterval { get; set; } = 1;

    /// <summary>The speed at which it stops gathering more, as a number of pixels.</summary>
    [Export]
    public double MaximumSpeed { get; set; } = 2;

    /// <summary>How many updates it waits before each drift.</summary>
    [Export, DataValue("wait_updates")]
    public int WaitUpdates { get; set; } = 16;

    /// <summary>How much its gain of speed itself grows each time, as a number of pixels.</summary>
    [Export, DataValue("acceleration_step")]
    public double AccelerationStep { get; set; } = 0.09375;

    /// <summary>How far off its line Samus must be, in 256ths of a turn, for it to slow down.</summary>
    [Export, DataValue("off_target_turn")]
    public int OffTargetTurn { get; set; } = 48;

    /// <summary>The speed a shot that cannot hurt it knocks it away with.</summary>
    [Export, DataValue("kick_speed")]
    public double KickSpeed { get; set; } = 6;

    /// <summary>The loss of speed a knock starts with.</summary>
    [Export, DataValue("kick_acceleration")]
    public double KickAcceleration { get; set; } = 1;

    /// <summary>How many updates after a knock another cannot move it.</summary>
    [Export, DataValue("kick_guard_updates")]
    public int KickGuardUpdates { get; set; } = 48;

    private enum Doing
    {
        Waiting,
        Aiming,
        Gathering,
        Slowing,
    }

    private Doing _doing;
    private Countdown _wait;
    private Countdown _interval;
    private Countdown _guard = Countdown.Over;
    private int _angle;
    private Pixels _speed;
    private Pixels _gain;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        Play("idle");
        _wait = Countdown.Of(WaitUpdates);
        _interval = Countdown.Of(AccelerationInterval);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        _guard = _guard.Stepped();
        Pixels step = Pixels.FromNumber(AccelerationStep);
        switch (_doing)
        {
            case Doing.Waiting:
                _wait = _wait.Stepped();
                if (_wait.IsRunning)
                    return;
                _wait = Countdown.Of(WaitUpdates);
                _doing = Doing.Aiming;
                return;

            case Doing.Aiming:
                _angle = AngleToSamus();
                _doing = Doing.Gathering;
                return;

            case Doing.Gathering:
                if (_speed < Pixels.FromNumber(MaximumSpeed) && Tick(AccelerationInterval))
                {
                    _gain += step;
                    _speed += _gain;
                }

                Drift();
                int off = Math.Abs(Turns.Wrap(AngleToSamus() - _angle + Turns.PerTurn / 2) - Turns.PerTurn / 2);
                if (off >= OffTargetTurn)
                    _doing = Doing.Slowing;
                return;

            case Doing.Slowing:
                if (_speed.IsZero || _speed.IsNegative || _gain.IsZero || _gain.IsNegative)
                {
                    _doing = Doing.Waiting;
                    _gain = Pixels.Zero;
                    _speed = Pixels.Zero;
                    return;
                }

                Drift();
                if (Tick(DecelerationInterval))
                {
                    _gain -= step;
                    _speed -= _gain;
                }

                return;

            default:
                throw new InvalidOperationException($"Undefined Bull stage {_doing}.");
        }
    }

    /// <inheritdoc/>
    protected override void OnAbsorbed(Hit hit, Collider part)
    {
        if (_guard.IsRunning)
            return;
        // A shot knocks it the way the shot was going; anything with no direction, such as a bomb, knocks it upward.
        PixelOffset way = hit.Direction;
        _angle = way.X.IsZero && way.Y.IsZero ? Turns.PerTurn * 3 / 4 : Turns.AngleOf(way.X.Whole * 256 + way.X.Fraction / 256, way.Y.Whole * 256 + way.Y.Fraction / 256);
        _gain = Pixels.FromNumber(KickAcceleration);
        _speed = Pixels.FromNumber(KickSpeed);
        _doing = Doing.Slowing;
        _interval = Countdown.Of(DecelerationInterval);
        _guard = Countdown.Of(KickGuardUpdates);
        Play("shot", restart: true);
    }

    /// <summary>Counts down the interval between changes of speed, and says when one is due.</summary>
    private bool Tick(int interval)
    {
        _interval = _interval.Stepped();
        if (_interval.IsRunning)
            return false;
        _interval = Countdown.Of(interval);
        return true;
    }

    /// <summary>The angle from it to Samus, 0 straight right and a quarter turn straight down.</summary>
    private int AngleToSamus()
    {
        PixelOffset toSamus = Target - Position;
        return Turns.AngleOf(toSamus.X.Whole, toSamus.Y.Whole);
    }

    /// <summary>Drifts one update along its line, at half its speed.</summary>
    private void Drift()
    {
        Pixels half = _speed.Share(1, 2);
        Position += new PixelOffset(Turns.Cosine(_angle, half), Turns.Sine(_angle, half));
    }
}
