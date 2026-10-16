using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// The session's battery-backed saves as owned state (#1322): three slots (null is empty or invalid), the menu's
/// selected slot and game completion. Gameplay, menus, checkpoints and JSON read and write it directly; SRAM is only
/// <see cref="SaveRamCodec"/>'s import and export format.
/// </summary>
public sealed class SaveSlotStore
{
    /// <summary>The retail format's fixed number of save slots.</summary>
    public const int SlotCount = SaveRamLayout.SlotCount;

    /// <summary>Each slot's decoded record; null is an empty or invalid slot.</summary>
    internal readonly SuperMetroidSaveSlot?[] slots = new SuperMetroidSaveSlot?[SlotCount];

    /// <summary>The file-select menu's persistent selection.</summary>
    private int selectedSlot;

    /// <summary>
    /// The marker comparison's carry for the marker field of the SRAM image this store was decoded from, which depends
    /// on its raw words; null once the marker is rewritten, or when the store was never decoded. Saving, copying or
    /// clearing a slot leaves the field, as SRAM does.
    /// </summary>
    private bool? decodedMarkerCarry;

    /// <summary>Installed stock map rules that pack and unpack explored maps; the host binds them with its artwork.</summary>
    [NonSerialized] private IAreaMapSource? mapPresentation;

    /// <summary>Creates an empty store: no saved slot, slot A selected, the game not completed.</summary>
    /// <param name="mapPresentation">Installed stock map rules; saving a slot requires them.</param>
    public SaveSlotStore(IAreaMapSource? mapPresentation = null) => this.mapPresentation = mapPresentation;

    /// <summary>The store <see cref="SaveRamCodec.Decode"/> produces from an SRAM image.</summary>
    internal static SaveSlotStore Decoded(SuperMetroidSaveSlot?[] slots, int selectedSlot, bool gameCompleted,
        bool corruptMarkerWritten, bool markerCarry, IAreaMapSource maps)
    {
        Ensure.LengthEqual(slots, SlotCount);
        var store = new SaveSlotStore(maps)
        {
            selectedSlot = selectedSlot,
            HasCompletedGame = gameCompleted,
            CorruptMarkerWritten = corruptMarkerWritten,
            decodedMarkerCarry = markerCarry,
        };
        slots.CopyTo(store.slots, 0);
        return store;
    }

    /// <summary>Whether the marker field holds the corrupt-SRAM signature boot writes when every slot is invalid.</summary>
    internal bool CorruptMarkerWritten { get; private set; }

    /// <summary>Installs map rules after a host binds artwork, or after restoring a store that did not serialize them.</summary>
    internal void BindMapPresentation(IAreaMapSource? maps) => mapPresentation = maps;

    /// <summary>A detached copy of the save in <paramref name="slot"/>, or null when it is empty or invalid.</summary>
    public SuperMetroidSaveSlot? ReadSlot(int slot)
    {
        SaveRamCodec.ValidateSlot(slot);
        return slots[slot] is { } saved ? Detached(saved, slot) : null;
    }

    /// <summary>
    /// Ports <c>SaveToSRAM</c>: stores the slot the cartridge would read back, so fields SRAM does not keep (explored
    /// bytes no stock tile occupies, Boolean words other than 0 and 1) read back as SRAM would return them.
    /// </summary>
    public void SaveSlot(int slot, SuperMetroidSaveSnapshot snapshot)
    {
        SaveRamCodec.ValidateSlot(slot);
        slots[slot] = SaveRamCodec.DecodePayload(SaveRamCodec.EncodePayload(snapshot, mapPresentation), slot, mapPresentation);
    }

    /// <summary>File-copy menu index 13 at <c>$81:9A2C</c>: the destination receives the source's save.</summary>
    public void CopySlot(int sourceSlot, int destinationSlot)
    {
        SaveRamCodec.ValidateSlot(sourceSlot);
        SaveRamCodec.ValidateSlot(destinationSlot);
        if (sourceSlot == destinationSlot)
            throw new ArgumentException("File copy requires two different SRAM slots.");
        SuperMetroidSaveSlot source = slots[sourceSlot] ??
            throw new InvalidOperationException($"Cannot copy empty save slot {sourceSlot}.");
        slots[destinationSlot] = Detached(source, destinationSlot);
    }

    /// <summary>File-clear menu index 25 at <c>$81:9C9E</c>: the slot becomes empty.</summary>
    public void ClearSlot(int slot)
    {
        SaveRamCodec.ValidateSlot(slot);
        slots[slot] = null;
    }

    /// <summary>Stores the menu's selected slot.</summary>
    public void SelectSlot(int slot)
    {
        SaveRamCodec.ValidateSlot(slot);
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
    /// The carry <c>DetermineNumberOfDemoSets</c> ($80:8261) returns at boot: set when no slot is valid, otherwise the
    /// marker comparison's carry. Until the marker is rewritten, a store decoded from SRAM compares that image's raw
    /// marker words; otherwise the marker the codec would encode.
    /// </summary>
    public bool DetermineBootMainLoopCarry()
    {
        if (!AnySlotValid)
            return true;
        if (decodedMarkerCarry is { } decoded)
            return decoded;
        ReadOnlySpan<byte> marker = HasCompletedGame ? SaveRamLayout.CompletionMarker
            : CorruptMarkerWritten ? SaveRamLayout.CorruptSramMarker
            : new byte[SaveRamLayout.CompletionMarker.Length];
        return SaveRamCodec.MarkerCarry(marker);
    }

    /// <summary>
    /// Boot's <c>DetermineNumberOfDemoSets</c> ($80:8261), called once from $80:8564. When all three slots are invalid
    /// it overwrites the completion marker with <see cref="SaveRamLayout.CorruptSramMarker"/> ($80:8283-8290) and
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
            return DemoSetUnlockRomData.DefaultSetCount;
        }
        return HasCompletedGame ? DemoSetUnlockRomData.SetCount : DemoSetUnlockRomData.DefaultSetCount;
    }

    /// <summary>Whether any slot holds a save.</summary>
    private bool AnySlotValid => slots.Any(slot => slot is not null);

    /// <summary>The marker field no longer holds the decoded image's words.</summary>
    private void MarkerRewritten() => decodedMarkerCarry = null;

    /// <summary>A copy of <paramref name="saved"/> as slot <paramref name="slot"/>, sharing no array with it.</summary>
    private static SuperMetroidSaveSlot Detached(SuperMetroidSaveSlot saved, int slot) => saved with
    {
        Slot = slot,
        EventBytes = saved.EventBytes.ToArray(),
        BossBytes = saved.BossBytes.ToArray(),
        RoomChozoBytes = saved.RoomChozoBytes.ToArray(),
        CollectedItemBytes = saved.CollectedItemBytes.ToArray(),
        OpenedDoorBytes = saved.OpenedDoorBytes.ToArray(),
        UsedSaveStationBytes = saved.UsedSaveStationBytes.ToArray(),
        MapStationBytes = saved.MapStationBytes.ToArray(),
        ExploredMapBytes = saved.ExploredMapBytes.ToArray(),
    };
}
