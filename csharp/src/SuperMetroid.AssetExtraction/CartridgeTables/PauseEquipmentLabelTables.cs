namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Where the cartridge keeps one inventory category's equipment-page labels.</summary>
/// <param name="Places">The table of each item label's equipment-page tilemap word.</param>
/// <param name="Labels">The table of each item label's bank-$82 tilemap pointer.</param>
internal readonly record struct PauseEquipmentLabelTables(int Places, int Labels);
