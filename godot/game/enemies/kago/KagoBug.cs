using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// One of the bugs a Kago lets out, which is one of the enemies' shots, as the cartridge's is: it waits on the ground
/// for a moment, jumps, a random height and usually back toward its hive, falls, and waits again. Until it has got
/// clear of its hive Samus's shots pass it by; after that any shot of hers that comes to stand in its square of the
/// room shoots it down: it bursts where it is, still hurting to the touch, and as the burst ends it leaves its drop
/// and is gone, as its settings have it. Touching Samus hurts her and is the end of it.
/// </summary>
/// <remarks>
/// Its settings name the timeline it comes out on, curled up before its first jump, and the burst a shot makes of
/// it. Its own data names the others: "jump_start_timeline", whose cue "jump" launches it; "falling_timeline"; and
/// "landed_timeline", whose cue "wait" sets it waiting again, standing as it landed.
/// </remarks>
public partial class KagoBug : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

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

    /// <summary>The data file of the timeline it sets itself to jump on.</summary>
    [Export, DataValue("jump_start_timeline")]
    public string JumpStartTimeline { get; set; } = "";

    /// <summary>The data file of the timeline it falls on.</summary>
    [Export, DataValue("falling_timeline")]
    public string FallingTimeline { get; set; } = "";

    /// <summary>The data file of the timeline it lands on.</summary>
    [Export, DataValue("landed_timeline")]
    public string LandedTimeline { get; set; } = "";

    private enum Doing
    {
        Waiting,
        Poised,
        Jumping,
        Falling,
        Landed,
        ShotDown,
    }

    private Doing _doing;
    private Countdown _wait;
    private Countdown _untilSound;
    private bool _clear;
    private PixelPosition _hiveAt;

    /// <summary>
    /// The random number last drawn as its last turn ended. The cartridge's bug plays its pictures, and what they
    /// tell it, once it has moved on each of its turns, and reads the number then; here its timeline's cues come as
    /// its next turn begins, by when the room has drawn again, so they read the number it kept.
    /// </summary>
    private RandomDraw _numberAsTurnEnded;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <summary>It moves itself against the blocks; its speed is what it leaves its place with.</summary>
    protected override bool MovesItself => true;

    /// <summary>Shots pass it by until it has got clear of its hive.</summary>
    public override bool IsHitBy(HitSide side) => _clear && base.IsHitBy(side);

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        // Shot down, it moves no more: it plays its burst out where it is, as its settings have it.
        HitResult result = base.TakeHit(hit, part);
        if (result == HitResult.Destroyed)
            _doing = Doing.ShotDown;
        return result;
    }

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        DataValues.Fill(this, room.Content);

        // It comes out where its hive is. Its first wait is a few updates longer by chance, read from the number
        // last drawn rather than a fresh one.
        _hiveAt = Position;
        _numberAsTurnEnded = room.Session.Random.Last;
        _wait = Countdown.Of(ShortestWait + _numberAsTurnEnded.Part(0, 3));
        _untilSound = Countdown.Of(_wait.Remaining + FirstSoundAfter);
    }

    /// <inheritdoc/>
    protected override void OnCue(Room room, string cue)
    {
        switch (cue)
        {
            case "jump":
                Jump(room);
                break;
            case "wait":
                // It waits standing, as it landed.
                _wait = Countdown.Of(ShortestWait + _numberAsTurnEnded.Part(0, 5));
                _doing = Doing.Waiting;
                break;
            default:
                throw new InvalidOperationException($"A Kago's bug does not know the cue \"{cue}\".");
        }
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        Behave(room);
        _numberAsTurnEnded = room.Session.Random.Last;
    }

    /// <summary>Plays its turn: it waits, sets itself, jumps or falls.</summary>
    private void Behave(Room room)
    {
        switch (_doing)
        {
            case Doing.Waiting:
                StepSound(room);
                CheckClear();
                _wait = _wait.Stepped();
                if (_wait.IsRunning)
                    return;
                PlayTimeline(JumpStartTimeline);
                _doing = Doing.Poised;
                return;

            case Doing.Poised or Doing.Landed or Doing.ShotDown:
                return;

            case Doing.Jumping:
                StepSound(room);
                CheckClear();
                if (MoveAgainstTerrain(room, Velocity.X, across: true))
                {
                    Velocity = new PixelVelocity(Pixels.Zero, Pixels.FromNumber(FallStartSpeed));
                    BeginFall();
                    return;
                }

                if (MoveAgainstTerrain(room, Velocity.Y, across: false))
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
                StepSound(room);
                CheckClear();
                if (MoveAgainstTerrain(room, Velocity.X, across: true))
                {
                    Velocity = Velocity.WithX(Pixels.Zero);
                    return;
                }

                if (MoveAgainstTerrain(room, Velocity.Y, across: false))
                {
                    // It keeps the speed it landed with, which is what its place is left with if it goes now; its
                    // next jump gives it another.
                    PlayTimeline(LandedTimeline);
                    _doing = Doing.Landed;
                    return;
                }

                Velocity = Velocity.WithY(Velocity.Y + Pixels.FromNumber(Gravity));
                return;

            default:
                throw new InvalidOperationException($"Undefined Kago bug stage {_doing}.");
        }
    }

    /// <summary>Counts one update toward its sound, which it makes as the count runs out. Only waiting, jumping and falling count.</summary>
    private void StepSound(Room room)
    {
        if (_untilSound.IsOver)
            return;
        _untilSound = _untilSound.Stepped();
        if (_untilSound.IsOver && BugSound.Length > 0)
            room.Session.Sounds.Request(BugSound, channel: "enemies");
    }

    /// <summary>Launches a jump: a random height, and back toward the hive when it has strayed, else either way by chance.</summary>
    private void Jump(Room room)
    {
        StepSound(room);
        CheckClear();
        RandomDraw last = _numberAsTurnEnded;
        int up = JumpSpeed + last.Part(8, 2);
        int toHive = _hiveAt.X.Whole - Position.X.Whole;
        bool left = Math.Abs(toHive) >= HomingDistance ? toHive < 0 : last.Part(8, 1) == 1;
        Pixels across = Pixels.FromNumber(AcrossSpeed);
        Velocity = new PixelVelocity(left ? -across : across, -Pixels.FromWhole(up));
        _doing = Doing.Jumping;
    }

    /// <summary>
    /// Lets shots land on it from now on once it is far enough across from its hive, where it is before it moves: it
    /// looks only while it waits, jumps or falls and as it starts a jump, never while it sets itself or lands.
    /// </summary>
    private void CheckClear()
    {
        if (Math.Abs(_hiveAt.X.Whole - Position.X.Whole) >= ClearDistance)
            _clear = true;
    }

    /// <summary>Turns the jump into a fall.</summary>
    private void BeginFall()
    {
        _doing = Doing.Falling;
        PlayTimeline(FallingTimeline);
    }
}
