using Engine.Collision;

namespace Engine.Actors;

/// <summary>Falling through the air, until landing.</summary>
public sealed class FallingState : PlayerState
{
    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.TrySetForm(owner.FormOr(Player.Falling), aroundMiddle: true);

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        owner.AimFromInput();
        return this;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        if (owner.WallJumpFromAnyJump && WallJumpingState.CanStart(owner))
            return new WallJumpingState();

        owner.StepDash(building: false);
        owner.Steer(owner.Moves(FallingMoves));
        owner.MoveAcross();
        // The fall moves by the speed it had, and gravity adds to it afterwards: a fall from rest moves nothing in its
        // first update.
        MoveResult down = owner.MoveDown();
        owner.Fall();
        if (down.Blocked || owner.IsOnGround)
            return owner.Intent != 0 ? new RunningState() : new StandingState();
        return null;
    }
}
