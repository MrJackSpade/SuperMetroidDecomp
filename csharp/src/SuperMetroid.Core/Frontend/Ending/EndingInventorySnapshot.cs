namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Collected capacity and item bits captured for the ending's item-percentage calculation.</summary>
internal readonly record struct EndingInventorySnapshot(
    int MaxHealth,
    int MaxReserveEnergy,
    int MaxMissiles,
    int MaxSuperMissiles,
    int MaxPowerBombs,
    SuperMetroid.Core.Progression.SamusEquipmentFlags CollectedItems,
    SuperMetroid.Core.Progression.SamusBeamFlags CollectedBeams);
