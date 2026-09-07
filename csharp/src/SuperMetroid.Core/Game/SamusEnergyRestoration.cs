namespace SuperMetroid.Core.Game;

/// <summary>Shared cartridge energy restoration, including overflow into reserve tanks.</summary>
public static class SamusEnergyRestoration
{
    /// <summary>
    /// Implements Restore_A_Energy_ToSamus ($91:DF12). Fill regular energy first, spill
    /// only the excess into reserves, and enable auto reserve only from disabled mode.
    /// Gunship and crystal-flash restoration both call this native routine.
    /// </summary>
    public static void Restore(SamusState samus, ushort amount)
    {
        ArgumentNullException.ThrowIfNull(samus);
        ushort restored = unchecked((ushort)(samus.Health + amount));
        samus.Health = restored;
        if (unchecked((short)(restored - samus.MaxHealth)) < 0)
            return;

        ushort overflow = unchecked((ushort)(restored - samus.MaxHealth));
        ushort reserve = unchecked((ushort)(samus.ReserveEnergy + overflow));
        if (unchecked((short)(reserve - samus.MaxReserveEnergy)) >= 0)
            reserve = samus.MaxReserveEnergy;
        samus.ReserveEnergy = reserve;
        if (reserve != 0 && samus.ReserveTankMode == 0)
            samus.ReserveTankMode = 1;
        samus.Health = samus.MaxHealth;
    }
}
