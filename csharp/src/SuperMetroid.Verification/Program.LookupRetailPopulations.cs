using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    // Independent population identities captured from the original Sources in
    // 80cc06ebb64ec54caabbd686a9a286fda0d16b3a, before replacing that storage.
    // Placement field values and terminating words are read from the pinned ROM.
    /// <summary>Sorted bank-$8F pointers independently captured for all 284 retail room-population lists.</summary>
    private static readonly ushort[] OriginalRetailPopulationPointers =
    [
        0x8000, 0x8026, 0x8058, 0x805A, 0x8104, 0x81CC, 0x81D4, 0x81DC, 0x81FC, 0x81FE, 0x8230, 0x8238,
        0x823A, 0x823C, 0x823E, 0x8246, 0x8248, 0x8250, 0x825E, 0x826C, 0x826E, 0x830C, 0x83B6, 0x83D0,
        0x83F6, 0x83FE, 0x8412, 0x8420, 0x8428, 0x8430, 0x8432, 0x843A, 0x8442, 0x8444, 0x844C, 0x8478,
        0x8480, 0x8482, 0x8484, 0x8486, 0x848E, 0x84D8, 0x84EC, 0x84F4, 0x8526, 0x8540, 0x8548, 0x8550,
        0x8558, 0x85E4, 0x8634, 0x8642, 0x864A, 0x8664, 0x867E, 0x86E6, 0x8754, 0x878C, 0x87A6, 0x87AE,
        0x87B0, 0x87D0, 0x87D8, 0x87E0, 0x87E8, 0x8802, 0x880A, 0x8824, 0x882C, 0x8834, 0x8836, 0x8844,
        0x884C, 0x8854, 0x886E, 0x8876, 0x887E, 0x8880, 0x88BE, 0x88D8, 0x891C, 0x891E, 0x896E, 0x8976,
        0x8996, 0x89A4, 0x89F4, 0x8A02, 0x8A2E, 0x8A3C, 0x8A3E, 0x8A46, 0x8A54, 0x8A5C, 0x8ACA, 0x8AD2,
        0x8ADA, 0x8AE2, 0x8AE4, 0x8AF2, 0x8AFA, 0x8B14, 0x8B22, 0x8B24, 0x8B2C, 0x8B46, 0x8B4E, 0x8B9E,
        0x8BAC, 0x8BB4, 0x8BC8, 0x8BF4, 0x8BFC, 0x8C04, 0x8C0C, 0x8C14, 0x8C1C, 0x8C2A, 0x8C32, 0x8C34,
        0x8C36, 0x8C3E, 0x8C4C, 0x8C5A, 0x8C6E, 0x8C82, 0x8C8A, 0x8CB0, 0x8CCA, 0x8CD2, 0x8CD4, 0x8D1E,
        0x8D56, 0x8D58, 0x8D7E, 0x8D80, 0x8D88, 0x8D96, 0x8D98, 0x8D9A, 0x8D9C, 0x8DA4, 0x8DA6, 0x8DD8,
        0x8DE0, 0x8DE8, 0x8DEA, 0x8DEC, 0x8DF4, 0x8DFC, 0x8E04, 0x8E12, 0x8E3E, 0x8E82, 0x8E90, 0x8E98,
        0x8EA6, 0x8EBA, 0x8ED4, 0x8ED6, 0x8F38, 0x8F3A, 0x8F3C, 0x8F7A, 0x8F7C, 0x8FD2, 0x8FDA, 0x9036,
        0x90C8, 0x90D0, 0x9108, 0x9110, 0x9118, 0x918C, 0xC215, 0xC22F, 0xC231, 0xC245, 0xC247, 0xC27F,
        0xC281, 0xC28F, 0xC291, 0xC2B1, 0xC2B3, 0xC2BB, 0xC2BD, 0xC2BF, 0xC2C7, 0xC2C9, 0xC2D1, 0xC2FD,
        0xC2FF, 0xC319, 0xC321, 0xC323, 0xC337, 0xC33F, 0xC34D, 0xC355, 0xC357, 0xC35F, 0xC36D, 0xC375,
        0xC37D, 0xC3DF, 0xC3E1, 0xC42B, 0xC445, 0xC47D, 0xC48B, 0xC499, 0xC49B, 0xC4A9, 0xC4BD, 0xC4EF,
        0xC503, 0xC53B, 0xC54F, 0xC551, 0xC553, 0xC561, 0xC563, 0xC571, 0xC57F, 0xC581, 0xC589, 0xC591,
        0xC593, 0xC595, 0xC597, 0xC5DB, 0xC5DD, 0xC5EB, 0xC5F9, 0xC5FB, 0xC5FD, 0xC611, 0xC619, 0xC61B,
        0xC6AD, 0xC6E5, 0xC6ED, 0xC6EF, 0xC703, 0xC70B, 0xC755, 0xC75D, 0xC765, 0xC76D, 0xC76F, 0xC771,
        0xC773, 0xC79F, 0xC7A7, 0xC7AF, 0xC7B7, 0xC7B9, 0xC7BB, 0xC7E1, 0xC7E9, 0xC7F7, 0xC805, 0xC813,
        0xC821, 0xC823, 0xC831, 0xC839, 0xC841, 0xC84F, 0xC857, 0xC86B, 0xC873, 0xC87B, 0xC889, 0xC897,
        0xC8A5, 0xC8B3, 0xC8BB, 0xC8BD, 0xC8BF, 0xC8C1, 0xC8C3, 0xC8C5,
    ];

    /// <summary>Runs identity, record-field, and zero-header termination comparisons for the complete retail population catalog.</summary>
    /// <param name="rom">Retail address space containing the original bank-$8F placement lists.</param>
    private static void VerifyRetailPopulationMappings(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyRetailPopulationIdentities), () => VerifyRetailPopulationIdentities());
        Suite(nameof(VerifyRetailPopulationHeaders), () => VerifyRetailPopulationHeaders(rom));
        Suite(nameof(VerifyRetailPopulationX), () => VerifyRetailPopulationX(rom));
        Suite(nameof(VerifyRetailPopulationY), () => VerifyRetailPopulationY(rom));
        Suite(nameof(VerifyRetailPopulationArguments), () => VerifyRetailPopulationArguments(rom));
        Suite(nameof(VerifyRetailPopulationTermination), () => VerifyRetailPopulationTermination(rom));
    }

    /// <summary>Checks that compiled population identities exactly preserve the independently captured retail pointer set.</summary>
    private static void VerifyRetailPopulationIdentities()
    {
        var original = OriginalRetailPopulationPointers.ToHashSet();
        AssertEqual(284, original.Count, "original population identities are unique");
        AssertTrue(RoomPlmPopulationDefinitions.Pointers.SequenceEqual(OriginalRetailPopulationPointers),
            "population enumeration retains original sorted identities");
        AssertTrue(RoomStateDefinitions.All.Select(state => state.PlmPointer).ToHashSet().SetEquals(original),
            "room state consumers select exactly the independently captured population identities");
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            bool expected = original.Contains(pointer);
            AssertEqual(expected, RoomPlmPopulationDefinitions.TryPlace(pointer, null),
                $"population identity {pointer:X4}");
            if (expected)
                AssertEqual(pointer, RoomPlmPopulationDefinition.FromCompiled(pointer).Pointer,
                    "typed population preserves selected source identity");
            else
            {
                bool invoked = false;
                AssertTrue(!RoomPlmPopulationDefinitions.TryPlace(pointer, (_, _, _, _) => invoked = true) && !invoked,
                    "unknown population performs no setup operations");
                AssertThrows<InvalidDataException>(() => RoomPlmPopulationDefinition.FromCompiled(pointer),
                    "unknown population is rejected by the production factory");
            }
        }
        AssertThrows<ArgumentNullException>(() => RoomPlmPopulationDefinitions.Place(0x8000, null!),
            "population dispatch requires a placement consumer");
    }

    /// <summary>Compares every compiled placement header with its corresponding retail record.</summary>
    /// <param name="rom">Retail address space containing the original placement records.</param>
    private static void VerifyRetailPopulationHeaders(SuperMetroidAddressSpace rom) => VerifyRetailPopulationField(rom, 0);
    /// <summary>Compares every compiled horizontal block coordinate with its corresponding retail record.</summary>
    /// <param name="rom">Retail address space containing the original placement records.</param>
    private static void VerifyRetailPopulationX(SuperMetroidAddressSpace rom) => VerifyRetailPopulationField(rom, 1);
    /// <summary>Compares every compiled vertical block coordinate with its corresponding retail record.</summary>
    /// <param name="rom">Retail address space containing the original placement records.</param>
    private static void VerifyRetailPopulationY(SuperMetroidAddressSpace rom) => VerifyRetailPopulationField(rom, 2);
    /// <summary>Compares every compiled room argument with its corresponding retail record.</summary>
    /// <param name="rom">Retail address space containing the original placement records.</param>
    private static void VerifyRetailPopulationArguments(SuperMetroidAddressSpace rom) => VerifyRetailPopulationField(rom, 3);
    /// <summary>Confirms each compiled list ends at the same zero-header record as its retail source.</summary>
    /// <param name="rom">Retail address space containing the original placement records and terminators.</param>
    private static void VerifyRetailPopulationTermination(SuperMetroidAddressSpace rom) => VerifyRetailPopulationField(rom, 4);

    /// <summary>Compares one selected placement field or the terminating record across all captured population lists.</summary>
    /// <param name="rom">Retail address space used to read each source list.</param>
    /// <param name="field">Selection code for header, X, Y, room argument, or zero-header termination.</param>
    private static void VerifyRetailPopulationField(SuperMetroidAddressSpace rom, int field)
    {
        int total = 0;
        foreach (ushort pointer in OriginalRetailPopulationPointers)
        {
            ReadOnlySpan<RoomPlmPlacement> actual = RoomPlmPopulationDefinition.FromCompiled(pointer).Placements.Span;
            int count = 0;
            int address = 0x8f0000 | pointer;
            while (ReadSamusEaterPlmWord(rom, address) != 0)
            {
                AssertTrue(count < 256 && count < actual.Length, "native placement has a corresponding bounded operation");
                RoomPlmPlacement placement = actual[count];
                int expected = field switch
                {
                    0 => ReadSamusEaterPlmWord(rom, address),
                    1 => rom.ReadByte(address + 2),
                    2 => rom.ReadByte(address + 3),
                    3 => ReadSamusEaterPlmWord(rom, address + 4),
                    _ => 0,
                };
                int result = field switch
                {
                    0 => placement.Header.Header,
                    1 => placement.BlockX,
                    2 => placement.BlockY,
                    3 => placement.RoomArgument,
                    _ => 0,
                };
                if (field != 4)
                    AssertEqual(expected, result, $"population {pointer:X4} operation {count} field {field}");
                address += 6;
                count++;
            }
            if (field == 4)
                AssertEqual(count, actual.Length, $"population {pointer:X4} returns exactly at the native zero header");
            total += count;
        }
        AssertEqual(941, total, $"complete original placement domain for field {field}");
    }
}
