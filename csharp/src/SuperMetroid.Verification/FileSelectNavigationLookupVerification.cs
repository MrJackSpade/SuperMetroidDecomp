using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static DoorDefinitionsVerification;
using SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Compares file-select cursor movement with the original filtered choice lists and cartridge branch data.</summary>
internal static class FileSelectNavigationLookupVerification
{
    /// <summary>Checks all source and clear occupancy masks, directional input priority, and invalid-mask rejection.</summary>
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
        for (int occupied = 0; occupied < original.Length; occupied++)
        {
            int mask = occupied;
            VerifyOriginalSubmenuChoices(original[mask], (selected, pressed) =>
                FileSelectDataNavigation.MoveSourceOrClear(selected, mask, pressed),
                $"source/clear occupancy={mask}");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 8, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() =>
                FileSelectDataNavigation.MoveSourceOrClear(3, invalid, SnesButton.None),
                "unsupported occupancy mask");
        Console.WriteLine("Source/clear navigation: all eight original choice lists, 20 selectable states, input priority and rejection pass.");
    }

    /// <summary>Reads a native source/clear occupancy bit for the independent navigation oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for file select source navigation")]
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
        for (int source = 0; source < original.Length; source++)
        {
            int slot = source;
            VerifyOriginalSubmenuChoices(original[slot], (selected, pressed) =>
                FileSelectDataNavigation.MoveCopyDestination(selected, slot, pressed),
                $"copy destination source={slot}");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() =>
                FileSelectDataNavigation.MoveCopyDestination(3, invalid, SnesButton.None),
                "unsupported copy source");
        Console.WriteLine("Copy destination navigation: all three original choice lists, nine selectable states, input priority and rejection pass.");
    }

    /// <summary>Reads a native destination-navigation opcode for comparison with the compiled selection rule.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for file select destination navigation")]
    private static byte FileSelectDestinationNavigationRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Uses the former ordinal-list behavior to check each selectable row and rejected row under directional inputs.</summary>
    private static void VerifyOriginalSubmenuChoices(int[] original,
        Func<int, SnesButton, int> actual, string description)
    {
        // The complete four directional equivalence classes, also with every
        // irrelevant controller bit set, cover the helper's input masking.
        foreach (SnesButton direction in new[] { SnesButton.None, SnesButton.Up,
            SnesButton.Down, SnesButton.Up | SnesButton.Down })
        foreach (ushort otherBits in new ushort[] { 0, unchecked((ushort)~(ushort)(SnesButton.Up | SnesButton.Down)) })
        {
            SnesButton pressed = direction | (SnesButton)otherBits;
            for (int ordinal = 0; ordinal < original.Length; ordinal++)
            {
                // Original MoveSubmenuSelection clamps an index into the filtered
                // list; it does not scan slot identities like the replacement.
                int nextOrdinal = ordinal;
                if ((pressed & SnesButton.Up) != 0) nextOrdinal = Math.Max(0, ordinal - 1);
                else if ((pressed & SnesButton.Down) != 0) nextOrdinal = Math.Min(original.Length - 1, ordinal + 1);
                AssertEqual(original[nextOrdinal], actual(original[ordinal], pressed),
                    $"{description} row={original[ordinal]} input={pressed}");
            }
            foreach (int rejected in new[] { int.MinValue, -1, 0, 1, 2, 4, int.MaxValue })
            {
                if (Array.IndexOf(original, rejected) >= 0) continue;
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
                int expected = original[(ordinal + (down ? 1 : -1) + original.Length) % original.Length];
                AssertEqual(expected, FileSelectMainNavigation.Move(original[ordinal], hasSave, down),
                    $"main navigation saves={hasSave} row={original[ordinal]} down={down}");
            }
            foreach (int selected in new[] { int.MinValue, -1, 3, 4, 6, int.MaxValue })
            {
                if (Array.IndexOf(original, selected) >= 0) continue;
                foreach (bool down in new[] { false, true })
                {
                    try
                    {
                        FileSelectMainNavigation.Move(selected, hasSave, down);
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
