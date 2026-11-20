using Engine.Actors.Components;
using Engine.Input;

namespace Engine.Actors;

/// <summary>Running along the ground.</summary>
public sealed class RunningState : PlayerState
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
        if (owner.MayCrouch && owner.Input.WasPressed(GameAction.Down))
            return new CrouchingState();

        owner.Steer(owner.RunAcceleration, owner.RunSpeed);
        owner.MoveAcross();
        if (!owner.IsOnGround)
            return new FallingState();
        return owner.Intent == 0 && owner.Velocity.X.IsZero ? new StandingState() : this;
    }
}
