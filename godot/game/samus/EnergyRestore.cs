using Engine.Actors;
using Game.Data;

namespace Game;

/// <summary>
/// How Samus is given energy back by her gunship and by a crystal flash: her energy takes what it has room for, and
/// what it has no room for goes to her reserve tanks, as far as they hold, on the same update. Nothing of it is lost
/// while the reserves have room.
/// </summary>
public static class EnergyRestore
{
    /// <summary>Gives <paramref name="amount"/> of energy to whoever carries <paramref name="inventory"/>.</summary>
    public static void Give(Inventory inventory, int amount)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        int taken = inventory.Add(Inventory.Energy, amount);
        if (taken < amount)
            inventory.Add(Amounts.ReserveEnergy, amount - taken);
    }
}
