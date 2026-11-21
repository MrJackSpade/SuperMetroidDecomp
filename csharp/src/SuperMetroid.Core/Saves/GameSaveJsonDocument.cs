namespace SuperMetroid.Core.Saves;

/// <summary>Versioned, human-readable representation of all three save slots.</summary>
public sealed record GameSaveJsonDocument
{
    /// <summary>The file-format revision used to select compatible decoding and migration rules.</summary>
    public int SchemaVersion { get; init; } = GameSaveJsonFormat.SchemaVersion;

    /// <summary>The zero-based file-select slot persisted globally; valid values are 0 through 2.</summary>
    public required int SelectedSlot { get; init; }

    /// <summary>The three fixed save slots in index order; <see langword="null"/> entries are empty or invalid.</summary>
    public required GameSaveSlotJsonDocument?[] Slots { get; init; }

    /// <summary>
    /// The save file's history (#2140, #2560): a completed game, which unlocks the fourth attract-demo set, or a power-on that
    /// found no valid save, which adds a frame to the earthquake rumble's delay at every later power-on until the game is
    /// completed, or neither.
    /// </summary>
    public required SaveFileHistory History { get; init; }
}
