using Engine.Actors;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// A shinespark: with a shine stored, a jump taken up aiming level with her arm at rest, up or diagonally up winds
/// her up and then launches her in a straight line, across, up or diagonally, faster and faster, hurting what she
/// passes through and costing her energy as she goes. It ends against something solid, or when her energy runs low.
/// There she is held while two echoes of her circle out, round and back in; then, after a moment, the echoes are let
/// go and fly off, and she is free. How long they circle is counted on from what the last crash left
/// (<see cref="Compatibility.CrashOrbitTravel"/>). The circling echoes are no shots of hers; let go, each takes one of
/// her shots' places, the second while one is free and the first only while two are, and one with no place is gone.
/// </summary>
public sealed class ShinesparkState : PlayerState, IUntouchable, ISamusLook
{
    private enum Stage
    {
        WindingUp,
        Flying,
        Crashed,
    }

    private enum Crash
    {
        Expanding,
        Circling,
        Contracting,
        Holding,
    }

    /// <summary>How far the echoes move out or in each update, how far out they circle from, and how far each turns an update.</summary>
    private const int CrashRadiusStep = 4, CrashCirclingRadius = 12, CrashTurn = 4;

    /// <summary>Whether the windup starts lifted, as one begun out of a jump does.</summary>
    private readonly bool _lifted;

    /// <summary>A shinespark.</summary>
    /// <param name="lifted">Whether its windup starts lifted off where she is, as one begun out of a jump does.</param>
    public ShinesparkState(bool lifted) => _lifted = lifted;

    private Stage _stage;
    private Crash _crash;
    private int _crashRadius;
    private int _firstAngle, _secondAngle;
    private ShinesparkEcho? _firstEcho, _secondEcho;
    private Countdown _left;
    private int _across;
    private int _down;
    private Pixels _acrossSpeed;
    private Pixels _riseSpeed;
    private Pixels _riseGain;

    /// <inheritdoc/>
    public string Look => _stage switch
    {
        Stage.WindingUp => "shinespark_windup",
        _ => _across == 0 ? "shinespark_vertical" : _down == 0 ? "shinespark_horizontal" : "shinespark_diagonal",
    };

    /// <summary>Whether she is still winding up.</summary>
    public bool IsWindingUp => _stage == Stage.WindingUp;

    /// <summary>
    /// Where she stood before she moved this update, while she flies; none on the update she launches and once she
    /// has crashed. Her speed echoes are left from here.
    /// </summary>
    public PixelPosition? FlownFrom { get; private set; }

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <summary>A knockback waits while she is in this state, as the cartridge refuses it here and tries again each update of its time.</summary>
    public override bool HoldsOffInterruptions => true;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        var samus = (Samus)owner;
        samus.SpendShine();
        owner.StopAcross();
        owner.Velocity = PixelVelocity.Zero;
        samus.SpendChargeOnShinespark();

        // The lift moves her as she is, whatever is above her, as the cartridge's does.
        if (_lifted)
            owner.Position += PixelOffset.FromWhole(0, -samus.ShinesparkLift);
        owner.TrySetForm(owner.FormOr(Player.Jumping), aroundMiddle: true);
        samus.BoostFully();
        samus.BeginSparkColouring();
        _stage = Stage.WindingUp;
        _left = Countdown.Of(samus.ShinesparkWindupUpdates);
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        var samus = (Samus)owner;
        switch (_stage)
        {
            case Stage.WindingUp:
                WindUp(samus);
                return null;
            case Stage.Flying:
                Fly(samus);
                return null;
            case Stage.Crashed:
                // Its glow is kept going through the crash, and outlasts its end by a moment.
                bool over = StepCrash(samus);
                samus.KeepSparkGlow(over ? samus.SparkFinishedGlowUpdates : samus.SparkGlowUpdates);
                return over ? owner.StateAtRest() : null;
            default:
                throw new InvalidOperationException($"Undefined shinespark stage {_stage}.");
        }
    }

    /// <summary>
    /// Waits for a direction, which launches her only with the jump held, the first of these that holds, as the
    /// cartridge has it: up launches her straight up, the angle-up action diagonally the way she faces, and the way she
    /// faces across. The way behind her does nothing. The windup running out launches her straight up, whatever is
    /// held on that update: the cartridge's launch then takes the place of what the input asked for.
    /// </summary>
    private void WindUp(Samus samus)
    {
        _left = _left.Stepped();
        if (_left.IsOver)
        {
            Launch(samus, 0, -1);
            return;
        }

        GameInput input = samus.Input;
        int forward = samus.Facing == Facing.Right ? 1 : -1;
        if (input.IsHeld(GameAction.Jump))
        {
            if (input.IsHeld(GameAction.Up))
            {
                Launch(samus, 0, -1);
                return;
            }

            if (input.IsHeld(Actions.AngleUp))
            {
                Launch(samus, forward, -1);
                return;
            }

            if (input.IsHeld(forward > 0 ? GameAction.Right : GameAction.Left))
            {
                Launch(samus, forward, 0);
            }
        }
    }

    /// <summary>Launches her the given way.</summary>
    private void Launch(Samus samus, int across, int down)
    {
        _across = across;
        _down = down;
        _acrossSpeed = Pixels.FromNumber(samus.ShinesparkSpeed);
        _riseSpeed = Pixels.Zero;
        _riseGain = Pixels.FromNumber(samus.ShinesparkRiseGain);
        _stage = Stage.Flying;
        FlownFrom = null;
        samus.Room.Session.Sounds.Request(Sounds.Shinespark, channel: "samus", priority: SoundPriority.High);
    }

    /// <summary>Flies one update: she moves, hurts what she touches, pays energy, and crashes if stopped or spent.</summary>
    private void Fly(Samus samus)
    {
        if (samus.Power(HitKinds.Shinespark) is { } power)
            power.IsActive = true;

        // While it flies no hurt colours her or is heard, and its glow is kept going.
        samus.HoldHurtFlashForSpark();
        samus.KeepSparkGlow(samus.SparkGlowUpdates);
        FlownFrom = samus.Position;
        Pixels gravity = Pixels.FromNumber(samus.Medium.Gravity);
        bool stoppedAcross = false, stoppedDown = false;
        if (_across != 0)
        {
            // Across, it gains the pull of gravity each update, up to its top speed, and then moves.
            Pixels faster = _acrossSpeed + gravity;
            Pixels top = Pixels.FromNumber(samus.ShinesparkTopSpeed);
            _acrossSpeed = faster >= top ? top : faster;

            // She moves as she always moves across: over slopes, and through what her body breaks.
            stoppedAcross = samus.MoveAcrossBy(_acrossSpeed * _across).Blocked;
        }

        if (_down != 0)
        {
            // Upward its gain itself grows by gravity each update. Its speed goes on growing, but it rises no more
            // whole pixels in an update than its most.
            _riseGain += gravity;
            _riseSpeed += _riseGain;
            Pixels rise = _riseSpeed.Whole > samus.ShinesparkMostRise ? _riseSpeed.WithWhole(samus.ShinesparkMostRise) : _riseSpeed;

            // The speed it has gathered is her speed upward, which a move across reads on the next update.
            samus.Velocity = samus.Velocity.WithY(_riseSpeed * _down);
            stoppedDown = samus.MoveDownBy(rise * _down, clearsWay: true).Blocked;
        }

        // A diagonal spark slides up a wall it meets, and ends only when it can rise no further.
        bool stopped = _down != 0 ? stoppedDown : stoppedAcross;

        int energy = samus.Inventory.Amount(Amounts.Energy);
        bool spent = energy < samus.ShinesparkLeastEnergy;
        if (!spent)
            samus.Inventory.Lose(Amounts.Energy, 1);
        if (!stopped && !spent)
            return;
        _stage = Stage.Crashed;
        FlownFrom = null;

        // Her boost and her speed end as she crashes. How far the echoes have circled is not started over.
        samus.EndBoost();
        samus.Velocity = PixelVelocity.Zero;
        samus.EndHurtFlash();
        (_crash, _crashRadius) = (Crash.Expanding, 0);
        (_firstAngle, _secondAngle) = samus.Facing == Facing.Left ? (32, 160) : (224, 96);
        samus.Room.Session.Sounds.Request(Sounds.SamusHurt, channel: "samus");
        samus.Room.Session.Sounds.Request(Sounds.ShinesparkEnd, channel: "samus");
        if (samus.ShinesparkEchoShot.Length == 0)
            return;
        ShotType shot = ShotType.Read(samus.Room.Content.ReadData(samus.ShinesparkEchoShot));
        _firstEcho = Echo(samus, shot, _firstAngle);
        _secondEcho = Echo(samus, shot, _secondAngle);
    }

    /// <inheritdoc/>
    public override void Exit(Player owner)
    {
        // Echoes still circling when she is taken out of this some other way go with it.
        if (_stage == Stage.Crashed && _crash != Crash.Holding)
        {
            _firstEcho?.Dismiss(owner.Room);
            _secondEcho?.Dismiss(owner.Room);
        }

        (_firstEcho, _secondEcho) = (null, null);
    }

    /// <summary>
    /// Lets the echoes go as far as her shots' places leave room: the second while a place is free, and the first only
    /// while two are. One let go takes a place until it has left the screen; one with no place is gone. The first, let
    /// go, leaves her height as what the next crash's echoes count their circling on from.
    /// </summary>
    private void LetGo(Samus samus, int firstAngle, int secondAngle)
    {
        if (_firstEcho is not { } first || _secondEcho is not { } second)
            return;
        int free = samus.Weapons.PlacesIn(samus.ShinesparkEchoPool) - samus.Weapons.PlacesHeld(samus.ShinesparkEchoPool);
        if (free >= 2)
        {
            samus.Weapons.HoldPlace(samus.ShinesparkEchoPool, first);
            first.Fly(firstAngle, samus.OrbitTravel);
            samus.OrbitTravel.LeaveHeight(samus.Position.Y.Whole);
        }
        else
        {
            first.Dismiss(samus.Room);
        }

        if (free >= 1)
        {
            samus.Weapons.HoldPlace(samus.ShinesparkEchoPool, second);
            second.Fly(secondAngle, null);
        }
        else
        {
            second.Dismiss(samus.Room);
        }
    }

    private static ShinesparkEcho Echo(Samus samus, ShotType shot, int angle)
    {
        var echo = shot.Launch(samus.Position, PixelVelocity.Zero, HitSide.Player, samus, samus) as ShinesparkEcho
            ?? throw new InvalidOperationException($"The shot \"{samus.ShinesparkEchoShot}\" is not a shinespark echo's.");
        echo.Circle(samus, angle, 0);
        samus.Room.Spawn(echo);
        return echo;
    }

    /// <summary>Plays one update of the crash and returns whether it is over.</summary>
    private bool StepCrash(Samus samus)
    {
        switch (_crash)
        {
            case Crash.Expanding:
                // They are out far enough once they had been before this step.
                bool farEnough = _crashRadius >= CrashCirclingRadius;
                _crashRadius += CrashRadiusStep;
                if (farEnough)
                    _crash = Crash.Circling;
                break;

            case Crash.Circling:
                int turn = samus.Facing == Facing.Left ? -CrashTurn : CrashTurn;
                (_firstAngle, _secondAngle) = (_firstAngle + turn, _secondAngle + turn);
                if (samus.OrbitTravel.Advance())
                    _crash = Crash.Contracting;
                break;

            case Crash.Contracting:
                _crashRadius -= CrashRadiusStep;
                if (_crashRadius <= 0)
                {
                    (_crash, _crashRadius) = (Crash.Holding, 0);
                    _left = Countdown.Of(samus.ShinesparkCrashUpdates);
                }

                break;

            default:
                _left = _left.Stepped();
                if (_left.IsRunning)
                    return false;

                // The echoes are let go along the line the spark flew: to either side of it, or along a diagonal.
                (int first, int second) = _across == 0 ? (64, 192) : _down == 0 ? (0, 128) : _across > 0 ? (224, 96) : (32, 160);
                LetGo(samus, first, second);
                return true;
        }

        _firstEcho?.Circle(samus, _firstAngle, _crashRadius);
        _secondEcho?.Circle(samus, _secondAngle, _crashRadius);
        return false;
    }
}
