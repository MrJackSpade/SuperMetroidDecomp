using Engine.Actors.Components;
using Engine.Input;

namespace Engine.Actors;

/// <summary>Standing upright on the ground, at rest or coming to rest.</summary>
public sealed class StandingState : PlayerState
{
    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.TrySetForm(BodyForm.Standing);

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        owner.AimFromInput();
        if (!owner.IsOnGround)
            return new FallingState();
        if (owner.Input.WasPressed(GameAction.Jump))
            return new JumpingState();
        if (owner.CanCrouch && owner.Input.WasPressed(GameAction.Down))
            return new CrouchingState();
        if (owner.Intent != 0)
        {
            bool turning = (owner.Intent > 0) != (owner.Facing == Shared.Facing.Right);
            return turning && owner.TurnUpdates > 0 ? new TurningState() : new RunningState();
        }

        owner.SlowToRest();
        owner.MoveAcross();
        return this;
    }
}
