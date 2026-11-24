using Engine.Actors;
using Engine.Shared;

namespace Game;

/// <summary>
/// Facing forward and still, in the hands of something else: in her gunship or on her way into or out of it, or on
/// the pad that sets her down in Ceres. She does nothing and goes where she is put until she is let go.
/// </summary>
public sealed class FacingForwardState : PlayerState, ISamusLook
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
    public override PlayerState? Move(Player owner)
    {
        owner.Velocity = PixelVelocity.Zero;
        return null;
    }
}
