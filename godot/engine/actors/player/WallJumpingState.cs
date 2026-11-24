using Engine.Input;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// Jumping off a wall: the player leaves the wall at a set speed and cannot steer for a moment, then carries on as in
/// any jump.
/// </summary>
public sealed class WallJumpingState : PlayerState
{
    /// <summary>The updates left before the player can steer again.</summary>
    private Countdown _locked;

    /// <summary>
    /// Whether the player can start a wall jump now: the game allows it, they are in the air within reach of a wall,
    /// the jump was just pressed, and, where the game asks it, they are steering away from that wall.
    /// </summary>
    public static bool CanStart(Player owner) => WallToJumpFrom(owner) is not null;

    /// <summary>The side of the wall the player can jump off right now, or null when they cannot.</summary>
    public static Facing? WallToJumpFrom(Player owner)
    {
        if (!owner.MayWallJump || !owner.Input.WasPressed(GameAction.Jump) || owner.IsOnGround)
            return null;
        foreach (Facing side in new[] { Facing.Left, Facing.Right })
        {
            if (owner.WallJumpNeedsSteeringAway && owner.Intent != (side == Facing.Left ? 1 : -1))
                continue;
            if (owner.IsWithinReachOfWall(side, owner.WallJumpReach))
                return side;
        }

        return null;
    }

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        Facing wall = owner.IsWithinReachOfWall(Facing.Left, owner.WallJumpReach) ? Facing.Left : Facing.Right;
        if (owner.WallJumpNeedsSteeringAway && owner.Intent != 0)
            wall = owner.Intent > 0 ? Facing.Left : Facing.Right;
        Facing away = wall.Opposite();
        owner.TrySetForm(owner.FormOr(Player.Jumping), aroundMiddle: true);
        owner.Facing = away;
        owner.Velocity = new PixelVelocity(
            away.Toward(Pixels.FromNumber(owner.WallJumpAcrossSpeed)), -owner.LaunchSpeed(offWall: true));
        _locked = Countdown.Of(owner.WallJumpLockUpdates);
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        JumpingState.CutShort(owner);
        owner.StepDash(building: false);
        if (owner.WallJumpSteered)
            owner.Steer(owner.Moves(WallJumpingMoves));
        owner.MoveAcross();
        owner.MoveDown();
        owner.Fall();
        _locked = _locked.Stepped();
        return null;
    }

    /// <inheritdoc/>
    public override PlayerState? Ended(Player owner)
    {
        if (_locked.IsRunning)
            return null;
        return owner.Velocity.Y.IsNegative ? new JumpingState(keepsSpeed: true) : new FallingState();
    }
}
