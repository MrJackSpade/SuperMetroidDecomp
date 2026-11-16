using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// The session's saves as owned state (#1322): the three slots A, B and C (an absent save is an empty or invalid slot),
/// the menu's selected slot and game completion. Gameplay, menus, checkpoints and JSON read and write it directly by
/// <see cref="SaveSlot"/>; a slot's number and the raw save image are only the save files' formats (#2083).
/// </summary>
public sealed class SaveSlotStore
{
    /// <summary>Each saved slot's decoded record; a slot with no entry is empty or invalid.</summary>
    internal readonly Dictionary<SaveSlot, SuperMetroidSaveSlot> slots = [];

    /// <summary>The file-select menu's persistent selection.</summary>
    private SaveSlot selectedSlot;

    /// <summary>
    /// The earthquake rumble delay the save image this store was imported from gives at power-on, which its host decided
    /// from the image's raw completion mark; null once the mark is rewritten, or when the store was never imported.
    /// Saving, copying or clearing a slot leaves the mark, as the game does.
    /// </summary>
    private PowerOnEarthquakeSoundDelay? importedEarthquakeDelay;

    /// <summary>Installed stock map rules that pack and unpack explored maps; the host binds them with its artwork.</summary>
    private IAreaMapSource? mapPresentation;

    /// <summary>Creates an empty store: no saved slot, slot A selected, the game not completed.</summary>
    /// <param name="mapPresentation">Installed stock map rules; saving a slot requires them.</param>
    public SaveSlotStore(IAreaMapSource? mapPresentation = null) => this.mapPresentation = mapPresentation;

    /// <summary>
    /// A store imported from a save image a host decoded: its slots, the menu's selection, completion, whether the
    /// completion mark held the corrupt-save signature, and the earthquake rumble delay the image's mark gives at power-on.
    /// </summary>
    /// <param name="saved">The slots holding a save, each in the slot it names; a slot not among them is empty or invalid.</param>
    /// <param name="selectedSlot">The menu's selected slot.</param>
    /// <param name="gameCompleted">Whether the mark holds the completed-game signature.</param>
    /// <param name="corruptMarkerWritten">Whether the mark holds the corrupt-save signature.</param>
    /// <param name="earthquakeDelay">The rumble delay the image's mark gives at power-on while a slot is valid.</param>
    /// <param name="maps">Installed stock map rules that saving a slot requires.</param>
    /// <exception cref="ArgumentException">Two saves name the same slot.</exception>
    [IndependentFlag("Game completion and the corrupt marker are separate facts decoded from a save image.")]
    public static SaveSlotStore Imported(IEnumerable<SuperMetroidSaveSlot> saved, SaveSlot selectedSlot, bool gameCompleted,
        bool corruptMarkerWritten, PowerOnEarthquakeSoundDelay earthquakeDelay, IAreaMapSource maps)
    {
        Ensure.NotNull(saved);
        RequireSlot(selectedSlot);
        var store = new SaveSlotStore(maps)
        {
            selectedSlot = selectedSlot,
            HasCompletedGame = gameCompleted,
            CorruptMarkerWritten = corruptMarkerWritten,
            importedEarthquakeDelay = Ensure.NotNull(earthquakeDelay),
        };
        foreach (SuperMetroidSaveSlot save in saved)
        {
            RequireSlot(Ensure.NotNull(save).Slot);
            if (!store.slots.TryAdd(save.Slot, save))
                throw new ArgumentException($"Two imported saves name slot {save.Slot}.", nameof(saved));
        }
        return store;
    }

    /// <summary>Whether the marker field holds the corrupt-saves signature boot writes when every slot is invalid.</summary>
    public bool CorruptMarkerWritten { get; private set; }

    /// <summary>Installs map rules when the host binds artwork.</summary>
    internal void BindMapPresentation(IAreaMapSource? maps) => mapPresentation = maps;

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
        slots[slot] = SaveSlotRetention.Retain(snapshot, slot, mapPresentation);
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

    /// <summary>True for the completed-game signature.</summary>
    public bool HasCompletedGame { get; private set; }

    /// <summary>Sets or clears the completed-game signature.</summary>
    public void SetGameCompleted(bool completed)
    {
        HasCompletedGame = completed;
        CorruptMarkerWritten = false;
        MarkerRewritten();
    }

    /// <summary>
    /// The frame this save file adds at power-on to every earthquake rumble's delay
    /// (<see cref="PowerOnEarthquakeSoundDelay"/>): one when no slot is valid, the game is completed or the mark holds
    /// the corrupt-save signature, none for a valid save without either mark. Until the mark is rewritten, a store
    /// imported from a save image gives the delay its host decided from that image's raw mark.
    /// </summary>
    public PowerOnEarthquakeSoundDelay EarthquakeSoundDelayAtPowerOn()
    {
        if (!AnySlotValid)
            return PowerOnEarthquakeSoundDelay.OneFrame;
        if (importedEarthquakeDelay is { } imported)
            return imported;
        return HasCompletedGame || CorruptMarkerWritten ? PowerOnEarthquakeSoundDelay.OneFrame : PowerOnEarthquakeSoundDelay.None;
    }

    /// <summary>
    /// Decides at boot, once, whether the title's attract demos include the completion set. When every save slot is
    /// invalid it overwrites the completion marker with the corrupt-saves signature and leaves the completion set out;
    /// otherwise the completion signature unlocks it. The game never decides again before reset.
    /// </summary>
    /// <returns>Whether the completion demo set plays for the session.</returns>
    public bool DetermineCompletionDemoSetUnlocked()
    {
        if (!AnySlotValid)
        {
            HasCompletedGame = false;
            CorruptMarkerWritten = true;
            MarkerRewritten();
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

    /// <summary>The completion marker has been rewritten, so it no longer holds what the imported save left in it.</summary>
    private void MarkerRewritten() => importedEarthquakeDelay = null;

    /// <summary>A copy of <paramref name="saved"/> as slot <paramref name="slot"/>, sharing no array with it.</summary>
    private static SuperMetroidSaveSlot Detached(SuperMetroidSaveSlot saved, SaveSlot slot) => saved with
    {
        Slot = slot,
        Progression = saved.Progression.Copy(),
    };
}
