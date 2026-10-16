namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Collected capacity and item bits captured for the ending's item-percentage calculation.</summary>
internal readonly record struct EndingInventorySnapshot(
    ushort MaxHealth,
    ushort MaxReserveEnergy,
    ushort MaxMissiles,
    ushort MaxSuperMissiles,
    ushort MaxPowerBombs,
    SuperMetroid.Core.Samus.SamusEquipmentFlags CollectedItems,
    SuperMetroid.Core.Samus.Projectiles.SamusBeamFlags CollectedBeams);
