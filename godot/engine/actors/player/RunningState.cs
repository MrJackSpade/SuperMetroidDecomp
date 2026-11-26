using Engine.Collision;
using Engine.Input;

namespace Engine.Actors;

/// <summary>
/// Running along the ground. Holding the dash action builds a dash on top of the run. A runner whose input asks for
/// nothing goes on running unsteered, slowing, and stands once at rest; one who only aims stands at once.
/// </summary>
public sealed class RunningState : PlayerState
{
    /// <summary>Whether the player had any speed of their own as this update's input was asked.</summary>
    private bool _hadSpeed;

    /// <inheritdoc/>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(Player.Standing);

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        owner.AimFromInput();
        _hadSpeed = !owner.Velocity.X.IsZero;
        if (owner.CanCrouch && owner.CrouchWhileRunning && owner.Input.WasPressed(GameAction.Down))
            return owner.Crouch();
        if (owner.JumpPressed)
            return new JumpingState();
        if (owner.Intent == 0)
            return owner.Aim == Shared.Aim.Forward ? null : new StandingState();
        if ((owner.Intent > 0) == (owner.Facing == Shared.Facing.Right))
            return this;
        if (owner.TurnUpdates > 0)
            return new TurningState();
        owner.TurnAround();
        return this;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        // A dash builds only while running freely: not while a liquid hinders the player.
        owner.StepDash(owner.DashIsHeld && !owner.IsHinderedByLiquid);
        owner.Steer(owner.Moves(RunningMoves));
        MoveResult across = owner.MoveAcross();
        if (!owner.PressToGround())
            return new FallingState(offFloor: true);

        // A wall has already brought everything across to rest: the runner stands against it.
        return across.Blocked ? owner.StoppedByWall() : null;
    }

    /// <summary>
    /// With nothing asked of them, a runner who had speed stays running: the dash's speed is added into their own and
    /// lost, their momentum ends, and they slow, unsteered, until at rest. One who had none stands.
    /// </summary>
    public override PlayerState? Unasked(Player owner)
    {
        if (!_hadSpeed)
        {
            owner.Steering = Steering.Steered;
            owner.EndMomentum();
            return new StandingState();
        }

        if (owner.Velocity.X.IsZero)
        {
            owner.Steering = Steering.Steered;
        }
        else
        {
            owner.FoldDash();
            owner.Steering = Steering.Unsteered;
        }

        owner.EndMomentum();
        return null;
    }

    /// <summary>Left and right held together are read as the way ahead: the run goes on.</summary>
    public override int ReadsBothAcross(Player owner) => Forward(owner);
}
