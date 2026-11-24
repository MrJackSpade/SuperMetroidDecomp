using Engine.Input;

namespace Engine.Actors;

/// <summary>Running along the ground. Holding the dash action builds a dash on top of the run.</summary>
public sealed class RunningState : PlayerState
{
    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.TrySetForm(Player.Standing);

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        owner.AimFromInput();
        if (owner.Input.WasPressed(GameAction.Jump))
            return new JumpingState();
        if (owner.MayCrouch && owner.CrouchWhileRunning && owner.Input.WasPressed(GameAction.Down))
            return new CrouchingState();
        if (owner.Intent == 0)
            return null;
        bool turning = (owner.Intent > 0) != (owner.Facing == Shared.Facing.Right);
        return turning && owner.TurnUpdates > 0 ? new TurningState() : this;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        // A dash builds only while running freely: not while a liquid hinders the player.
        owner.StepDash(owner.Intent != 0 && owner.DashIsHeld && !owner.IsHinderedByLiquid);
        owner.Steer(owner.Moves(RunningMoves));
        owner.MoveAcross();
        return owner.IsOnGround ? null : new FallingState();
    }

    /// <summary>With nothing asked of them, a runner who has come to rest stands.</summary>
    public override PlayerState? Unasked(Player owner)
    {
        if (!owner.Velocity.X.IsZero)
            return null;
        owner.LoseMomentum();
        return new StandingState();
    }
}
