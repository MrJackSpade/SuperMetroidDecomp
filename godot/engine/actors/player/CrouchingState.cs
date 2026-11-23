using Engine.Actors.Components;
using Engine.Input;

namespace Engine.Actors;

/// <summary>Crouched on the ground: shorter, and not moving. Down again takes the low form; up stands.</summary>
public sealed class CrouchingState : PlayerState
{
    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.TrySetForm(Player.Crouching);

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        owner.AimFromInput();
        if (!owner.IsOnGround)
            return owner.TrySetForm(Player.Standing) ? new FallingState() : this;
        if (owner.MayCrawl && owner.Input.WasPressed(GameAction.Down))
            return new CrawlingState();
        if (owner.Input.WasPressed(GameAction.Up) && owner.TrySetForm(Player.Standing))
            return new StandingState();
        if (owner.Input.WasPressed(GameAction.Jump) && owner.TrySetForm(Player.Standing))
            return new JumpingState();
        if (owner.Intent != 0)
            owner.Facing = owner.Intent > 0 ? Shared.Facing.Right : Shared.Facing.Left;

        owner.LoseMomentum();
        owner.Slow(owner.Moves(CrouchingMoves));
        owner.MoveAcross();
        return this;
    }
}
