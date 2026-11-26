using Engine.Input;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// Jumping off a wall. In the plain kind the player leaves the wall at a set speed and cannot steer for a moment, then
/// carries on as in any jump. In the steered kind they leave it with no speed across, gain speed by the wall jump's
/// own move values without having to steer, and carry on so for as long as the jump is held.
/// </summary>
public sealed class WallJumpingState : PlayerState
{
    /// <summary>The updates left before the player can steer again, in the plain kind.</summary>
    private Countdown _locked;

    /// <summary>
    /// Whether the player can start a wall jump now: the game allows it, they are in the air within reach of a wall,
    /// the jump was just pressed, and, where the game asks it, they are steering away from that wall.
    /// </summary>
    public static bool CanStart(Player owner) => WallToJumpFrom(owner) is not null;

    /// <summary>The side of the wall the player can jump off right now, or null when they cannot.</summary>
    public static Facing? WallToJumpFrom(Player owner) =>
        owner.MayWallJump && owner.JumpPressed && !owner.IsOnGround ? WallInReach(owner) : null;

    /// <summary>
    /// The side of the wall within reach, or null when there is none: where the game asks it, only the wall on the
    /// side away from the direction held.
    /// </summary>
    public static Facing? WallInReach(Player owner)
    {
        foreach (Facing side in new[] { Facing.Right, Facing.Left })
        {
            if (owner.WallJumpNeedsSteeringAway && !owner.Input.IsHeld(side == Facing.Left ? GameAction.Right : GameAction.Left))
                continue;
            if (owner.IsWithinReachOfWall(side, owner.WallJumpReach))
                return side;
        }

        return null;
    }

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.TrySetForm(owner.FormOr(Player.Jumping), aroundMiddle: true);
        if (owner.WallJumpSteered)
        {
            // The player keeps the way they face and whatever dash carries them; their own speed starts from rest.
            owner.ComeToRest();
        }
        else
        {
            Facing away = (WallInReach(owner) ?? owner.Facing).Opposite();
            owner.Facing = away;
            owner.Steering = Steering.Steered;
            owner.Velocity = owner.Velocity.WithX(away.Toward(Pixels.FromNumber(owner.WallJumpAcrossSpeed)));
            _locked = Countdown.Of(owner.WallJumpLockUpdates);
        }

        owner.Rise(owner.LaunchSpeed(offWall: true, owner.Form));
    }

    /// <summary>The steered kind lasts while the jump is held.</summary>
    public override PlayerState? Asked(Player owner) => !owner.WallJumpSteered || owner.Input.IsHeld(GameAction.Jump) ? this : null;

    /// <inheritdoc/>
    public override PlayerState? Unasked(Player owner) => new JumpingState(keepsSpeed: true);

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        JumpingState.CutShort(owner);
        owner.StepDash(building: false);
        if (owner.WallJumpSteered)
        {
            // A wall jump gains speed across whether or not a direction is held.
            owner.SteerInAir(owner.Moves(WallJumpingMoves));
            if (owner.Steering == Steering.Steered)
                owner.Steering = Steering.Unsteered;
        }

        owner.MoveAcross();
        _locked = _locked.Stepped();
        return AirStance.FallAndLand(owner, this);
    }

    /// <inheritdoc/>
    public override PlayerState? Ended(Player owner) =>
        owner.WallJumpSteered || _locked.IsRunning ? null : new JumpingState(keepsSpeed: true);
}
