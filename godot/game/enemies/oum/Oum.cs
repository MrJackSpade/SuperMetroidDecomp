using Engine.Actors;
using Engine.Actors.Movement;
using Engine.Collision;
using Engine.Content;
using Engine.Input;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// An Oum: a great snail in a boulder of a shell. Dropped into the room it bounces to rest, and when Samus comes close
/// it rolls, back and forth between walls. She can stand on it and be carried, lean on it to stop it, or be pushed
/// along ahead of it. Now and then, when she is near, it turns to her and lunges out of its shell, which is the only
/// time its touch hurts.
/// </summary>
/// <remarks>
/// Its timelines are "idle_left", "idle_right", "roll_forward_left", "roll_forward_right", "roll_backward_left",
/// "roll_backward_right", "attack_left" and "attack_right". The attack's cue "attack_done" ends it, and the cues
/// "can_attack" and "cannot_attack" mark the parts of a roll from which it may lunge. Its hitbox is live only while
/// it lunges. Its bounces follow the speed curve its data names.
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
    private Collider? _lunge;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _curve = SpeedCurve.Read(Room.Content.ReadData(SpeedCurveFile));
        Facing = Facing.Right;
        _cooldown = Countdown.Of(AttackCooldown);
        _bouncesLeft = SettlingBounces;
        _lunge = EntityNode.ColliderOf(this, ColliderRole.Hitbox);
        if (_lunge is not null)
            _lunge.Enabled = false;
        Play("idle_left");
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        switch (cue)
        {
            case "attack_done":
                if (_doing != Doing.Attacking)
                    throw new InvalidOperationException($"The Oum \"{Name}\" finished lunging while it was {_doing}.");
                _doing = Doing.AttackDone;
                if (_lunge is not null)
                    _lunge.Enabled = false;
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
        if (_doing == Doing.Attacking)
            base.Touch(part);
        if (!IsRiddenByPlayer)
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
                Play(Facing == Facing.Left ? "idle_left" : "idle_right");
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
                    Play(Facing == Facing.Left ? "attack_left" : "attack_right", restart: true);
                    _doing = Doing.Attacking;
                    if (_lunge is not null)
                        _lunge.Enabled = true;
                    break;
                }

                var roll = PixelOffset.FromWhole(_heading == Facing.Left ? -RollSpeed : RollSpeed, 0);
                if (Blocked(roll))
                {
                    _heading = _heading.Opposite();
                    _rollsBackward = !_rollsBackward;
                    Play(RollTimeline, restart: true);
                }
                else
                {
                    MoveCarrying(roll);
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
        // Samus leaning against the way it rolls stops it for the next update, and puts it back where it was.
        int moved = Position.X.Whole - beforeX;
        GameInput input = Room.Player.Input;
        bool leans = moved == 0
            ? samusSide == _heading && input.IsHeld(_heading == Facing.Left ? GameAction.Right : GameAction.Left)
            : input.IsHeld(moved < 0 ? GameAction.Right : GameAction.Left);
        if (!leans)
            return;
        _stopped = true;
        if (moved != 0)
            MoveCarrying(PixelOffset.FromWhole(-moved, 0));
    }

    /// <summary>The name of its rolling timeline for the way it faces and rolls.</summary>
    private string RollTimeline => $"roll_{(_rollsBackward ? "backward" : "forward")}_{(Facing == Facing.Left ? "left" : "right")}";

    /// <summary>Sets it rolling forward.</summary>
    private void StartRolling()
    {
        _rollsBackward = false;
        Play(RollTimeline);
        _doing = Doing.Rolling;
    }

    /// <summary>Whether terrain stops its shell moving by <paramref name="offset"/>.</summary>
    private bool Blocked(PixelOffset offset) =>
        Solid is { } shell && Room.Terrain.IsSolidIn(shell.RoomRect.MovedBy(offset.X.Whole, offset.Y.Whole), SlopeTreatment.Wall);

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

    /// <summary>Moves it down, or up, as far as terrain lets it, takes whoever stands on it along, and returns whether something stopped it.</summary>
    private bool MoveDownWithRider(Pixels distance)
    {
        bool ridden = IsRiddenByPlayer;
        int before = Position.Y.Whole;
        bool stopped = MoveDownBy(distance);
        if (ridden)
            ShiftPlayer(PixelOffset.FromWhole(0, Position.Y.Whole - before));
        return stopped;
    }
}
