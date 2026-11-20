using Engine.Actors.Components;
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
    /// Whether the player can start a wall jump now: the game allows it, they are in the air against a wall, and the
    /// jump was just pressed.
    /// </summary>
    internal static bool CanStart(Player owner) =>
        owner.CanWallJump && owner.Input.WasPressed(GameAction.Jump) && !owner.IsOnGround &&
        (owner.IsAgainstWall(Facing.Left) || owner.IsAgainstWall(Facing.Right));

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        Facing away = owner.IsAgainstWall(Facing.Left) ? Facing.Right : Facing.Left;
        owner.Facing = away;
        owner.Velocity = new PixelVelocity(
            away.Toward(Pixels.FromNumber(owner.WallJumpAcrossSpeed)), -Pixels.FromNumber(owner.WallJumpUpSpeed));
        _locked = Countdown.Of(owner.WallJumpLockUpdates);
    }

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        owner.MoveAcross();
        owner.MoveDown();
        owner.Fall();
        _locked = _locked.Stepped();
        if (_locked.IsRunning)
            return this;
        return owner.Velocity.Y.IsNegative ? new JumpingState(keepsSpeed: true) : new FallingState();
    }
}
