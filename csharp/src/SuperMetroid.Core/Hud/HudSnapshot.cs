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
    SuperMetroid.Core.Samus.SamusEquipmentFlags EquippedItems,
    ushort SelectedItem,
    int ReserveHealth,
    ushort ReserveMode)
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
        SelectedItem: 0,
        ReserveHealth: 0,
        ReserveMode: 0);
}
