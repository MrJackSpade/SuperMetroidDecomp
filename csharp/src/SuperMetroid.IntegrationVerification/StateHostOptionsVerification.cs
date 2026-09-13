using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Desktop;

internal static class StateHostOptionsVerification
{
    public static int Run(string statePath, string iniPath)
    {
        string rom = Path.GetFullPath("Super Metroid.smc");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var options = SuperMetroidGameOptionsIni.Parse(File.ReadAllText(iniPath), iniPath);
        if (!options.Invincibility || !options.InfiniteAmmo)
            throw new InvalidDataException("This reported-state audit requires both protection options enabled.");
        string directory = Path.GetDirectoryName(Path.GetFullPath(statePath))!;
        var exact = new DebuggerSaveStateStore(rom, bus.Rom, directory).Load(0);
        var store = new DebuggerSaveStateStore(rom, bus.Rom, Path.GetDirectoryName(Path.GetFullPath(statePath)), options);
        if (!string.Equals(store.GetSlotPath(0), Path.GetFullPath(statePath), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Supply the actual slot-0 state path.", nameof(statePath));
        var restored = store.Load(0);
        var runtime = restored.Game.RuntimeForVerification!;
        Console.WriteLine($"INI: invincibility={options.Invincibility}, ammo={options.InfiniteAmmo}; restored runtime: invincibility={runtime.PlayerInvincibilityEnabled}, ammo={runtime.InfiniteAmmoEnabled}; room={restored.Metadata.RoomPointer:X4}");
        if (runtime.PlayerInvincibilityEnabled != options.Invincibility || runtime.InfiniteAmmoEnabled != options.InfiniteAmmo)
            throw new InvalidDataException("Restored runtime discarded active host protection settings.");
        if (restored.Game.ConfiguredOptions != options)
            throw new InvalidDataException("Frontend and live runtime host options disagree.");
        var samus = runtime.Samus!;
        var capturedSamus = exact.Game.RuntimeForVerification!.Samus!;
        if (samus.Health != capturedSamus.Health || samus.Missiles != capturedSamus.Missiles ||
            samus.SuperMissiles != capturedSamus.SuperMissiles || samus.PowerBombs != capturedSamus.PowerBombs)
            throw new InvalidDataException("Rebinding host policy modified captured resources before a frame ran.");
        samus.Health = 0;
        samus.MaxMissiles = samus.MaxSuperMissiles = samus.MaxPowerBombs = 5;
        samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 0;
        runtime.StepFrame(0);
        if (samus.Health != 1 || samus.Missiles != 1 || samus.SuperMissiles != 1 || samus.PowerBombs != 1)
            throw new InvalidDataException($"Protection failed: energy={samus.Health}, ammo={samus.Missiles}/{samus.SuperMissiles}/{samus.PowerBombs}.");
        Console.WriteLine("PASS restored protection: energy and all unlocked ammo counters reach one, not zero.");
        runtime.StepFrame(0);
        ushort Word(int address) => (ushort)(restored.AddressSpace.ReadByte(address) |
            restored.AddressSpace.ReadByte(address + 1) << 8);
        foreach (var (offset, table) in new[] { (0x8e, 0x809dbf), (0x98, 0x809dd3), (0x9e, 0x809dd3), (0xa4, 0x809dd3) })
            if (runtime.Hud.Tiles[offset / 2] != Word(table + 2))
                throw new InvalidDataException($"HUD digit at {offset:X} did not display one.");
        samus.MaxMissiles = samus.MaxSuperMissiles = samus.MaxPowerBombs = 0;
        samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 0;
        runtime.StepFrame(0);
        if (samus.Missiles != 0 || samus.SuperMissiles != 0 || samus.PowerBombs != 0)
            throw new InvalidDataException("Host override unlocked unavailable ammunition.");
        restored.Game.ApplyHostOptions(options with { Invincibility = false, InfiniteAmmo = false });
        samus.Health = 0;
        samus.MaxMissiles = samus.MaxSuperMissiles = samus.MaxPowerBombs = 5;
        runtime.StepFrame(0);
        if (samus.Health != 0 || samus.Missiles != 0 || samus.SuperMissiles != 0 || samus.PowerBombs != 0)
            throw new InvalidDataException("Disabling host guards retained old protection.");
        Console.WriteLine("PASS HUD one digits, locked ammo remains zero, disabled guards retain zero; exact loader remains independent.");
        return 0;
    }
}
