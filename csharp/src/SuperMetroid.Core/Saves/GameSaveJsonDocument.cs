namespace SuperMetroid.Core.Saves;

/// <summary>Versioned, human-readable representation of all three cartridge save slots.</summary>
public sealed record GameSaveJsonDocument
{
    /// <summary>The file-format revision used to select compatible decoding and migration rules.</summary>
    public int SchemaVersion { get; init; } = GameSaveJsonFormat.SchemaVersion;

    /// <summary>The zero-based file-select slot persisted globally; valid values are 0 through 2.</summary>
    public required int SelectedSlot { get; init; }

    /// <summary>The three fixed cartridge slots in index order; <see langword="null"/> entries are empty or invalid.</summary>
    public required GameSaveSlotJsonDocument?[] Slots { get; init; }

    /// <summary>Global completion marker that unlocks the fourth attract-demo set.</summary>
    public required bool GameCompleted { get; init; }
}
