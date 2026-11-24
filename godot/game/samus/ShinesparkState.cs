using Engine.Actors;
using Engine.Collision;
using Engine.Input;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// A shinespark: with a shine stored, a jump taken up aiming level with her arm at rest, up or diagonally up winds
/// her up and then launches her in a straight line, across, up or diagonally, faster and faster, hurting what she
/// passes through and costing her energy as she goes. It ends against something solid, or when her energy runs low.
/// There she is held while two echoes of her circle out, round and back in; then, after a moment, the echoes are let
/// go and fly off, and she is free.
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

    /// <summary>How far the echoes move out or in each update, how far out they circle from, how far each turns an update, and how far they circle in all.</summary>
    private const int CrashRadiusStep = 4, CrashCirclingRadius = 12, CrashTurn = 4, CrashTravel = 128;

    /// <summary>Whether the windup starts lifted, as one begun out of a jump does.</summary>
    private readonly bool _lifted;

    /// <summary>A shinespark.</summary>
    /// <param name="lifted">Whether its windup starts lifted off where she is, as one begun out of a jump does.</param>
    public ShinesparkState(bool lifted) => _lifted = lifted;

    private Stage _stage;
    private Crash _crash;
    private int _crashRadius;
    private int _crashTravelled;
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

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        var samus = (Samus)owner;
        samus.SpendShine();
        owner.StopAcross();
        owner.Velocity = PixelVelocity.Zero;
        owner.Weapons.DropCharge();

        // The lift moves her as she is, whatever is above her, as the cartridge's does.
        if (_lifted)
            owner.Position += PixelOffset.FromWhole(0, -samus.ShinesparkLift);
        owner.TrySetForm(owner.FormOr(Player.Jumping), aroundMiddle: true);
        samus.BoostFully();
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
                if (!StepCrash(samus))
                    return null;
                samus.EndBoost();
                return owner.StateAtRest();
            default:
                throw new InvalidOperationException($"Undefined shinespark stage {_stage}.");
        }
    }

    /// <summary>
    /// Waits for a direction. Steering launches her across; an angle action, or up with steering, diagonally; up
    /// alone, or the windup running out, straight up.
    /// </summary>
    private void WindUp(Samus samus)
    {
        GameInput input = samus.Input;
        int forward = samus.Facing == Facing.Right ? 1 : -1;
        if (input.IsHeld(Actions.AngleUp) || (input.IsHeld(GameAction.Up) && samus.Intent != 0))
        {
            Launch(samus, samus.Intent != 0 ? samus.Intent : forward, -1);
            return;
        }

        if (samus.Intent != 0)
        {
            Launch(samus, samus.Intent, 0);
            return;
        }

        _left = _left.Stepped();
        if (input.IsHeld(GameAction.Up) || _left.IsOver)
            Launch(samus, 0, -1);
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
        if (across != 0)
            samus.Facing = across > 0 ? Facing.Right : Facing.Left;
        samus.Room.Session.Sounds.Request(Sounds.Shinespark, channel: "samus");
    }

    /// <summary>Flies one update: she moves, hurts what she touches, pays energy, and crashes if stopped or spent.</summary>
    private void Fly(Samus samus)
    {
        if (samus.Power(HitKinds.Shinespark) is { } power)
            power.IsActive = true;

        MoveOptions options = MoveOptions.Plain with { Slopes = SlopeTreatment.Wall };
        Pixels gravity = Pixels.FromNumber(samus.Medium.Gravity);
        bool stoppedAcross = false, stoppedDown = false;
        if (_across != 0)
        {
            // Across, it gains the pull of gravity each update, up to its top speed, and then moves.
            Pixels faster = _acrossSpeed + gravity;
            Pixels top = Pixels.FromNumber(samus.ShinesparkTopSpeed);
            _acrossSpeed = faster >= top ? top : faster;
            stoppedAcross = samus.Room.Mover.MoveAcross(samus.Body, _acrossSpeed * _across, options).Blocked;
        }

        if (_down != 0)
        {
            // Upward its gain itself grows by gravity each update. Its speed goes on growing, but it rises no more
            // whole pixels in an update than its most.
            _riseGain += gravity;
            _riseSpeed += _riseGain;
            Pixels rise = _riseSpeed.Whole > samus.ShinesparkMostRise ? _riseSpeed.WithWhole(samus.ShinesparkMostRise) : _riseSpeed;
            stoppedDown = samus.Room.Mover.MoveDown(samus.Body, rise * _down, options).Blocked;
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
        (_crash, _crashRadius, _crashTravelled) = (Crash.Expanding, 0, 0);
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
                _crashTravelled += CrashTurn;
                if (_crashTravelled >= CrashTravel)
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
                _firstEcho?.Fly(first);
                _secondEcho?.Fly(second);
                return true;
        }

        _firstEcho?.Circle(samus, _firstAngle, _crashRadius);
        _secondEcho?.Circle(samus, _secondAngle, _crashRadius);
        return false;
    }
}
