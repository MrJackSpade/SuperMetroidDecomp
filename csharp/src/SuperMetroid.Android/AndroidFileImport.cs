using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Desktop;

namespace SuperMetroid.Android;

/// <summary>Validated local file imports; replacement always preserves a unique recovery copy.</summary>
internal static class AndroidFileImport
{
    public static string ImportState(string root, string romPath, string source, int slot)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var destinationStore = new DebuggerSaveStateStore(romPath, bus.Rom, Path.Combine(root, "debug-states"));
        string destination = destinationStore.GetSlotPath(slot);
        string staging = Directory.CreateTempSubdirectory("SuperMetroid-import-").FullName;
        try
        {
            var stagingStore = new DebuggerSaveStateStore(romPath, bus.Rom, staging);
            File.Copy(source, stagingStore.GetSlotPath(slot));
            var decoded = stagingStore.Load(slot);
            if (decoded.AudioPlayer is null) throw new InvalidDataException("State has no managed audio graph.");
            ReplaceWithBackup(root, stagingStore.GetSlotPath(slot), destination);
            return $"Imported state into slot {slot}. Use Load state to resume it. Previous slot retained in import-backups." +
                (decoded.Warnings.Count == 0 ? "" : "\nWARNING: " + string.Join("\n", decoded.Warnings));
        }
        finally { Directory.Delete(staging, recursive: true); }
    }

    public static string StageRegularSave(string root, string romPath, string source)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        // Validate the full schema and its SRAM application before publishing a pending
        // import. A separate file prevents ongoing gameplay persistence overwriting it.
        GameSaveJsonCodec.Apply(GameSaveJsonCodec.Deserialize(File.ReadAllText(source), source), bus);
        ReplaceWithBackup(root, source, PendingPath(root));
        return "Regular save validated and staged for next app launch. Current gameplay is unchanged. Previous save will be backed up before activation.";
    }

    public static void ActivatePendingSave(string root, SuperMetroidAddressSpace bus, string savePath)
    {
        string pending = PendingPath(root);
        if (!File.Exists(pending)) return;
        GameSaveJsonCodec.Apply(GameSaveJsonCodec.Deserialize(File.ReadAllText(pending), pending), bus);
        if (File.Exists(savePath)) Backup(root, savePath);
        GameSaveFileStore.WriteAtomic(bus, savePath);
        File.Delete(pending);
    }

    private static string PendingPath(string root) => Path.Combine(root, "SuperMetroid.import.save.json");

    private static void ReplaceWithBackup(string root, string source, string destination)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        string temporary = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.Copy(source, temporary);
            if (File.Exists(destination)) Backup(root, destination);
            File.Move(temporary, destination, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static void Backup(string root, string source)
    {
        string directory = Path.Combine(root, "import-backups");
        Directory.CreateDirectory(directory);
        File.Copy(source, Path.Combine(directory, Path.GetFileName(source) + "." + Guid.NewGuid().ToString("N") + ".bak"));
    }
}
