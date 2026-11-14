namespace SuperMetroid.Core.Saves;

/// <summary>
/// The three save slots in the order the file select lists them, and the number <see cref="SaveSlotStore"/> keeps each
/// slot's save under.
/// </summary>
public static class SaveSlots
{
    /// <summary>Every save slot, top to bottom.</summary>
    public static IReadOnlyList<SaveSlot> All { get; } = [SaveSlot.A, SaveSlot.B, SaveSlot.C];

    /// <summary>The number <see cref="SaveSlotStore"/> keeps <paramref name="slot"/>'s save under.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="slot"/> is not a save slot.</exception>
    public static int StoreNumber(SaveSlot slot) => slot switch
    {
        SaveSlot.A => 0,
        SaveSlot.B => 1,
        SaveSlot.C => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Undefined save slot."),
    };

    /// <summary>The slot whose save <see cref="SaveSlotStore"/> keeps under <paramref name="storeNumber"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The store keeps no slot under <paramref name="storeNumber"/>.</exception>
    public static SaveSlot AtStoreNumber(int storeNumber) => storeNumber switch
    {
        0 => SaveSlot.A,
        1 => SaveSlot.B,
        2 => SaveSlot.C,
        _ => throw new ArgumentOutOfRangeException(nameof(storeNumber), storeNumber, "The save store keeps no slot under this number."),
    };
}
