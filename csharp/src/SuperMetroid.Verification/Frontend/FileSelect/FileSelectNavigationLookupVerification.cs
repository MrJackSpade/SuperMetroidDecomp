using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Verification.Frontend.FileSelect;

/// <summary>Compares file-select cursor movement with the original filtered choice lists and cartridge branch data.</summary>
internal static class FileSelectNavigationLookupVerification
{
    /// <summary>Checks every set of occupied slots and directional input priority against the original filtered lists.</summary>
    internal static void VerifyFileSelectSourceNavigation(ISnesAddressSpace rom)
    {
        // Complete filtered lists produced by the old source/clear code in
        // 3058abb7. Keep the ordinal-list representation only in this oracle.
        int[][] original = [[3], [0, 3], [1, 3], [0, 1, 3],
            [2, 3], [0, 2, 3], [1, 2, 3], [0, 1, 2, 3]];
        for (int slot = 0; slot < 3; slot++)
        {
            AssertEqual((byte)(1 << slot), FileSelectSourceNavigationRomByte(rom, 0x81975b + slot), "native copy occupancy bit");
            AssertEqual((byte)(1 << slot), FileSelectSourceNavigationRomByte(rom, 0x819bec + slot), "native clear occupancy bit");
        }
        // The oracle's lists are indexed by the cartridge's occupancy bits, one per slot from A; the production
        // navigation takes the set of occupied slots those bits name. A set cannot name a fourth slot, so the former
        // out-of-range mask rejection has nothing left to reject.
        for (int occupied = 0; occupied < original.Length; occupied++)
        {
            int mask = occupied;
            HashSet<SaveSlot> slots = [.. SaveSlots.All.Where(slot => (mask & (1 << SaveSlots.StoreNumber(slot))) != 0)];
            VerifyOriginalSubmenuChoices(original[mask], (selected, pressed) =>
                FileSelectDataNavigation.MoveSourceOrClear(selected, slots, pressed),
                $"source/clear occupancy={string.Join(",", slots)}");
        }
        Console.WriteLine("Source/clear navigation: all eight original choice lists, 20 selectable states and input priority pass.");
    }

    /// <summary>Reads a native source/clear occupancy bit for the independent navigation oracle.</summary>
    private static byte FileSelectSourceNavigationRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks that copy destinations omit only the source slot for both cursor directions.</summary>
    internal static void VerifyFileSelectDestinationNavigation(ISnesAddressSpace rom)
    {
        // Original destination lists exclude just the source, even if another
        // slot is empty. Native $81:9843/$81:98A8 compare directly with that source.
        int[][] original = [[1, 2, 3], [0, 2, 3], [0, 1, 3]];
        AssertEqual((byte)0xec, FileSelectDestinationNavigationRomByte(rom, 0x819843), "native downward source-slot CPX");
        AssertEqual((byte)0xec, FileSelectDestinationNavigationRomByte(rom, 0x8198a8), "native upward source-slot CPX");
        AssertEqual(ReadVerificationWord(rom, 0x819844), ReadVerificationWord(rom, 0x8198a9),
            "both directions exclude the same live source");
        AssertEqual((ushort)4, ReadVerificationWord(rom, 0x81983f), "native destination end boundary");
        // The copy source is a typed slot, so the former out-of-range source rejection has nothing left to reject.
        foreach (SaveSlot source in SaveSlots.All)
        {
            VerifyOriginalSubmenuChoices(original[SaveSlots.StoreNumber(source)], (selected, pressed) =>
                FileSelectDataNavigation.MoveCopyDestination(selected, source, pressed),
                $"copy destination source={source}");
        }
        Console.WriteLine("Copy destination navigation: all three original choice lists, nine selectable states and input priority pass.");
    }

    /// <summary>Reads a native destination-navigation opcode for comparison with the compiled selection rule.</summary>
    private static byte FileSelectDestinationNavigationRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Uses the former ordinal-list behavior to check each selectable row and rejected row under directional inputs.</summary>
    private static void VerifyOriginalSubmenuChoices(int[] original,
        Func<FileSelectItem, ControllerButton, FileSelectItem> actual, string description)
    {
        // The complete four directional equivalence classes, also with every
        // irrelevant controller bit set, cover the helper's input masking.
        foreach (ControllerButton direction in new[] { ControllerButton.None, ControllerButton.Up,
            ControllerButton.Down, ControllerButton.Up | ControllerButton.Down })
        foreach (ControllerButton otherBits in new[] { ControllerButton.None, ControllerButtons.All & ~(ControllerButton.Up | ControllerButton.Down) })
        {
            ControllerButton pressed = direction | otherBits;
            for (int ordinal = 0; ordinal < original.Length; ordinal++)
            {
                // Original MoveSubmenuSelection clamps an index into the filtered
                // list; it does not scan slot identities like the replacement.
                int nextOrdinal = ordinal;
                if ((pressed & ControllerButton.Up) != 0) nextOrdinal = Math.Max(0, ordinal - 1);
                else if ((pressed & ControllerButton.Down) != 0) nextOrdinal = Math.Min(original.Length - 1, ordinal + 1);
                AssertEqual(FileSelectItemRows.DataItem(original[nextOrdinal]), actual(FileSelectItemRows.DataItem(original[ordinal]), pressed),
                    $"{description} row={original[ordinal]} input={pressed}");
            }
            FileSelectItem[] selectable = original.Select(FileSelectItemRows.DataItem).ToArray();
            foreach (FileSelectItem rejected in Enum.GetValues<FileSelectItem>())
            {
                if (selectable.Contains(rejected)) continue;
                try
                {
                    actual(rejected, pressed);
                    throw new InvalidOperationException("Unselectable submenu item was accepted.");
                }
                catch (InvalidDataException error)
                {
                    AssertEqual($"Submenu item {rejected} is not selectable.", error.Message,
                        $"{description} preserves rejection even without directional input");
                }
            }
        }
    }

    /// <summary>Checks main-menu wraparound and hidden-row rejection with and without existing save files.</summary>
    internal static void VerifyFileSelectMainNavigation()
    {
        // Independent original managed lists from b9e1e0ff, cross-checked against
        // native $81:A25E..A2AF. Preserve this former representation only as the oracle.
        foreach (bool hasSave in new[] { false, true })
        {
            int[] original = hasSave ? [0, 1, 2, 3, 4, 5] : [0, 1, 2, 5];
            for (int ordinal = 0; ordinal < original.Length; ordinal++)
            foreach (bool down in new[] { false, true })
            {
                FileSelectItem expected = FileSelectItemRows.MainItem(original[(ordinal + (down ? 1 : -1) + original.Length) % original.Length]);
                AssertEqual(expected, FileSelectMainNavigation.Move(FileSelectItemRows.MainItem(original[ordinal]), hasSave, down ? VerticalDirection.Down : VerticalDirection.Up),
                    $"main navigation saves={hasSave} row={original[ordinal]} down={down}");
            }
            FileSelectItem[] visible = original.Select(FileSelectItemRows.MainItem).ToArray();
            foreach (FileSelectItem selected in Enum.GetValues<FileSelectItem>())
            {
                if (visible.Contains(selected)) continue;
                foreach (bool down in new[] { false, true })
                {
                    try
                    {
                        FileSelectMainNavigation.Move(selected, hasSave, down ? VerticalDirection.Down : VerticalDirection.Up);
                        throw new InvalidOperationException("Invalid main selection was accepted.");
                    }
                    catch (InvalidDataException error)
                    {
                        AssertEqual($"File-select item {selected} is not currently visible.", error.Message,
                            "main navigation preserves rejection diagnostic");
                    }
                }
            }
        }
        Console.WriteLine("File-select navigation: all 20 original visible transitions and hidden/invalid selection rejection pass.");
    }
}
