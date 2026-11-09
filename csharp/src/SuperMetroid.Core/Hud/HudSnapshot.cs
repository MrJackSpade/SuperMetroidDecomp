namespace SuperMetroid.Core.Hud;

/// <summary>Explicit inputs consumed while constructing one HUD state.</summary>
public readonly record struct HudSnapshot(
    int Health,
    int MaxHealth,
    int Missiles,
    int MaxMissiles,
    int SuperMissiles,
    int MaxSuperMissiles,
    int PowerBombs,
    int MaxPowerBombs,
    SuperMetroid.Core.Progression.SamusEquipmentFlags EquippedItems,
    SuperMetroid.Core.Samus.HudItem SelectedItem,
    int ReserveHealth,
    SuperMetroid.Core.Progression.ReserveTankMode ReserveMode)
{
    /// <summary>The values the HUD shows for <paramref name="samus"/>'s inventory and selection now.</summary>
    internal static HudSnapshot Of(SuperMetroid.Core.Samus.SamusState samus) => new(
        Ensure.NotNull(samus).Health, samus.MaxHealth, samus.Missiles, samus.MaxMissiles, samus.SuperMissiles,
        samus.MaxSuperMissiles, samus.PowerBombs, samus.MaxPowerBombs, samus.EquippedItems, samus.SelectedHudItem,
        samus.ReserveEnergy, samus.ReserveTankMode);

    /// <summary>A HUD before any item is collected: 99 energy and no equipment.</summary>
    public static HudSnapshot CeresDebug => new(
        Health: 99,
        MaxHealth: 99,
        Missiles: 0,
        MaxMissiles: 0,
        SuperMissiles: 0,
        MaxSuperMissiles: 0,
        PowerBombs: 0,
        MaxPowerBombs: 0,
        EquippedItems: 0,
        SelectedItem: SuperMetroid.Core.Samus.HudItem.Beam,
        ReserveHealth: 0,
        ReserveMode: SuperMetroid.Core.Progression.ReserveTankMode.None);
}
