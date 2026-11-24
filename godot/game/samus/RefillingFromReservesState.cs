using Engine.Actors;
using Engine.Core;
using Game.Data;

namespace Game;

/// <summary>
/// Being refilled from the reserve tanks after her energy ran out: energy comes back a little each update until she
/// is full or the tanks are empty. She neither moves nor can be hit meanwhile, and everything else in the room waits.
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
        if (owner.Room.Session.Clock.IsEvery(8))
            owner.Room.Session.Sounds.Request(Sounds.EnergyRefill, channel: "samus", priority: SoundPriority.Low);
        return null;
    }
}
