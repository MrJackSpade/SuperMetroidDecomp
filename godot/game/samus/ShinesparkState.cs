using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Input;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// A shinespark: with a shine stored, a jump winds her up and then launches her in a straight line, across, up or
/// diagonally, faster and faster, hurting what she passes through and costing her energy as she goes. It ends against
/// something solid, or when her energy runs low, and holds her there for a moment.
/// </summary>
public sealed class ShinesparkState : PlayerState, IUntouchable, ISamusLook
{
    private enum Stage
    {
        WindingUp,
        Flying,
        Crashed,
    }

    private Stage _stage;
    private Countdown _left;
    private int _across;
    private int _down;
    private Pixels _speed;

    /// <inheritdoc/>
    public string Look => _stage switch
    {
        Stage.WindingUp => "shinespark_windup",
        Stage.Crashed => "shinespark_crash",
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
        owner.TrySetForm(owner.FormOr(Player.Jumping), aroundMiddle: true);
        samus.BoostFully();
        _stage = Stage.WindingUp;
        _left = Countdown.Of(samus.ShinesparkWindupUpdates);
    }

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        var samus = (Samus)owner;
        switch (_stage)
        {
            case Stage.WindingUp:
                WindUp(samus);
                return this;
            case Stage.Flying:
                Fly(samus);
                return this;
            case Stage.Crashed:
                _left = _left.Stepped();
                if (_left.IsRunning)
                    return this;
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
        _speed = Pixels.FromNumber(samus.ShinesparkSpeed);
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
        bool stoppedAcross = _across != 0 && samus.Room.Mover.MoveAcross(samus.Body, _speed * _across, options).Blocked;
        bool stoppedDown = _down != 0 && samus.Room.Mover.MoveDown(samus.Body, _speed * _down, options).Blocked;
        // A diagonal spark slides up a wall it meets, and ends only when it can rise no further.
        bool stopped = _down != 0 ? stoppedDown : stoppedAcross;

        Pixels faster = _speed + Pixels.FromNumber(samus.ShinesparkAcceleration);
        Pixels top = Pixels.FromNumber(samus.ShinesparkTopSpeed);
        _speed = faster > top ? top : faster;

        int energy = samus.Inventory.Amount(Amounts.Energy);
        bool spent = energy < samus.ShinesparkLeastEnergy;
        if (!spent)
            samus.Inventory.Lose(Amounts.Energy, 1);
        if (!stopped && !spent)
            return;
        _stage = Stage.Crashed;
        _left = Countdown.Of(samus.ShinesparkCrashUpdates);
        samus.Room.Session.Sounds.Request(Sounds.ShinesparkEnd, channel: "samus");
    }
}
