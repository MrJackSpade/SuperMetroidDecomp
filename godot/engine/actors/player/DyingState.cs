using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// Dying: the player's energy is gone and nothing saved them. They neither move of themselves nor act, and time
/// stands still for everything else. A throw that was carrying them as they began to die goes on carrying them
/// (<see cref="Throw"/>), for as long as the game playing the death lets it. The state has no end of its own: the
/// game plays the death, in the mode its definition makes for it
/// (<see cref="Core.GameDefinition.CreateDeathMode"/>), which is handed play on the update the player begins to die
/// and says what comes after.
/// </summary>
public sealed class DyingState : PlayerState
{
    /// <summary>The throw that was carrying the player as they began to die, if one was.</summary>
    private readonly Throw? _thrown;

    /// <summary>Dying, carried on by <paramref name="thrown"/> if a throw had them.</summary>
    public DyingState(Throw? thrown = null) => _thrown = thrown;

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override bool InAnothersHands => true;

    /// <summary>
    /// Time stands still for the enemies, the player's own shots and the room itself. What the enemies had launched
    /// goes on moving, and strikes no one.
    /// </summary>
    public override World.Stopped Stops => World.Stopped.Enemies | World.Stopped.PlayerShots | World.Stopped.Room;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        if (_thrown is { } thrown)
            owner.GoOnThrown(thrown);
        else
            owner.Velocity = PixelVelocity.Zero;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner) => null;
}
