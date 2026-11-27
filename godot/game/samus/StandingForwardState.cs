using Engine.Actors;
using Engine.Input;
using Engine.Shared;

namespace Game;

/// <summary>
/// Standing in the front view in her own hands, as an elevator leaves her: she stays so, moving nothing and firing
/// nothing, until right or left is held, which turns her to face that way, right before left. Nothing else she
/// presses changes it. She makes no step down in it, so she does not stand at an elevator's door.
/// </summary>
/// <param name="look">
/// The look she stands in: the front view's own, or the one she is already shown in, which then goes on unbroken, as
/// the last picture of her appearance does where a saved game is taken up.
/// </param>
public sealed class StandingForwardState(string look = "facing_forward") : PlayerState, ISamusLook
{
    /// <inheritdoc/>
    public string Look => look;

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.Velocity = PixelVelocity.Zero;
        owner.TrySetForm(RidingElevatorState.Form);
    }

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        bool right = owner.Input.IsHeld(GameAction.Right);
        if (!right && !owner.Input.IsHeld(GameAction.Left))
            return this;

        // The turn begins from the other way, in the upright body, and ends facing the way held. The upright body is
        // taken about her middle, as the cartridge only changes her body's size there: her feet are left above the
        // floor by the difference, and the turn's own press to the ground brings her down as it goes.
        owner.Facing = right ? Facing.Left : Facing.Right;
        owner.TrySetForm(Player.Standing, aroundMiddle: true);
        return new TurningState();
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner) => null;
}
