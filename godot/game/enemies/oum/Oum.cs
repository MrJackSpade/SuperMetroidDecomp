using Engine.Actors;
using Engine.Actors.Movement;
using Engine.Collision;
using Engine.Content;
using Engine.Input;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// An Oum: a great snail in a boulder of a shell. Dropped into the room it bounces to rest, and when Samus comes close
/// it rolls, back and forth between walls. She can stand on it and ride its bounces, lean on it to stop it, or be
/// pushed along ahead of it. Now and then, when she is near, it turns to her and lunges out of its shell on a jet, which is
/// the only time its touch hurts.
/// </summary>
/// <remarks>
/// Its timelines are "idle_left", "idle_right", "roll_forward_left", "roll_forward_right", "roll_backward_left",
/// "roll_backward_right", "attack_left" and "attack_right". The attack's cue "attack_done" ends it, and the cues
/// "can_attack" and "cannot_attack" mark the parts of a roll from which it may lunge. Its bounces follow the speed
/// curve its data names.
/// <para>What shots and Samus meet of it are the boxes of the picture it shows: its data's "poses"
/// (<see cref="ColliderPoses"/>) place its hurtboxes "Part1" to "Part3" and its hitboxes "Part1Touch" to "Part3Touch"
/// by picture, and each step of its timelines begins with the cue "pose:" and the picture's name. A pose says what
/// each part does: under "shots", "shell" for one that swallows a shot and "soft" for one its reactions answer; under
/// "touches", "hurts" for one that hurts Samus and shoves her, and "shoves" for one that only shoves her.</para>
/// </remarks>
public partial class Oum : Enemy
{
    /// <summary>How near across Samus must be, in pixels, to set it rolling, and to push or be pushed by it.</summary>
    [Export, DataValue("pushing_reach_across")]
    public int PushingReachAcross { get; set; } = 24;

    /// <summary>How near Samus's height must be to its own, in pixels, to push or be pushed by it.</summary>
    [Export, DataValue("pushing_reach_down")]
    public int PushingReachDown { get; set; } = 32;

    /// <summary>How near across Samus must be, in pixels, for it to lunge.</summary>
    [Export, DataValue("attack_reach")]
    public int AttackReach { get; set; } = 32;

    /// <summary>How many updates pass between its lunges at least.</summary>
    [Export, DataValue("attack_cooldown")]
    public int AttackCooldown { get; set; } = 129;

    /// <summary>How many times it bounces before it lies still.</summary>
    [Export, DataValue("settling_bounces")]
    public int SettlingBounces { get; set; } = 3;

    /// <summary>How far along the curve a bounce moves each update, in 256ths of a mark.</summary>
    [Export, DataValue("bounce_step")]
    public int BounceStep { get; set; } = 384;

    /// <summary>The mark of the curve its fall speeds up to and stays at.</summary>
    [Export, DataValue("fastest_fall_mark")]
    public int FastestFallMark { get; set; } = 64;

    /// <summary>How many marks of the curve each landing takes off its bounce.</summary>
    [Export, DataValue("impact_loss_marks")]
    public int ImpactLossMarks { get; set; } = 16;

    /// <summary>How far it rolls each update, in pixels.</summary>
    [Export, DataValue("roll_speed")]
    public int RollSpeed { get; set; } = 1;

    /// <summary>How much further ahead, in pixels, it feels for a wall while it rolls toward Samus near it.</summary>
    [Export, DataValue("push_reach")]
    public int PushReach { get; set; } = 16;

    /// <summary>How far its touch shoves Samus aside, in pixels.</summary>
    [Export, DataValue("shove_distance")]
    public int ShoveDistance { get; set; } = 4;

    /// <summary>The data file of the speed curve its bounces follow.</summary>
    [Export, DataValue("speed_curve")]
    public string SpeedCurveFile { get; set; } = "";

    private enum Doing
    {
        Idle,
        Rolling,
        Attacking,
        AttackDone,
    }

    private SpeedCurve? _curve;
    private Doing _doing;
    private Facing _heading = Facing.Right;
    private Countdown _cooldown;
    private int _bouncesLeft;
    private int _place;
    private bool _rising;
    private bool _rollsBackward;
    private bool _mayAttack;
    private bool _stopped;
    private ColliderPoses? _poses;

    /// <summary>
    /// Whether it still shows the left-facing idle it is placed with, which counts as its right-facing one: as on the
    /// cartridge, asking for the right-facing idle then changes nothing, and the first other timeline it starts, the
    /// left-facing idle too, starts from the beginning.
    /// </summary>
    private bool _showsPlacedIdle;

    /// <summary>What each of its parts does with a shot and with Samus's touch, by its pose and the part's name.</summary>
    private readonly Dictionary<(string Pose, string Part), (string Shot, string Touch)> _parts = [];

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _poses = ColliderPoses.Read(data.Get("poses"), this);
        _parts.Clear();
        foreach ((string name, DataNode pose) in data.Get("poses").Entries())
        {
            // A picture the cartridge gives no boxes is a pose with no parts, and says nothing of them.
            if (!pose.Has("shots"))
                continue;
            foreach ((string part, DataNode shot) in pose.Get("shots").Entries())
            {
                string touch = pose.Get("touches").Text(part);
                if (shot.AsText() is not ("shell" or "soft"))
                    throw shot.Problem($"is \"{shot.AsText()}\", which is not \"shell\" or \"soft\"");
                if (touch is not ("hurts" or "shoves"))
                    throw pose.Get("touches").Get(part).Problem($"is \"{touch}\", which is not \"hurts\" or \"shoves\"");
                _parts[(name, part)] = (shot.AsText(), touch);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _curve = SpeedCurve.Read(Room.Content.ReadData(SpeedCurveFile));
        Facing = Facing.Right;
        _cooldown = Countdown.Of(AttackCooldown);
        _bouncesLeft = SettlingBounces;
        Play("idle_left");
        _showsPlacedIdle = true;
    }

    /// <summary>Starts the timeline named <paramref name="name"/> unless it is the one showing, or its placed idle stands for it.</summary>
    private void Start(string name)
    {
        if (!_showsPlacedIdle)
        {
            Play(name);
            return;
        }

        if (name == "idle_right")
            return;
        _showsPlacedIdle = false;
        Play(name, restart: true);
    }

    /// <summary>What the part of it a shot or Samus has met does, in the pose it is in.</summary>
    private (string Shot, string Touch) PartMet(Collider part)
    {
        // A part is a hurtbox, which a shot meets, and a hitbox named for it with "Touch" after, which Samus meets.
        string pose = _poses?.Current ?? "", name = part.Name.ToString();
        if (name.EndsWith("Touch", StringComparison.Ordinal))
            name = name[..^"Touch".Length];
        return _parts.TryGetValue((pose, name), out (string Shot, string Touch) met)
            ? met
            : throw new InvalidOperationException($"The Oum \"{Name}\"'s pose \"{pose}\" does not say what its part \"{part.Name}\" does.");
    }

    /// <summary>
    /// A beam, missile or super missile that meets its shell does nothing to it and shows nothing, and is spent on it,
    /// but a plasma beam flies on; a bomb's blast there takes no notice of it. Anything else, and anything that meets
    /// its soft part, is answered as its reactions say.
    /// </summary>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        ArgumentNullException.ThrowIfNull(hit);
        ArgumentNullException.ThrowIfNull(part);
        if (IsDead || PartMet(part).Shot != "shell")
            return base.TakeHit(hit, part);
        if (hit.Carries(new HitKind(HitKinds.Bomb)) || hit.Carries(new HitKind(HitKinds.Plasma)))
            return HitResult.Ignored;
        if (hit.CarriesAny([new HitKind(HitKinds.Beam), new HitKind(HitKinds.Missile), new HitKind(HitKinds.SuperMissile)]))
            return HitResult.Dud;
        return base.TakeHit(hit, part);
    }

    /// <summary>A beam, missile, super missile or bomb its soft part takes no harm from is answered with a dud's sound of its own besides.</summary>
    protected override void OnAbsorbed(Hit hit, Collider part)
    {
        if (hit.CarriesAny([new HitKind(HitKinds.Beam), new HitKind(HitKinds.Missile), new HitKind(HitKinds.SuperMissile), new HitKind(HitKinds.Bomb)]))
            Sound(Sounds.ShotDeflected);
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        const string pose = "pose:";
        if (cue.StartsWith(pose, StringComparison.Ordinal))
        {
            (_poses ?? throw new InvalidOperationException($"The data of the Oum \"{Name}\" has not been read.")).Take(cue[pose.Length..]);
            return;
        }

        switch (cue)
        {
            case "attack_done":
                if (_doing != Doing.Attacking)
                    throw new InvalidOperationException($"The Oum \"{Name}\" finished lunging while it was {_doing}.");
                _doing = Doing.AttackDone;
                break;
            case "can_attack":
                _mayAttack = true;
                break;
            case "cannot_attack":
                _mayAttack = false;
                break;
        }
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        // The boxes of its jet hurt her and then shove her, as every other box only does.
        if (PartMet(part).Touch == "hurts")
            base.Touch(part);
        if (!IsRiddenByPlayer(Riding.FeetAbove))
            ShiftPlayer(PixelOffset.FromWhole(Target.X.Whole < Position.X.Whole ? -ShoveDistance : ShoveDistance, 0));
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        Facing samusSide = TowardPlayer;
        bool near = Math.Abs(Target.Y.Whole - Position.Y.Whole) < PushingReachDown && Math.Abs(Target.X.Whole - Position.X.Whole) < PushingReachAcross;
        int beforeX = Position.X.Whole;
        bool wasStopped = _stopped;
        _stopped = false;
        switch (_doing)
        {
            case Doing.Idle:
                if (_bouncesLeft != 0)
                {
                    Bounce();
                    break;
                }

                Facing = samusSide;
                if (Facing == Facing.Left)
                    _heading = Facing.Left;
                Start(Facing == Facing.Left ? "idle_left" : "idle_right");
                if (Math.Abs(Target.X.Whole - Position.X.Whole) >= PushingReachAcross)
                    break;
                StartRolling();
                break;

            case Doing.Rolling:
                if (_bouncesLeft != 0)
                {
                    Bounce();
                }
                else if (!MoveDownWithRider(Pixels.FromWhole(1)))
                {
                    // The floor has gone from under it: it drops, and bounces to rest again.
                    _place = 0;
                    _rising = false;
                    _bouncesLeft = SettlingBounces;
                }

                if (wasStopped)
                    break;
                _cooldown = _cooldown.Stepped();
                if (_cooldown.IsOver && Math.Abs(Target.X.Whole - Position.X.Whole) < AttackReach && _mayAttack && _bouncesLeft == 0)
                {
                    _heading = samusSide;
                    _cooldown = Countdown.Of(AttackCooldown);
                    Facing = _heading;
                    Start(Facing == Facing.Left ? "attack_left" : "attack_right");
                    _doing = Doing.Attacking;
                    break;
                }

                if (Roll(near ? samusSide : default(Facing?)))
                {
                    _heading = _heading.Opposite();
                    _rollsBackward = !_rollsBackward;
                    Start(RollTimeline);
                }

                break;

            case Doing.Attacking:
                break;

            case Doing.AttackDone:
                StartRolling();
                break;

            default:
                throw new InvalidOperationException($"Undefined Oum stage {_doing}.");
        }

        if (!near)
            return;

        // Samus near it on the side it moved toward is pushed along as far as it moved; one standing on it is too
        // high to be pushed so, and is not carried across.
        int moved = Position.X.Whole - beforeX;
        if (moved != 0 && (moved < 0) == (samusSide == Facing.Left))
            ShiftPlayer(PixelOffset.FromWhole(moved, 0));

        // Samus leaning against the way it rolls stops it for the next update, and puts it back where it was, though
        // she stays where it pushed her.
        GameInput input = Room.Player.Input;
        bool leans = moved == 0
            ? samusSide == _heading && input.IsHeld(_heading == Facing.Left ? GameAction.Right : GameAction.Left)
            : input.IsHeld(moved < 0 ? GameAction.Right : GameAction.Left);
        if (!leans)
            return;
        _stopped = true;
        Position -= PixelOffset.FromWhole(moved, 0);
    }

    /// <summary>The name of its rolling timeline for the way it faces and rolls.</summary>
    private string RollTimeline => $"roll_{(_rollsBackward ? "backward" : "forward")}_{(Facing == Facing.Left ? "left" : "right")}";

    /// <summary>Sets it rolling forward.</summary>
    private void StartRolling()
    {
        _rollsBackward = false;
        Start(RollTimeline);
        _doing = Doing.Rolling;
    }

    /// <summary>
    /// Rolls one update the way it heads, carrying no one, and returns whether a wall stopped it, which leaves it
    /// against the wall. Rolling toward Samus near it, it feels for a wall as much further ahead as its push reach and
    /// draws back by as much after, as the cartridge does: a wall that near turns it, and it ends that far short of it.
    /// </summary>
    private bool Roll(Facing? pushSide)
    {
        Pixels reach = Pixels.FromWhole(_heading == pushSide ? PushReach : 0);
        bool blocked = Room.Mover.MoveAcross(MovingBody(), _heading.Toward(Pixels.FromWhole(RollSpeed) + reach), MovesAcross).Blocked;
        Position -= new PixelOffset(_heading.Toward(reach), Pixels.Zero);
        return blocked;
    }

    /// <summary>Moves one update of a bounce: falling until it lands, each landing weaker, and rising between.</summary>
    private void Bounce()
    {
        SpeedCurve curve = _curve ?? throw new InvalidOperationException($"The Oum \"{Name}\" is not in a room.");
        if (!_rising)
        {
            _place = Math.Min(_place + BounceStep, FastestFallMark * SpeedCurve.PartsPerMark);
            if (!MoveDownWithRider(curve.SpeedAt(_place)))
                return;

            _bouncesLeft--;
            _place -= ImpactLossMarks * SpeedCurve.PartsPerMark;
            if (_place < 0)
                _bouncesLeft = 0;
            _rising = true;
            return;
        }

        _place -= BounceStep;
        if (_place < 0)
        {
            _place = 0;
            _rising = false;
            return;
        }

        MoveDownWithRider(-curve.SpeedAt(_place));
    }

    /// <summary>
    /// How its moves are tried, as the cartridge moves an enemy: only where each would end, and sloped ground that holds
    /// things up by their middle met under its middle alone.
    /// </summary>
    private static readonly MoveOptions Moves =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.ClimbUnderMiddle, RestsUnderSlopes = true };

    /// <summary>
    /// How its roll is tried: as its other moves, but such sloped ground in the column it rolls into is a whole wall
    /// to it, whatever the slope's shape, as the cartridge's Oum alone takes slopes for walls.
    /// </summary>
    private static readonly MoveOptions MovesAcross = Moves with { Slopes = SlopeTreatment.WallMiddleHeld };

    /// <summary>The collider it moves by.</summary>
    private Collider MovingBody() => Body ?? throw new InvalidOperationException($"The Oum \"{Name}\" has no body collider.");

    /// <summary>Moves it down, or up, as far as terrain lets it, takes whoever stands on it along, and returns whether something stopped it.</summary>
    private bool MoveDownWithRider(Pixels distance)
    {
        bool ridden = IsRiddenByPlayer(Riding.FeetAbove);
        int before = Position.Y.Whole;
        bool stopped = Room.Mover.MoveDown(MovingBody(), distance, Moves).Blocked;
        if (ridden)
            ShiftPlayer(PixelOffset.FromWhole(0, Position.Y.Whole - before));
        return stopped;
    }
}
