using Engine.Input;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// Crouched on the ground: shorter, and keeping no speed across. Down again takes the low form; up stands; a press of
/// the direction behind turns the player round, still crouched; the direction ahead held stands them at once. A
/// stand asked for where there is no room to stand is refused, and the player stays as they are: it was asked all
/// the same, so nothing else is asked that update and their momentum is not ended.
/// </summary>
public sealed class CrouchingState : PlayerState
{
    /// <inheritdoc/>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(Player.Crouching);

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        GameInput input = owner.Input;
        bool up = input.WasPressed(GameAction.Up);

        // A turn is made aiming as the player aimed before it: what is held with the way behind aims nothing.
        if (!up && input.WasPressed(owner.Facing == Facing.Right ? GameAction.Left : GameAction.Right))
            return new TurningState(crouched: true);
        owner.AimFromInput();
        if (up)
            return owner.CanTakeForm(Player.Standing) ? owner.StandUp() : PlainCrouch(owner);
        if (owner.MayCrawl && input.WasPressed(GameAction.Down))
            return owner.TakeLowForm();
        if (owner.JumpPressed)
            return owner.CanTakeForm(owner.FormOr(Player.Jumping), aroundMiddle: true) ? new JumpingState() : PlainCrouch(owner);
        if (input.IsHeld(owner.Facing == Facing.Right ? GameAction.Right : GameAction.Left))
            return owner.CanTakeForm(Player.Standing) ? new StandingState() : PlainCrouch(owner);
        return owner.Aim == Aim.Forward ? null : this;
    }

    /// <summary>
    /// What a taller form asked for that has no room leaves: the plain crouch. The player stays crouched with their
    /// aim level for the update, whatever aim the input asked for, as the cartridge picks the taller pose and falls
    /// back to its plain crouching pose.
    /// </summary>
    private PlayerState PlainCrouch(Player owner)
    {
        owner.Aim = Aim.Forward;
        return this;
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
