using Engine.Input;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// Crouched on the ground: shorter, and keeping no speed across. Down again takes the low form; up stands; a press of
/// the direction behind turns the player round, still crouched; the direction ahead held stands them at once.
/// </summary>
public sealed class CrouchingState : PlayerState
{
    /// <inheritdoc/>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(Player.Crouching);

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        owner.AimFromInput();
        GameInput input = owner.Input;
        if (input.WasPressed(GameAction.Up) && owner.CanTakeForm(Player.Standing))
            return owner.StandUp();
        if (input.WasPressed(owner.Facing == Facing.Right ? GameAction.Left : GameAction.Right))
            return new TurningState(crouched: true);
        if (owner.MayCrawl && input.WasPressed(GameAction.Down))
            return owner.TakeLowForm();
        if (owner.JumpPressed && owner.CanTakeForm(owner.FormOr(Player.Jumping), aroundMiddle: true))
            return new JumpingState();
        if (input.IsHeld(owner.Facing == Facing.Right ? GameAction.Right : GameAction.Left) && owner.CanTakeForm(Player.Standing))
            return new StandingState();
        return owner.Aim == Aim.Forward ? null : this;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        // A crouch keeps no speed across: a dash still carries the player this update, and is then spent. Their
        // momentum is left for the input to end.
        owner.ComeToRest();
        owner.MoveAcross();
        bool grounded = owner.PressToGround();
        owner.ClearSpeedAcross();
        bool falls = !grounded && owner.CanTakeForm(owner.FormOr(Player.Falling), aroundMiddle: true);
        return falls ? new FallingState(offFloor: true) : null;
    }

    /// <summary>With nothing asked, the momentum a crouch was still holding ends.</summary>
    public override PlayerState? Unasked(Player owner)
    {
        owner.Steering = Steering.Steered;
        owner.EndMomentum();
        return null;
    }

    /// <summary>Left and right held together are read as the way ahead, which stands the player up.</summary>
    public override int ReadsBothAcross(Player owner) => Forward(owner);
}
