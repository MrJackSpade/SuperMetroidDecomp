using Engine.Input;

namespace Engine.Actors;

/// <summary>Standing upright on the ground, at rest or coming to rest.</summary>
public sealed class StandingState : PlayerState
{
    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.TrySetForm(Player.Standing);

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        owner.AimFromInput();
        if (owner.Input.WasPressed(GameAction.Jump))
            return new JumpingState();
        if (owner.MayCrouch && owner.Input.WasPressed(GameAction.Down))
            return new CrouchingState();
        if (owner.Intent == 0)
            return null;
        bool turning = (owner.Intent > 0) != (owner.Facing == Shared.Facing.Right);
        return turning && owner.TurnUpdates > 0 ? new TurningState() : new RunningState();
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        // Standing still ends any momentum a dash had built.
        owner.LoseMomentum();
        owner.Slow(owner.Moves(StandingMoves));
        owner.MoveAcross();
        return owner.IsOnGround ? null : new FallingState();
    }
}
