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
    /// <summary>A useful pre-item debugging state: 99 energy and no equipment.</summary>
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
