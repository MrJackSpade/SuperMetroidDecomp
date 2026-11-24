using Engine.Shared;

namespace Engine.Actors;

/// <summary>Standing at a station while it works: the player does not move or act until the station is done.</summary>
public sealed class UsingStationState : PlayerState
{
    /// <summary>Whether the player has turned to face out of the screen, as in a station they stand in.</summary>
    public bool FacingOut { get; init; }

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.Velocity = PixelVelocity.Zero;

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner) => null;
}
