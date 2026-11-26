using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// One of the bugs a Kago lets out: it waits on the ground for a moment, jumps, a random height and usually back
/// toward its hive, falls, and waits again. Until it has got clear of its hive, shots pass it by; after that any
/// shot destroys it: it bursts where it is, still hurting to the touch, and as the burst ends it leaves its drop and
/// is gone. Bombs and power bombs never touch it. Touching Samus hurts her and is the end of it.
/// </summary>
/// <remarks>
/// Its timelines are "waiting", curled up before its first jump; "jump_start", whose cue "jump" launches it;
/// "falling"; "landed", whose cue "wait" sets it waiting again, standing as it landed; and "burst", whose cue
/// "drop" leaves its drop and ends it.
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

    /// <summary>The sound it makes once, by name: a few updates into the air on its first jump.</summary>
    [Export, DataValue("sound")]
    public string BugSound { get; set; } = "";

    /// <summary>How many updates of jumping and falling pass, after its first wait, before it makes its sound.</summary>
    [Export, DataValue("first_sound_after")]
    public int FirstSoundAfter { get; set; } = 3;

    private enum Doing
    {
        Waiting,
        Poised,
        Jumping,
        Falling,
        Landed,
        Bursting,
    }

    private Doing _doing;
    private Countdown _wait;
    private Countdown _untilSound;
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
        _untilSound = Countdown.Of(_wait.Remaining + FirstSoundAfter);
        Play("waiting");
    }

    /// <inheritdoc/>
    protected override void OnTouchedPlayer() => Vanish();

    /// <summary>Counts one update toward its sound, which it makes as the count runs out. Only waiting, jumping and falling count.</summary>
    private void StepSound()
    {
        if (_untilSound.IsOver)
            return;
        _untilSound = _untilSound.Stepped();
        if (_untilSound.IsOver)
            Sound(BugSound);
    }

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => _clear && _doing != Doing.Bursting && base.IsHitBy(side);

    /// <inheritdoc/>
    protected override bool DiesAtOnce(Hit hit)
    {
        // Shot down, it bursts where it is, as the cartridge's bug plays its shot animation: it moves no more, shows
        // no hurt, and still hurts what touches it.
        EndHurt();
        Velocity = PixelVelocity.Zero;
        _doing = Doing.Bursting;
        Play("burst", restart: true);
        return false;
    }

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part) =>
        // The old game's bombs and power bombs strike none of what enemies let out, its bugs among it.
        hit.CarriesAny([new HitKind(HitKinds.Bomb), new HitKind(HitKinds.PowerBomb)]) ? HitResult.Ignored : base.TakeHit(hit, part);

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue == "jump")
            Jump();
        else if (cue == "wait")
        {
            // It waits standing, as it landed.
            _wait = Countdown.Of(ShortestWait + Random.Last.Part(0, 5));
            _doing = Doing.Waiting;
        }
        else if (cue == "drop")
        {
            // Its burst over, it leaves its drop where it is and is gone, a thing let out and no enemy killed.
            Drop(Drops, Position);
            Vanish();
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
                StepSound();
                _wait = _wait.Stepped();
                if (_wait.IsRunning)
                    return;
                Velocity = PixelVelocity.Zero;
                Play("jump_start", restart: true);
                _doing = Doing.Poised;
                return;

            case Doing.Poised or Doing.Landed or Doing.Bursting:
                return;

            case Doing.Jumping:
                StepSound();
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
                StepSound();
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
        StepSound();
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
