namespace SuperMetroid.Core.Frontend;

/// <summary>Outcome of choosing JSON persistence or importing legacy SRAM, without claiming that a JSON file exists or reporting a gameplay-load result.</summary>
/// <param name="JsonPath">The requested JSON path, preserved exactly as supplied rather than normalized to an absolute path.</param>
/// <param name="MigratedLegacySram">True only when an absent JSON file led to importing legacy SRAM and successfully writing its successor; false also covers no files present and older-JSON upgrades.</param>
public readonly record struct GameSaveLoadResult(
    string JsonPath,
    bool MigratedLegacySram);
