using Engine.Input;

namespace Engine.Actors;

/// <summary>Crouched on the ground: shorter, and not moving. Down again takes the low form; up stands.</summary>
public sealed class CrouchingState : PlayerState
{
    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.TrySetForm(Player.Crouching);

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        owner.AimFromInput();
        if (owner.MayCrawl && owner.Input.WasPressed(GameAction.Down))
            return new CrawlingState();
        if (owner.Input.WasPressed(GameAction.Up) && owner.CanTakeForm(Player.Standing))
            return new StandingState();
        if (owner.JumpPressed && owner.CanTakeForm(owner.FormOr(Player.Jumping), aroundMiddle: true))
            return new JumpingState();
        if (owner.Intent != 0)
            owner.Facing = owner.Intent > 0 ? Shared.Facing.Right : Shared.Facing.Left;
        return this;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        owner.ComeToRest();
        owner.MoveAcross();
        owner.StopAcross();
        bool falls = !owner.IsOnGround && owner.CanTakeForm(owner.FormOr(Player.Falling), aroundMiddle: true);
        return falls ? new FallingState() : null;
    }
}
