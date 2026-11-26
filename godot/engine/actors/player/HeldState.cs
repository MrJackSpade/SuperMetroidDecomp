using Engine.Shared;

namespace Engine.Actors;

/// <summary>Held in place by something: the player does not move or act until whatever holds them lets go.</summary>
/// <param name="was">What the player was doing as they were taken hold of.</param>
public sealed class HeldState(PlayerState was) : PlayerState
{
    /// <summary>
    /// What the player was doing as they were taken hold of, for a game that goes on showing them so while they are
    /// held. It is not stepped.
    /// </summary>
    public PlayerState Was { get; } = was ?? throw new ArgumentNullException(nameof(was));

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
