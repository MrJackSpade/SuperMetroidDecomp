using Engine.Shared;

namespace Engine.Actors;

/// <summary>Held in place by something: the player does not move or act until whatever holds them lets go.</summary>
/// <param name="was">What the player was doing as they were taken hold of.</param>
/// <param name="keepsSpeed">
/// Whether the speed the player had stays theirs through the hold: a hold that keeps what they were doing. Nothing
/// moves them by it while they are held, and whether they were on the way up stays as it was, so the view goes on
/// following where they are carried as it follows a rise or a fall.
/// </param>
public sealed class HeldState(PlayerState was, bool keepsSpeed = false) : PlayerState
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
    public override void Enter(Player owner)
    {
        if (!keepsSpeed)
            owner.Velocity = PixelVelocity.Zero;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner) => null;
}
