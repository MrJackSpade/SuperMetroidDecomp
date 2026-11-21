using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// The session's saves as owned state (#1322): the three slots A, B and C (an absent save is an empty or invalid slot),
/// the menu's selected slot and the save file's history of the game as a whole (<see cref="SaveFileHistory"/>).
/// Gameplay, menus, checkpoints and JSON read and write it directly by <see cref="SaveSlot"/>; a slot's number and the
/// raw save image are only the save files' formats (#2083, #2140).
/// </summary>
public sealed class SaveSlotStore
{
    /// <summary>Each saved slot's decoded record; a slot with no entry is empty or invalid.</summary>
    private readonly Dictionary<SaveSlot, SuperMetroidSaveSlot> slots = [];

    /// <summary>The file-select menu's persistent selection.</summary>
    private SaveSlot selectedSlot;

    /// <summary>Installed stock map rules that pack and unpack explored maps; the host binds them with its artwork.</summary>
    private IAreaMapSource maps;

    /// <summary>Creates an empty store: no saved slot, slot A selected, no history.</summary>
    /// <param name="maps">Installed stock map rules, which decide what a saved slot keeps of the explored maps.</param>
    public SaveSlotStore(IAreaMapSource maps) => this.maps = Ensure.NotNull(maps);

    /// <summary>A store imported from a save file a host decoded: its slots, the menu's selection and the file's history.</summary>
    /// <param name="saved">The slots holding a save, each in the slot it names; a slot not among them is empty or invalid.</param>
    /// <param name="selectedSlot">The menu's selected slot.</param>
    /// <param name="history">The save file's history of the game as a whole.</param>
    /// <param name="maps">Installed stock map rules that saving a slot requires.</param>
    /// <exception cref="ArgumentException">Two saves name the same slot.</exception>
    public static SaveSlotStore Imported(IEnumerable<SuperMetroidSaveSlot> saved, SaveSlot selectedSlot, SaveFileHistory history,
        IAreaMapSource maps)
    {
        Ensure.NotNull(saved);
        RequireSlot(selectedSlot);
        var store = new SaveSlotStore(maps)
        {
            selectedSlot = selectedSlot,
            History = RequireHistory(history),
        };
        foreach (SuperMetroidSaveSlot save in saved)
        {
            RequireSlot(Ensure.NotNull(save).Slot);
            if (!store.slots.TryAdd(save.Slot, save))
                throw new ArgumentException($"Two imported saves name slot {save.Slot}.", nameof(saved));
        }
        return store;
    }

    /// <summary>The save file's history of the game as a whole: a completed game, a power-on that found no valid save, or neither.</summary>
    public SaveFileHistory History { get; private set; }

    /// <summary>Installs map rules when the host binds artwork.</summary>
    internal void BindMaps(IAreaMapSource maps) => this.maps = Ensure.NotNull(maps);

    /// <summary>A detached copy of the save in <paramref name="slot"/>, or null when it is empty or invalid.</summary>
    public SuperMetroidSaveSlot? ReadSlot(SaveSlot slot)
    {
        RequireSlot(slot);
        return slots.TryGetValue(slot, out SuperMetroidSaveSlot? saved) ? Detached(saved, slot) : null;
    }

    /// <summary>
    /// Saves a slot: stores the slot as the game would read it back, so what a save does not keep (exploration
    /// of map runs no stock cell occupies, Ceres's exploration) is gone when the slot is read back.
    /// </summary>
    public void SaveSlot(SaveSlot slot, SuperMetroidSaveSnapshot snapshot)
    {
        RequireSlot(slot);
        slots[slot] = SaveSlotRetention.Retain(snapshot, slot, maps);
    }

    /// <summary>Copies a save as the file-select menu's data copy does: the destination receives the source's save.</summary>
    public void CopySlot(SaveSlot sourceSlot, SaveSlot destinationSlot)
    {
        RequireSlot(sourceSlot);
        RequireSlot(destinationSlot);
        if (sourceSlot == destinationSlot)
            throw new ArgumentException("File copy requires two different save slots.");
        if (!slots.TryGetValue(sourceSlot, out SuperMetroidSaveSlot? source))
            throw new InvalidOperationException($"Cannot copy empty save slot {sourceSlot}.");
        slots[destinationSlot] = Detached(source, destinationSlot);
    }

    /// <summary>Clears a save as the file-select menu's data clear does: the slot becomes empty.</summary>
    public void ClearSlot(SaveSlot slot)
    {
        RequireSlot(slot);
        slots.Remove(slot);
    }

    /// <summary>Stores the menu's selected slot.</summary>
    public void SelectSlot(SaveSlot slot)
    {
        RequireSlot(slot);
        selectedSlot = slot;
    }

    /// <summary>The menu's selected slot.</summary>
    public SaveSlot ReadSelectedSlot() => selectedSlot;

    /// <summary>Whether the save file's history holds a completed game, which unlocks the completion demo set.</summary>
    private bool HasCompletedGame => History == SaveFileHistory.GameCompleted;

    /// <summary>The ending completes the game; the save file's history keeps it until a power-on finds no valid save.</summary>
    public void CompleteGame() => History = SaveFileHistory.GameCompleted;

    /// <summary>Restores the history a save file kept, as its host reads it back.</summary>
    /// <param name="history">The save file's history of the game as a whole.</param>
    public void RestoreHistory(SaveFileHistory history) => History = RequireHistory(history);

    /// <summary>
    /// The frame this save file adds at power-on to every earthquake rumble's delay, which
    /// <see cref="PowerOnEarthquakeSoundDelay"/> derives from whether a slot is valid and from the file's history.
    /// </summary>
    public PowerOnEarthquakeSoundDelay EarthquakeSoundDelayAtPowerOn() => PowerOnEarthquakeSoundDelay.FoundAtPowerOn(
        AnySlotValid, historyKept: History != SaveFileHistory.None);

    /// <summary>
    /// Decides at boot, once, whether the title's attract demos include the completion set. When every save slot is
    /// invalid the file's history becomes a power-on that found no valid save, replacing a completed game, and the
    /// completion set stays out; otherwise a completed game unlocks it. The game never decides again before reset.
    /// </summary>
    /// <returns>Whether the completion demo set plays for the session.</returns>
    public bool DetermineCompletionDemoSetUnlocked()
    {
        if (!AnySlotValid)
        {
            History = SaveFileHistory.PowerOnFoundNoValidSave;
            return false;
        }
        return HasCompletedGame;
    }
    /// <summary>Rejects a value that names none of the slots A, B and C.</summary>
    private static void RequireSlot(SaveSlot slot)
    {
        if (!Enum.IsDefined(slot))
            throw new ArgumentOutOfRangeException(nameof(slot), slot, "Save slot must be A, B, or C.");
    }

    /// <summary>Whether any slot holds a save.</summary>
    private bool AnySlotValid => slots.Count > 0;

    /// <summary>Rejects a value that names no save-file history.</summary>
    private static SaveFileHistory RequireHistory(SaveFileHistory history) => Enum.IsDefined(history)
        ? history
        : throw new ArgumentOutOfRangeException(nameof(history), history, "Undefined save-file history.");

    /// <summary>A copy of <paramref name="saved"/> as slot <paramref name="slot"/>, sharing no array with it.</summary>
    private static SuperMetroidSaveSlot Detached(SuperMetroidSaveSlot saved, SaveSlot slot) => saved with
    {
        Slot = slot,
        Progression = saved.Progression.Copy(),
    };
}
