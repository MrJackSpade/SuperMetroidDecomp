using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

/// <summary>Ordinary battery-save/reload boundary after the verified Flash generator.</summary>
internal static class FlashSaveReloadProbe
{
    public static void Run(ISnesAddressSpace bus, SuperMetroidRuntime runtime, string[] native)
    {
        var samus = runtime.Samus ?? throw new InvalidDataException("Missing generated actor.");
        var saves = new SuperMetroidSaveRam(bus);
        // Use only the disposable ROM bus's in-memory SRAM. No player file is touched.
        // Station zero is a real load destination, not a debugger-state restoration.
        saves.SaveSlot(0, SuperMetroidSaveSnapshot.Capture(samus, runtime.System, area: 0, saveStation: 0));
        string saved = $"{samus.SharedShineTimer:X4},{samus.CrystalFlash.SpecialPaletteType:X4}";
        if (saved != string.Join(',', native[27..29]))
            throw new InvalidDataException("Writing a save incorrectly cancelled retained Flash.");
        ushort health = samus.Health;
        samus.Health = 0;
        var slot = saves.ReadSlot(0) ?? throw new InvalidDataException("Save checksum rejected.");
        runtime.InitializeSavedGame(slot);
        var loaded = runtime.Samus ?? throw new InvalidDataException("Reload omitted Samus.");
        string reloaded = $"{loaded.SharedShineTimer:X4},{loaded.CrystalFlash.SpecialPaletteType:X4}";
        if (reloaded != string.Join(',', native[29..31]) || loaded.Health != health ||
            loaded.CrystalFlash.Phase != CrystalFlashPhase.Inactive ||
            loaded.Shinespark.Phase != ShinesparkPhase.Inactive)
            throw new InvalidDataException("Ordinary reload retained the glitch or failed to restore health.");
    }
}
