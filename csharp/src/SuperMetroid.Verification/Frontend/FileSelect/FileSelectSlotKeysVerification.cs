using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Verification.Frontend.FileSelect;

/// <summary>
/// Checks that file select addresses its save slots by <see cref="SaveSlot"/> (#1976): each slot's letter is pictured under
/// the slot's own name, and only a slot row names a selected slot, a command row failing instead of reading as slot C.
/// </summary>
internal static class FileSelectSlotKeysVerification
{
    /// <summary>Checks the letter pictures' names and which rows name a save slot.</summary>
    internal static void VerifyFileSelectSlotKeys()
    {
        AssertTrue(SaveSlots.All.Select(FileSelectParts.LetterFrame)
                .SequenceEqual(["FileSelect.Letter.A", "FileSelect.Letter.B", "FileSelect.Letter.C"]),
            "each slot's letter is pictured under the slot's name");
        AssertThrows<ArgumentOutOfRangeException>(() => FileSelectParts.LetterFrame((SaveSlot)byte.MaxValue), "an undefined slot has no letter");
        AssertEqual(SaveSlot.A, FileSelectItemRows.SaveSlotOf(FileSelectItem.SlotA), "slot A's row names slot A");
        AssertEqual(SaveSlot.B, FileSelectItemRows.SaveSlotOf(FileSelectItem.SlotB), "slot B's row names slot B");
        AssertEqual(SaveSlot.C, FileSelectItemRows.SaveSlotOf(FileSelectItem.SlotC), "slot C's row names slot C");
        foreach (FileSelectItem command in new[] { FileSelectItem.Copy, FileSelectItem.Clear, FileSelectItem.Exit })
            AssertThrows<InvalidDataException>(() => FileSelectItemRows.SaveSlotOf(command), $"the {command} row names no save slot");
        Console.WriteLine("File-select slot keys: letters pictured by slot name; only slot rows name a selected slot.");
    }
}
