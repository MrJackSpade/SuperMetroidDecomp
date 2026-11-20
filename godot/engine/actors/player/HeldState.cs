using Engine.Actors.Components;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>Held in place by something: the player does not move or act until whatever holds them lets go.</summary>
public sealed class HeldState : PlayerState
{
    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.Velocity = PixelVelocity.Zero;

    /// <inheritdoc/>
    public override State<Player> Step(Player owner) => this;
}
