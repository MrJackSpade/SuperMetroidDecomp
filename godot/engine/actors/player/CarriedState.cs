using Engine.Shared;

namespace Engine.Actors;

/// <summary>Carried along by something: the player goes where they are taken, and does not act, until they are let go.</summary>
public sealed class CarriedState : PlayerState
{
    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override bool InAnothersHands => true;

    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.Velocity = PixelVelocity.Zero;

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner) => null;
}
