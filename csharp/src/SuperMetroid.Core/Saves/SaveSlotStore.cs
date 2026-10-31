using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// The session's battery-backed saves as owned state (#1322): three slots (null is empty or invalid), the menu's
/// selected slot and game completion. Gameplay, menus, checkpoints and JSON read and write it directly; the cartridge's
/// save image is only the hosts' import and export format.
/// </summary>
public sealed class SaveSlotStore
{
    /// <summary>The retail format's fixed number of save slots.</summary>
    public const int SlotCount = 3;

    /// <summary>Each slot's decoded record; null is an empty or invalid slot.</summary>
    internal readonly SuperMetroidSaveSlot?[] slots = new SuperMetroidSaveSlot?[SlotCount];

    /// <summary>The file-select menu's persistent selection.</summary>
    private int selectedSlot;

    /// <summary>
    /// The earthquake rumble delay the save image this store was imported from gives at power-on, which its host decided
    /// from the image's raw completion mark; null once the mark is rewritten, or when the store was never imported.
    /// Saving, copying or clearing a slot leaves the mark, as the cartridge does.
    /// </summary>
    private PowerOnEarthquakeSoundDelay? importedEarthquakeDelay;

    /// <summary>Installed stock map rules that pack and unpack explored maps; the host binds them with its artwork.</summary>
    [NonSerialized] private IAreaMapSource? mapPresentation;

    /// <summary>Creates an empty store: no saved slot, slot A selected, the game not completed.</summary>
    /// <param name="mapPresentation">Installed stock map rules; saving a slot requires them.</param>
    public SaveSlotStore(IAreaMapSource? mapPresentation = null) => this.mapPresentation = mapPresentation;

    /// <summary>
    /// A store imported from a save image a host decoded: its slots, the menu's selection, completion, whether the
    /// completion mark held the corrupt-save signature, and the earthquake rumble delay the image's mark gives at power-on.
    /// </summary>
    /// <param name="slots">The three slots; null is an empty or invalid slot.</param>
    /// <param name="selectedSlot">The menu's selected slot.</param>
    /// <param name="gameCompleted">Whether the mark holds the completed-game signature.</param>
    /// <param name="corruptMarkerWritten">Whether the mark holds the corrupt-save signature.</param>
    /// <param name="earthquakeDelay">The rumble delay the image's mark gives at power-on while a slot is valid.</param>
    /// <param name="maps">Installed stock map rules that saving a slot requires.</param>
    [IndependentFlag("Game completion and the corrupt marker are separate facts decoded from a save image.")]
    public static SaveSlotStore Imported(SuperMetroidSaveSlot?[] slots, int selectedSlot, bool gameCompleted,
        bool corruptMarkerWritten, PowerOnEarthquakeSoundDelay earthquakeDelay, IAreaMapSource maps)
    {
        Ensure.LengthEqual(slots, SlotCount);
        var store = new SaveSlotStore(maps)
        {
            selectedSlot = selectedSlot,
            HasCompletedGame = gameCompleted,
            CorruptMarkerWritten = corruptMarkerWritten,
            importedEarthquakeDelay = Ensure.NotNull(earthquakeDelay),
        };
        slots.CopyTo(store.slots, 0);
        return store;
    }

    /// <summary>Whether the marker field holds the corrupt-SRAM signature boot writes when every slot is invalid.</summary>
    public bool CorruptMarkerWritten { get; private set; }

    /// <summary>Installs map rules after a host binds artwork, or after restoring a store that did not serialize them.</summary>
    internal void BindMapPresentation(IAreaMapSource? maps) => mapPresentation = maps;

    /// <summary>A detached copy of the save in <paramref name="slot"/>, or null when it is empty or invalid.</summary>
    public SuperMetroidSaveSlot? ReadSlot(int slot)
    {
        RequireSlot(slot);
        return slots[slot] is { } saved ? Detached(saved, slot) : null;
    }

    /// <summary>
    /// Ports <c>SaveToSRAM</c>: stores the slot the cartridge would read back, so what a save does not keep (exploration
    /// of map runs no stock cell occupies, Ceres's exploration) is gone when the slot is read back.
    /// </summary>
    public void SaveSlot(int slot, SuperMetroidSaveSnapshot snapshot)
    {
        RequireSlot(slot);
        slots[slot] = SaveSlotRetention.Retain(snapshot, slot, mapPresentation);
    }

    /// <summary>File-copy menu index 13 at <c>$81:9A2C</c>: the destination receives the source's save.</summary>
    public void CopySlot(int sourceSlot, int destinationSlot)
    {
        RequireSlot(sourceSlot);
        RequireSlot(destinationSlot);
        if (sourceSlot == destinationSlot)
            throw new ArgumentException("File copy requires two different SRAM slots.");
        SuperMetroidSaveSlot source = slots[sourceSlot] ??
            throw new InvalidOperationException($"Cannot copy empty save slot {sourceSlot}.");
        slots[destinationSlot] = Detached(source, destinationSlot);
    }

    /// <summary>File-clear menu index 25 at <c>$81:9C9E</c>: the slot becomes empty.</summary>
    public void ClearSlot(int slot)
    {
        RequireSlot(slot);
        slots[slot] = null;
    }

    /// <summary>Stores the menu's selected slot.</summary>
    public void SelectSlot(int slot)
    {
        RequireSlot(slot);
        selectedSlot = slot;
    }

    /// <summary>The menu's selected slot.</summary>
    public int ReadSelectedSlot() => selectedSlot;

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
    /// Boot's <c>DetermineNumberOfDemoSets</c> ($80:8261), called once from $80:8564. When all three slots are invalid
    /// it overwrites the completion marker with the corrupt-SRAM signature ($80:8283-8290) and
    /// enables three demo sets; otherwise the completion signature enables the fourth. The cartridge never re-evaluates
    /// the count before reset.
    /// </summary>
    /// <returns>The attract-demo set count fixed for the session.</returns>
    public int DetermineNumberOfDemoSets()
    {
        if (!AnySlotValid)
        {
            HasCompletedGame = false;
            CorruptMarkerWritten = true;
            MarkerRewritten();
            return DemoSetUnlockDefinitions.SetsBeforeCompletion;
        }
        return HasCompletedGame ? DemoSetUnlockDefinitions.SetCount : DemoSetUnlockDefinitions.SetsBeforeCompletion;
    }

    /// <summary>Rejects slot numbers outside A, B and C.</summary>
    private static void RequireSlot(int slot)
    {
        if ((uint)slot >= SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slot), slot, "Save slot must be A, B, or C (0-2).");
    }

    /// <summary>Whether any slot holds a save.</summary>
    private bool AnySlotValid => slots.Any(slot => slot is not null);

    /// <summary>The marker field no longer holds the decoded image's words.</summary>
    private void MarkerRewritten() => importedEarthquakeDelay = null;

    /// <summary>A copy of <paramref name="saved"/> as slot <paramref name="slot"/>, sharing no array with it.</summary>
    private static SuperMetroidSaveSlot Detached(SuperMetroidSaveSlot saved, int slot) => saved with
    {
        Slot = slot,
        Progression = saved.Progression.Copy(),
    };
}
