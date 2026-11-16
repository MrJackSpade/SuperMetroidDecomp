namespace SuperMetroid.Core.Saves;

/// <summary>The three save slots in the order the file select lists them.</summary>
public static class SaveSlots
{
    /// <summary>Every save slot, top to bottom.</summary>
    public static IReadOnlyList<SaveSlot> All { get; } = [SaveSlot.A, SaveSlot.B, SaveSlot.C];
}
