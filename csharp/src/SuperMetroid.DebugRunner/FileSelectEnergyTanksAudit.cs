using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static class FileSelectEnergyTanksAudit
{
    internal static int Run(string installationRoot)
    {
        // Fresh arrays only. No persisted save is opened or written by this fixture.
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var saves = new SuperMetroidSaveRam(bus);
        saves.SaveSlot(0, new SuperMetroidSaveSnapshot { Health = 899, MaxHealth = 1499 });
        saves.SaveSlot(1, new SuperMetroidSaveSnapshot { Health = 99, MaxHealth = 99 });
        var maps = new GameInstallation(installationRoot).LoadMaps();
        var menu = new FileSelectMenuState(bus, mapPresentation: maps);
        for (int frame = 0; frame < 16; frame++) menu.Step(0);
        Check(false);
        for (int move = 0; move < 4; move++) { menu.Step((ushort)SnesButton.Down); menu.Step(0); }
        menu.Step((ushort)SnesButton.A); menu.Step(0);
        for (int frame = 0; menu.Phase != FileSelectPhase.ClearSelectSlot && frame < 40; frame++) menu.Step(0);
        if (menu.Phase != FileSelectPhase.ClearSelectSlot) throw new InvalidDataException("Expected clear selection page.");
        Check(true);
        Console.WriteLine("PASS: main and data-management menus show 8 filled + 6 empty tanks at native two-row positions; 99-health and empty saves have none; filled and empty tank pixels are visible.");
        return 0;

        void Check(bool management)
        {
            var anchor = maps.FileSelect.Slot(management, 0).EnergyAnchor;
            for (int tank = 0; tank < 14; tank++)
            {
                int x = anchor.X + 4 + tank % 7;
                int y = anchor.Y + (tank < 7 ? 1 : 0);
                ushort expected = (ushort)(tank < 8 ? 0x98 : 0x99);
                if (menu.BackgroundTilemap[y * 32 + x] != expected)
                    throw new InvalidDataException($"{(management ? "Data" : "Main")} tank {tank}: expected {expected:X4}, actual {menu.BackgroundTilemap[y * 32 + x]:X4}.");
            }
            for (int slot = 1; slot <= 2; slot++)
            {
                var unused = maps.FileSelect.Slot(management, slot).EnergyAnchor;
                for (int tank = 0; tank < 14; tank++)
                {
                    ushort cell = menu.BackgroundTilemap[(unused.Y + (tank < 7 ? 1 : 0)) * 32 + unused.X + 4 + tank % 7];
                    if ((cell & 0x3ff) is 0x98 or 0x99) throw new InvalidDataException("Unowned tank was drawn.");
                }
            }
            var ppu = (MenuPpuState)typeof(FileSelectMenuState).GetField("ppu", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(menu)!;
            var original = menu.BackgroundTilemap.ToArray();
            var withTanks = menu.Render();
            var erased = original.ToArray();
            for (int tank = 0; tank < 14; tank++) erased[(anchor.Y + (tank < 7 ? 1 : 0)) * 32 + anchor.X + 4 + tank % 7] = 0xf;
            ppu.Vram.ExecuteWordTransfer(erased, MenuPpuState.Bg1TilemapWord, 1);
            var withoutTanks = menu.Render();
            ppu.Vram.ExecuteWordTransfer(original, MenuPpuState.Bg1TilemapWord, 1);
            foreach (int tank in new[] { 0, 8 })
            {
                int x = (anchor.X + 4 + tank % 7) * 8;
                int y = (anchor.Y + (tank < 7 ? 1 : 0)) * 8;
                bool visible = false;
                for (int dy = 0; dy < 8; dy++) for (int dx = 0; dx < 8; dx++)
                    visible |= withTanks[(y + dy) * 256 + x + dx] != withoutTanks[(y + dy) * 256 + x + dx];
                if (!visible) throw new InvalidDataException($"Tank {tank} has no visible pixels.");
            }
        }
    }
}
