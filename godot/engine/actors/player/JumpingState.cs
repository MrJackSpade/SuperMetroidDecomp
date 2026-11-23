using Engine.Actors.Components;
using Engine.Input;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>Rising in a jump. Letting go of the jump early cuts it short; at its top the player starts to fall.</summary>
public sealed class JumpingState : PlayerState
{
    /// <summary>Whether the jump keeps the upward speed the player already has, as a wall jump's does.</summary>
    private readonly bool _keepsSpeed;

    /// <summary>A jump from the ground.</summary>
    public JumpingState()
    {
    }

    /// <summary>A jump that carries on with the upward speed the player already has.</summary>
    public JumpingState(bool keepsSpeed) => _keepsSpeed = keepsSpeed;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.TrySetForm(owner.FormOr(Player.Jumping), aroundMiddle: true);
        if (!_keepsSpeed)
            owner.Velocity = owner.Velocity.WithY(-owner.LaunchSpeed(offWall: false));
    }

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        owner.AimFromInput();
        if (owner.WallJumpFromAnyJump && WallJumpingState.CanStart(owner))
            return new WallJumpingState();

        CutShort(owner);
        owner.StepDash(building: false);
        owner.Steer(owner.Moves(JumpingMoves));
        owner.MoveAcross();
        owner.MoveDown();
        owner.Fall();
        return owner.Velocity.Y.IsNegative ? this : new FallingState();
    }

    /// <summary>Cuts a rising jump to the cut speed once the jump action is no longer held.</summary>
    public static void CutShort(Player owner)
    {
        Pixels cut = -Pixels.FromNumber(owner.JumpCutSpeed);
        if (!owner.Input.IsHeld(GameAction.Jump) && owner.Velocity.Y < cut)
            owner.Velocity = owner.Velocity.WithY(cut);
    }
}
