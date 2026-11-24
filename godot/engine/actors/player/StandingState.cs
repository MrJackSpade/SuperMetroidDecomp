using Engine.Input;

namespace Engine.Actors;

/// <summary>
/// Standing upright on the ground. Standing keeps none of the player's speed across: on each update a dash's speed
/// alone still carries them, and then everything across is at rest.
/// </summary>
public sealed class StandingState : PlayerState
{
    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.TrySetForm(Player.Standing);

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        owner.AimFromInput();
        if (owner.JumpPressed)
            return new JumpingState();
        if (owner.MayCrouch && owner.Input.WasPressed(GameAction.Down))
            return new CrouchingState();
        if (owner.Intent == 0)
            return owner.Aim == Shared.Aim.Forward ? null : this;
        if ((owner.Intent > 0) == (owner.Facing == Shared.Facing.Right))
            return new RunningState();
        if (owner.TurnUpdates > 0)
            return new TurningState();

        // A player who turns at once is already running the other way.
        owner.TurnAround();
        return new RunningState();
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        owner.ComeToRest();
        owner.MoveAcross();
        owner.StopAcross();
        return owner.IsOnGround ? null : new FallingState();
    }
}
