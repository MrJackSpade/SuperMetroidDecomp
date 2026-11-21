namespace SuperMetroid.Core.Saves;

/// <summary>Versioned, human-readable representation of all three save slots.</summary>
public sealed record GameSaveJsonDocument
{
    /// <summary>The file-format revision used to select compatible decoding and migration rules.</summary>
    public int SchemaVersion { get; init; } = GameSaveJsonFormat.SchemaVersion;

    /// <summary>The file-select slot persisted globally, by its name (<see cref="GameSaveJsonFormat.SlotKey"/>).</summary>
    public required string SelectedSlot { get; init; }

    /// <summary>The slots holding a save, keyed by the slot's name; an empty or invalid slot has no entry.</summary>
    public required Dictionary<string, GameSaveSlotJsonDocument> Slots { get; init; }

    /// <summary>Whether the save file's history holds a completed game, which unlocks the fourth attract-demo set.</summary>
    public required bool GameCompleted { get; init; }

    /// <summary>
    /// Whether the save file's history holds a power-on that found no valid save (#2140), which adds a frame to the earthquake
    /// rumble's delay at every later power-on until the game is completed; never together with <see cref="GameCompleted"/>.
    /// </summary>
    public required bool PowerOnFoundNoValidSave { get; init; }
}
