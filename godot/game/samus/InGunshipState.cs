using Engine.Actors;
using Engine.Actors.Components;
using Engine.Shared;

namespace Game;

/// <summary>
/// In her gunship, or on her way into or out of it: she faces forward, does nothing, and goes where the ship puts
/// her until it lets her go.
/// </summary>
public sealed class InGunshipState : PlayerState, ISamusLook
{
    /// <inheritdoc/>
    public string Look => "facing_forward";

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.Velocity = PixelVelocity.Zero;
        owner.LoseMomentum();
        owner.Weapons.DropCharge();
        owner.TrySetForm(Player.Standing);
    }

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        owner.Velocity = PixelVelocity.Zero;
        return this;
    }
}
