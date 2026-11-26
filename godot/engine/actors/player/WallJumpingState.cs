using Engine.Input;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// Jumping off a wall: the player leaves it with no speed of their own across, facing as they did, gains speed by the
/// wall jump's own move values without having to steer, and carries on so for as long as the jump is held. The game's
/// player says when a wall has been found to jump from, and enters it.
/// </summary>
public sealed class WallJumpingState : PlayerState
{
    /// <summary>
    /// Whether the body of a wall jump fits where the player is. A wall jump with no room for its body does not
    /// happen: the player goes on as they were.
    /// </summary>
    public static bool HasRoom(Player owner) => owner.CanTakeForm(owner.FormOr(Player.Jumping), aroundMiddle: true);

    /// <inheritdoc/>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(owner.FormOr(Player.Jumping), AroundMiddle: true);

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        // The player keeps the way they face and whatever dash carries them; their own speed starts from rest.
        owner.ComeToRest();
        owner.Rise(owner.LaunchSpeed(offWall: true, owner.Form));
    }

    /// <summary>It lasts while the jump is held.</summary>
    public override PlayerState? Asked(Player owner) => owner.Input.IsHeld(GameAction.Jump) ? this : null;

    /// <inheritdoc/>
    public override PlayerState? Unasked(Player owner) => new JumpingState(keepsSpeed: true);

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        JumpingState.CutShort(owner);
        owner.StepDash(building: false);
        // A wall jump gains speed across whether or not a direction is held.
        owner.SteerInAir(owner.Moves(WallJumpingMoves));
        if (owner.Steering == Steering.Steered)
            owner.Steering = Steering.Unsteered;
        owner.MoveAcross();
        return AirStance.FallAndLand(owner, this);
    }

    /// <summary>Left and right held together are read as the way behind.</summary>
    public override int ReadsBothAcross(Player owner) => -Forward(owner);
}
