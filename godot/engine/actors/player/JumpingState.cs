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
    internal JumpingState(bool keepsSpeed) => _keepsSpeed = keepsSpeed;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.TrySetForm(BodyForm.Standing);
        if (!_keepsSpeed)
            owner.Velocity = owner.Velocity.WithY(-Pixels.FromNumber(owner.JumpSpeed));
    }

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        owner.AimFromInput();
        if (WallJumpingState.CanStart(owner))
            return new WallJumpingState();

        Pixels cut = -Pixels.FromNumber(owner.JumpCutSpeed);
        if (!owner.Input.IsHeld(GameAction.Jump) && owner.Velocity.Y < cut)
            owner.Velocity = owner.Velocity.WithY(cut);

        owner.Steer(owner.AirAcceleration, owner.RunSpeed);
        owner.MoveAcross();
        owner.MoveDown();
        owner.Fall();
        return owner.Velocity.Y.IsNegative ? this : new FallingState();
    }
}
