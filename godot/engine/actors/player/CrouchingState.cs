using Engine.Actors.Components;
using Engine.Input;

namespace Engine.Actors;

/// <summary>Crouched on the ground: shorter, and not moving. Down again takes the low form; up stands.</summary>
public sealed class CrouchingState : PlayerState
{
    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.TrySetForm(BodyForm.Crouching);

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        owner.AimFromInput();
        if (!owner.IsOnGround)
            return owner.TrySetForm(BodyForm.Standing) ? new FallingState() : this;
        if (owner.CanCrawl && owner.Input.WasPressed(GameAction.Down))
            return new CrawlingState();
        if (owner.Input.WasPressed(GameAction.Up) && owner.TrySetForm(BodyForm.Standing))
            return new StandingState();
        if (owner.Input.WasPressed(GameAction.Jump) && owner.TrySetForm(BodyForm.Standing))
            return new JumpingState();
        if (owner.Intent != 0)
            owner.Facing = owner.Intent > 0 ? Shared.Facing.Right : Shared.Facing.Left;

        owner.SlowToRest();
        owner.MoveAcross();
        return this;
    }
}
