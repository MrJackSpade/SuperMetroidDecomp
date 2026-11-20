using Engine.Actors.Components;
using Engine.Collision;

namespace Engine.Actors;

/// <summary>Falling through the air, until landing.</summary>
public sealed class FallingState : PlayerState
{
    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        owner.AimFromInput();
        if (WallJumpingState.CanStart(owner))
            return new WallJumpingState();

        owner.Fall();
        owner.Steer(owner.AirAcceleration, owner.RunSpeed);
        owner.MoveAcross();
        MoveResult down = owner.MoveDown();
        if (down.Blocked || owner.IsOnGround)
            return owner.Intent != 0 ? new RunningState() : new StandingState();
        return this;
    }
}
