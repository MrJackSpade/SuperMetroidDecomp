using SuperMetroid.Android;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class AndroidImportVerification
{
    public static void Run(string root, string rom, string audio, string validSeed)
    {
        VerifyMutableMemoryWithoutCartridge();
        string slot = Path.Combine(root, "debug-states", "SuperMetroid-debug-slot-9.smstate");
        Directory.CreateDirectory(Path.GetDirectoryName(slot)!);
        byte[] previous = [1, 2, 3];
        File.WriteAllBytes(slot, previous);
        string bad = Path.Combine(root, "bad-import.tmp");
        File.WriteAllText(bad, "invalid data");
        bool failed = false;
        try { AndroidFileImport.ImportState(root, rom, bad, 9); }
        catch (InvalidDataException) { failed = true; }
        if (!failed || !previous.AsSpan().SequenceEqual(File.ReadAllBytes(slot)))
            throw new InvalidDataException("Corrupt import replaced the existing slot.");
        AndroidFileImport.ImportState(root, rom, validSeed, 9);
        if (!File.ReadAllBytes(validSeed).AsSpan().SequenceEqual(File.ReadAllBytes(slot)))
            throw new InvalidDataException("Valid imported state bytes changed.");
        string backup = Directory.GetFiles(Path.Combine(root, "import-backups")).Single();
        if (!previous.AsSpan().SequenceEqual(File.ReadAllBytes(backup)))
            throw new InvalidDataException("State import did not preserve the exact previous slot.");
        string emptyResult = AndroidFileImport.ImportState(root, rom, validSeed, 8);
        if (!emptyResult.Contains("slot was empty", StringComparison.Ordinal) ||
            Directory.GetFiles(Path.Combine(root, "import-backups")).Length != 1)
            throw new InvalidDataException("Empty-slot import claimed or created a nonexistent previous-state backup.");

        var importedBus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        new SuperMetroidSaveRam(importedBus).SaveSlot(0, new SuperMetroidSaveSnapshot { Health = 17 });
        string json = Path.Combine(root, "import-source.json");
        File.WriteAllText(json, GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Capture(importedBus)));
        string save = Path.Combine(root, "SuperMetroid.save.json");
        byte[] previousSave = File.ReadAllBytes(save);
        // Installed save validation only needs mutable SRAM. Prove it neither opens nor
        // depends on the private cartridge path by using the ROM-free overload here.
        AndroidFileImport.StageRegularSave(root, json);
        if (!previousSave.AsSpan().SequenceEqual(File.ReadAllBytes(save)))
            throw new InvalidDataException("Staging regular save changed current save prematurely.");
        string pending = Path.Combine(root, "SuperMetroid.import.save.json");
        byte[] pendingBytes = File.ReadAllBytes(pending);
        failed = false;
        try { AndroidFileImport.StageRegularSave(root, bad); }
        catch (InvalidDataException) { failed = true; }
        if (!failed || !pendingBytes.AsSpan().SequenceEqual(File.ReadAllBytes(pending)))
            throw new InvalidDataException("Invalid JSON displaced the valid pending import.");
        using var restarted = new AndroidSessionData(root, rom, audio);
        if (new SuperMetroidSaveRam(restarted.Bus).ReadSlot(0)?.Health != 17 || File.Exists(pending))
            throw new InvalidDataException("Next session did not activate the pending regular save exactly once.");
        if (!Directory.GetFiles(Path.Combine(root, "import-backups"))
            .Any(path => previousSave.AsSpan().SequenceEqual(File.ReadAllBytes(path))))
            throw new InvalidDataException("Activating regular save omitted the previous-save recovery copy.");
        Console.WriteLine("PASS Android imports: corrupt rejection preserves slots/pending save, valid state bytes and backups, next-launch regular save activation.");
    }

    private static void VerifyMutableMemoryWithoutCartridge()
    {
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        memory.WriteByte(0x7e1234, 0x56);
        memory.WriteByte(0x700123, 0x78);
        if (memory.ReadByte(0x7e1234) != 0x56 || memory.ReadByte(0x700123) != 0x78)
            throw new InvalidDataException("Cartridge-free address space did not retain mutable WRAM/SRAM.");

        bool rejectedRomRead = false;
        try { _ = memory.ReadByte(0x808000); }
        catch (InvalidOperationException error) when (
            error.Message.Contains("unpopulated ROM", StringComparison.Ordinal))
        {
            rejectedRomRead = true;
        }
        if (!rejectedRomRead)
            throw new InvalidDataException("Cartridge-free address space hid a ROM read instead of failing loudly.");
    }
}
