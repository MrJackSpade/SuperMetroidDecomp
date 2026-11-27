using Engine.Actors;
using Engine.Core;
using Engine.World;
using Game.Data;

namespace Game;

/// <summary>
/// Being refilled from the reserve tanks after her energy ran out: energy comes back a little each update until she
/// is full, which empties the tanks of what is left, or the tanks are empty. She neither moves nor can be hit meanwhile, and everything else in the room waits.
/// </summary>
public sealed class RefillingFromReservesState : PlayerState, IUntouchable, ISamusLook
{
    private string _look = "standing";

    /// <inheritdoc/>
    public string Look => _look;

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override bool InAnothersHands => true;

    /// <summary>
    /// Time stands still for the enemies, her own shots and the room itself. What the enemies had launched goes on
    /// moving, and strikes no one: the cartridge's refill freezes play's time, which its enemies' shots do not go by.
    /// </summary>
    public override Stopped Stops => Stopped.Enemies | Stopped.PlayerShots | Stopped.Room;

    /// <inheritdoc/>
    public override void Enter(Player owner) =>
        _look = owner.Form == Player.Crawling ? "morph_ball" : owner.IsOnGround ? "standing" : "falling";

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        var samus = (Samus)owner;
        Inventory inventory = owner.Inventory;
        int wanted = Math.Min(samus.ReserveRefillPerUpdate, inventory.Capacity(Amounts.Energy) - inventory.Amount(Amounts.Energy));
        int given = Math.Min(wanted, inventory.Amount(Amounts.ReserveEnergy));
        if (given <= 0)
            return owner.StateAtRest();
        inventory.Spend(Amounts.ReserveEnergy, given);
        inventory.Add(Amounts.Energy, given);
        // Once she is full, whatever is left in the tanks is lost.
        if (inventory.Amount(Amounts.Energy) >= inventory.Capacity(Amounts.Energy))
            inventory.Spend(Amounts.ReserveEnergy, inventory.Amount(Amounts.ReserveEnergy));
        if (owner.Room.Session.Clock.IsEvery(8))
            owner.Room.Session.Sounds.Request(Sounds.EnergyRefill, channel: "samus", priority: SoundPriority.Low);

        // The refill is over on the update that leaves the tanks empty: she is her own again, and the room goes on,
        // from the next.
        return inventory.Amount(Amounts.ReserveEnergy) == 0 ? owner.StateAtRest() : null;
    }
}
