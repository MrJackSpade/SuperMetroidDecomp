using Engine.Actors.Components;
using Engine.Input;

namespace Engine.Actors;

/// <summary>Running along the ground. Holding the dash action builds a dash on top of the run.</summary>
public sealed class RunningState : PlayerState
{
    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.TrySetForm(Player.Standing);

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        owner.AimFromInput();
        if (!owner.IsOnGround)
            return new FallingState();
        if (owner.Input.WasPressed(GameAction.Jump))
            return new JumpingState();
        if (owner.MayCrouch && owner.CrouchWhileRunning && owner.Input.WasPressed(GameAction.Down))
            return new CrouchingState();
        bool turning = owner.Intent != 0 && (owner.Intent > 0) != (owner.Facing == Shared.Facing.Right);
        if (turning && owner.TurnUpdates > 0)
            return new TurningState();

        // A dash builds only while running freely: not while a liquid hinders the player.
        owner.StepDash(owner.Intent != 0 && owner.DashIsHeld && !owner.IsHinderedByLiquid);
        owner.Steer(owner.Moves(RunningMoves));
        bool stopped = owner.Intent == 0 && owner.Velocity.X.IsZero;
        if (stopped)
            owner.LoseMomentum();
        owner.MoveAcross();
        if (!owner.IsOnGround)
            return new FallingState();
        return stopped ? new StandingState() : this;
    }
}
