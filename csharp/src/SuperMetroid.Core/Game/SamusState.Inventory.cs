namespace SuperMetroid.Core.Game;

public sealed partial class SamusState
{
    /// <summary>
    /// Copies the inventory and energy words at WRAM $09A2-$09D8, which lie below the Samus
    /// RAM block ($0A02 onward) that <c>InitializeSamus</c> ($91:E00D) clears. A Samus created
    /// fresh and given these words is that routine's result for the same player.
    /// </summary>
    internal void CopyInventoryFrom(SamusState source)
    {
        ArgumentNullException.ThrowIfNull(source);
        EquippedItems = source.EquippedItems;
        CollectedItems = source.CollectedItems;
        EquippedBeams = source.EquippedBeams;
        CollectedBeams = source.CollectedBeams;
        ReserveTankMode = source.ReserveTankMode;
        Health = source.Health;
        MaxHealth = source.MaxHealth;
        Missiles = source.Missiles;
        MaxMissiles = source.MaxMissiles;
        SuperMissiles = source.SuperMissiles;
        MaxSuperMissiles = source.MaxSuperMissiles;
        PowerBombs = source.PowerBombs;
        MaxPowerBombs = source.MaxPowerBombs;
        MaxReserveEnergy = source.MaxReserveEnergy;
        ReserveEnergy = source.ReserveEnergy;
        ReserveMissiles = source.ReserveMissiles;
    }
}
