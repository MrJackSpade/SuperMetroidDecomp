using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// Dying: the player's energy is gone and nothing saved them. They neither move nor act, and time stands still for
/// everything else. The state has no end of its own: the game plays the death, in the mode its definition makes
/// for it (<see cref="Core.GameDefinition.CreateDeathMode"/>), which is handed play on the update the player begins
/// to die and says what comes after.
/// </summary>
public sealed class DyingState : PlayerState
{
    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override bool InAnothersHands => true;

    /// <summary>Time stands still for everything but the player: the enemies, every shot and pickup, and the room itself wait.</summary>
    public override World.Stopped Stops => World.Stopped.Enemies | World.Stopped.Projectiles | World.Stopped.Room;

    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.Velocity = PixelVelocity.Zero;

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner) => null;
}
