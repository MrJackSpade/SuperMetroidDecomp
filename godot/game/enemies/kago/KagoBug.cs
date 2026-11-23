using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// One of the bugs a Kago lets out: it waits on the ground for a moment, jumps, a random height and usually back
/// toward its hive, falls, and waits again. Until it has got clear of its hive, shots pass it by.
/// </summary>
/// <remarks>
/// Its timelines are "waiting", "jump_start", whose cue "jump" launches it, "falling", and "landed", whose cue "wait"
/// sets it waiting again.
/// </remarks>
public partial class KagoBug : Enemy
{
    /// <summary>The upward speed its lowest jump starts with, in pixels.</summary>
    [Export, DataValue("jump_speed")]
    public int JumpSpeed { get; set; } = 8;

    /// <summary>How far it travels across each update of a jump, as a number of pixels.</summary>
    [Export, DataValue("across_speed")]
    public double AcrossSpeed { get; set; } = 2;

    /// <summary>The downward speed it gains each update in the air.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; } = 0.875;

    /// <summary>The downward speed its fall starts with when something stops its jump.</summary>
    [Export, DataValue("fall_start_speed")]
    public double FallStartSpeed { get; set; } = 1;

    /// <summary>The fewest updates it waits on the ground.</summary>
    [Export, DataValue("shortest_wait")]
    public int ShortestWait { get; set; } = 2;

    /// <summary>How far across from its hive it must be, in pixels, before it always jumps back toward it.</summary>
    [Export, DataValue("homing_distance")]
    public int HomingDistance { get; set; } = 48;

    /// <summary>How far across from its hive it must be, in pixels, before shots can land on it.</summary>
    [Export, DataValue("clear_distance")]
    public int ClearDistance { get; set; } = 23;

    private enum Doing
    {
        Waiting,
        Poised,
        Jumping,
        Falling,
        Landed,
    }

    private Doing _doing;
    private Countdown _wait;
    private bool _clear;
    private PixelPosition _hiveAt;

    /// <summary>The hive that let it out.</summary>
    public Kago? Hive { get; set; }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _hiveAt = Hive?.Position ?? Position;
        // Its first wait is a few updates longer by chance, read from the number last drawn rather than a fresh one.
        _wait = Countdown.Of(ShortestWait + Random.Last.Part(0, 3));
        Play("waiting");
    }

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => _clear && base.IsHitBy(side);

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue == "jump")
            Jump();
        else if (cue == "wait")
        {
            _wait = Countdown.Of(ShortestWait + Random.Last.Part(0, 5));
            _doing = Doing.Waiting;
            Play("waiting", restart: true);
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (Math.Abs(_hiveAt.X.Whole - Position.X.Whole) >= ClearDistance)
            _clear = true;
        switch (_doing)
        {
            case Doing.Waiting:
                _wait = _wait.Stepped();
                if (_wait.IsRunning)
                    return;
                Velocity = PixelVelocity.Zero;
                Play("jump_start", restart: true);
                _doing = Doing.Poised;
                return;

            case Doing.Poised or Doing.Landed:
                return;

            case Doing.Jumping:
                if (MoveAcrossBy(Velocity.X))
                {
                    Velocity = new PixelVelocity(Pixels.Zero, Pixels.FromNumber(FallStartSpeed));
                    BeginFall();
                    return;
                }

                if (MoveDownBy(Velocity.Y))
                {
                    Velocity = Velocity.WithY(Pixels.FromNumber(FallStartSpeed));
                    BeginFall();
                    return;
                }

                Velocity = Velocity.WithY(Velocity.Y + Pixels.FromNumber(Gravity));
                if (!Velocity.Y.IsNegative)
                    BeginFall();
                return;

            case Doing.Falling:
                if (MoveAcrossBy(Velocity.X))
                {
                    Velocity = Velocity.WithX(Pixels.Zero);
                    return;
                }

                if (MoveDownBy(Velocity.Y))
                {
                    Velocity = PixelVelocity.Zero;
                    Play("landed", restart: true);
                    _doing = Doing.Landed;
                    return;
                }

                Velocity = Velocity.WithY(Velocity.Y + Pixels.FromNumber(Gravity));
                return;

            default:
                throw new InvalidOperationException($"Undefined Kago bug stage {_doing}.");
        }
    }

    /// <summary>Launches a jump: a random height, and back toward the hive when it has strayed, else either way by chance.</summary>
    private void Jump()
    {
        RandomDraw last = Random.Last;
        int up = JumpSpeed + last.Part(8, 2);
        int toHive = _hiveAt.X.Whole - Position.X.Whole;
        bool left = Math.Abs(toHive) >= HomingDistance ? toHive < 0 : last.Part(8, 1) == 1;
        Pixels across = Pixels.FromNumber(AcrossSpeed);
        Velocity = new PixelVelocity(left ? -across : across, -Pixels.FromWhole(up));
        _doing = Doing.Jumping;
    }

    /// <summary>Turns the jump into a fall.</summary>
    private void BeginFall()
    {
        _doing = Doing.Falling;
        Play("falling", restart: true);
    }
}
