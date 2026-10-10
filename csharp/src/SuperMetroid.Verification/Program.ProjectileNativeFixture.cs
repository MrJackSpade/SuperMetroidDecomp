using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Totals timed records in a native bank-$93 projectile program until its terminator, following any list jumps.</summary>
    /// <param name="rom">Retail address space supplying the pointer table and native instruction records.</param>
    /// <param name="selector">Address of the word that selects the projectile instruction list.</param>
    /// <returns>The total number of timed frames before the program terminates.</returns>
    private static int NativeProjectileLifetime(SuperMetroidAddressSpace rom, int selector)
    {
        ushort Word(int a) => (ushort)(rom.ReadByte(a) | rom.ReadByte(a + 1) << 8);
        int pointer = Word(selector), ticks = 0;
        var seen = new HashSet<int>();
        while (seen.Add(pointer))
        {
            int a = 0x930000 | pointer;
            ushort command = Word(a);
            if (command == 0x822f) return ticks;
            if (command == 0x8239) { pointer = Word(a + 2); continue; }
            AssertTrue(command > 0 && command < 0x8000, "Finite native explosion frame");
            ticks += command;
            pointer += 8;
        }
        throw new InvalidDataException("Expected finite native explosion program.");
    }

    // Keep synthetic artwork and terrain, but use the cartridge-selected programs.
    // Only sprite-reference fields are replaced; timing/damage/collision stay native.
    /// <summary>Seeds a synthetic projectile bus with retail bank-$93 programs while replacing their sprite references with fixture artwork.</summary>
    /// <param name="bus">Mutable test address space receiving native program data and synthetic presentation pointers.</param>
    /// <returns>The retail cartridge import used as the source for native program bytes and reference lookups.</returns>
    private static SuperMetroid.AssetExtraction.CartridgeImportAddressSpace SeedNativeProjectileFixture(TestAddressSpace bus)
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        ushort Word(int a) => (ushort)(rom.ReadByte(a) | rom.ReadByte(a + 1) << 8);
        void Art(int entry, ushort map)
        {
            var seen = new HashSet<int>();
            while (seen.Add(entry))
            {
                ushort command = Word(0x930000 | entry);
                if (command == 0x822f) break;
                if (command == 0x8239) { entry = Word(0x930000 | (entry + 2)); continue; }
                AssertTrue(command > 0 && command < 0x8000, "Native fixture program reaches a timed record");
                WriteTestWord(bus, 0x930000 | (entry + 2), map);
                entry += 8;
            }
        }
        // Complete the installed visual-binding domain before applying synthetic art.
        foreach (ushort record in SuperMetroid.Core.Game.SamusProjectileRadiusDefinitions.TimedRecordPointers)
            WriteTestWord(bus, 0x930000 | (record + 2), Word(0x930000 | (record + 2)));
        foreach (int table in new[] { 0x9383c1, 0x9383d9 })
        for (int beam = 0; beam < 12; beam++)
        for (int direction = 0; direction < 10; direction++)
            Art(Word(0x930000 | (Word(table + beam * 2) + 2 + direction * 2)), 0xa24d);
        for (int direction = 0; direction < 10; direction++)
        {
            Art(Word(0x938643 + direction * 2), 0xa25b);
            Art(Word(0x938659 + direction * 2), 0xa262);
        }
        Art(Word(0x93866f), 0xa117);
        foreach (int pointer in new[] { 0x93867b, 0x93867f, 0x938693 }) Art(Word(pointer), 0xa254);
        return rom;
    }
}
