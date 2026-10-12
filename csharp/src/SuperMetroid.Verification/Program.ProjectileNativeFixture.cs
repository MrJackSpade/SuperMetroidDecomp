using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for projectile lifetime")]
    private static int NativeProjectileLifetime(SuperMetroidAddressSpace rom, int selector)
    {
        int pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord(selector), ticks = 0;
        var seen = new HashSet<int>();
        while (seen.Add(pointer))
        {
            int a = 0x930000 | pointer;
            ushort command = CartridgeImportSource.Require(rom).ReadCartridgeWord(a);
            if (command == 0x822f) return ticks;
            if (command == 0x8239) { pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((a + 2)); continue; }
            AssertTrue(command is > 0 and < 0x8000, "Finite native explosion frame");
            ticks += command;
            pointer += 8;
        }
        throw new InvalidDataException("Expected finite native explosion program.");
    }

    // Keep synthetic artwork and terrain, but use the cartridge-selected programs.
    // Only sprite-reference fields are replaced; timing/damage/collision stay native.
    [CpuBusBoundary("exact bus test: retail ROM oracle for projectile fixture")]
    private static SuperMetroid.AssetExtraction.CartridgeImportAddressSpace SeedNativeProjectileFixture(TestAddressSpace bus)
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        void Art(int entry, ushort map)
        {
            var seen = new HashSet<int>();
            while (seen.Add(entry))
            {
                ushort command = rom.ReadCartridgeWord((0x930000 | entry));
                if (command == 0x822f) break;
                if (command == 0x8239) { entry = rom.ReadCartridgeWord((0x930000 | (entry + 2))); continue; }
                AssertTrue(command is > 0 and < 0x8000, "Native fixture program reaches a timed record");
                WriteTestWord(bus, 0x930000 | (entry + 2), map);
                entry += 8;
            }
        }
        // Complete the installed visual-binding domain before applying synthetic art.
        foreach (ushort record in SuperMetroid.Core.Game.SamusProjectileRadiusDefinitions.TimedRecordPointers)
            WriteTestWord(bus, 0x930000 | (record + 2), rom.ReadCartridgeWord((0x930000 | (record + 2))));
        foreach (int table in new[] { 0x9383c1, 0x9383d9 })
        for (int beam = 0; beam < 12; beam++)
        for (int direction = 0; direction < 10; direction++)
            Art(rom.ReadCartridgeWord((0x930000 | (CartridgeImportSource.Require(rom).ReadCartridgeWord(table + beam * 2) + 2 + direction * 2))), 0xa24d);
        for (int direction = 0; direction < 10; direction++)
        {
            Art(rom.ReadCartridgeWord((0x938643 + direction * 2)), 0xa25b);
            Art(rom.ReadCartridgeWord((0x938659 + direction * 2)), 0xa262);
        }
        Art(rom.ReadCartridgeWord(0x93866f), 0xa117);
        foreach (int pointer in new[] { 0x93867b, 0x93867f, 0x938693 }) Art(rom.ReadCartridgeWord(pointer), 0xa254);
        return rom;
    }
}
