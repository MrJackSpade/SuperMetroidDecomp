using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Captures the retail-ROM main and CLEAR screens without touching disk SRAM.</summary>
internal static class FileSelectDataManagementAudit
{
    public static int Run(string romPath, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var saveRam = new SuperMetroidSaveRam(bus);
        saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot
        {
            Health = 99,
            MaxHealth = 99,
            GameTimeMinutes = 42,
        });

        var menu = new FileSelectMenuState(bus);
        for (int frame = 0; frame < 15; frame++)
            menu.Step(0);
        Capture(menu, outputDirectory, "file-select-main.png");

        // Main index four is DATA CLEAR. Release between presses so the same native
        // rising-edge latch used by the playable frontend drives this diagnostic.
        for (int move = 0; move < 4; move++)
            Pulse(menu, SnesButton.Down);
        Pulse(menu, SnesButton.A);
        for (int guard = 0; menu.Phase != FileSelectPhase.ClearSelectSlot && guard < 40; guard++)
            menu.Step(0);
        if (menu.Phase != FileSelectPhase.ClearSelectSlot)
            throw new InvalidDataException($"CLEAR screen did not finish its fade: {menu.Phase}.");
        Capture(menu, outputDirectory, "file-clear-select.png");

        Pulse(menu, SnesButton.A);
        if (menu.Phase != FileSelectPhase.ClearConfirm)
            throw new InvalidDataException($"CLEAR confirmation did not open: {menu.Phase}.");
        Capture(menu, outputDirectory, "file-clear-confirm.png");
        Console.WriteLine($"Captured file-select data menus in {Path.GetFullPath(outputDirectory)}.");
        return 0;
    }

    private static void Pulse(FileSelectMenuState menu, SnesButton button)
    {
        menu.Step((ushort)button);
        menu.Step(0);
    }

    private static void Capture(FileSelectMenuState menu, string directory, string name) =>
        PngWriter.WriteRgba(Path.Combine(directory, name), 256, 224, menu.Render());
}
