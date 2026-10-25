using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus;

/// <summary>Shared cartridge energy restoration, including overflow into reserve tanks.</summary>
public static class SamusEnergyRestoration
{
    /// <summary>
    /// Implements Restore_A_Energy_ToSamus ($91:DF12). Fill regular energy first, spill
    /// only the excess into reserves, and enable auto reserve only from disabled mode.
    /// Gunship and crystal-flash restoration both call this native routine.
    /// </summary>
    public static void Restore(SamusState samus, int amount)
    {
        Ensure.NotNull(samus);
        int restored = samus.Health + amount;
        samus.Health = restored;
        if (restored < samus.MaxHealth)
            return;

        int reserve = samus.ReserveEnergy + restored - samus.MaxHealth;
        if (reserve >= samus.MaxReserveEnergy)
            reserve = samus.MaxReserveEnergy;
        samus.ReserveEnergy = reserve;
        if (reserve != 0 && samus.ReserveTankMode == ReserveTankMode.None)
            samus.ReserveTankMode = ReserveTankMode.Auto;
        samus.Health = samus.MaxHealth;
    }
}
