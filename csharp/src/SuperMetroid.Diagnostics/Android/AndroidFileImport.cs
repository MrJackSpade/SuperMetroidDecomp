using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Desktop;

namespace SuperMetroid.Android;

/// <summary>Validated local file imports; replacement always preserves a unique recovery copy.</summary>
internal static class AndroidFileImport
{
    /// <summary>Imports a state into an installed game without opening its private ROM.</summary>
    public static string ImportState(string root, string source, int slot)
    {
        var installation = new SuperMetroid.AssetExtraction.GameInstallation(root);
        var contentIdentity = SuperMetroid.AssetExtraction.GameContentIdentity.Create(
            installation.LoadAudio(),
            installation.LoadMaps(),
            installation.LoadProjectiles());
        return ImportStateCore(
            root,
            source,
            slot,
            directory => DebuggerSaveStateStore.ForInstalledGame(
                root,
                hostOptions: null,
                contentIdentity,
                directory));
    }

    /// <summary>Imports a state for an explicit cartridge-backed diagnostic session.</summary>
    public static string ImportState(string root, string romPath, string source, int slot)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        return ImportStateCore(
            root,
            source,
            slot,
            directory => new DebuggerSaveStateStore(
                romPath,
                bus.Rom,
                directory));
    }

    private static string ImportStateCore(
        string root,
        string source,
        int slot,
        Func<string, DebuggerSaveStateStore> createStore)
    {
        var destinationStore = createStore(Path.Combine(root, "debug-states"));
        string destination = destinationStore.GetSlotPath(slot);
        string staging = Directory.CreateTempSubdirectory("SuperMetroid-import-").FullName;
        try
        {
            var stagingStore = createStore(staging);
            File.Copy(source, stagingStore.GetSlotPath(slot));
            var decoded = stagingStore.Load(slot);
            if (decoded.AudioPlayer is null) throw new InvalidDataException("State has no managed audio graph.");
            bool replacingSlot = File.Exists(destination);
            ReplaceWithBackup(root, stagingStore.GetSlotPath(slot), destination);
            return $"Imported state into slot {slot}. Use Load state to resume it." +
                (replacingSlot ? " Previous slot retained in import-backups." : " The slot was empty; no existing state was replaced.") +
                (decoded.Warnings.Count == 0 ? "" : "\nWARNING: " + string.Join("\n", decoded.Warnings));
        }
        finally { Directory.Delete(staging, recursive: true); }
    }

    /// <summary>Stages and fully validates an installed-game save without a cartridge payload.</summary>
    public static string StageRegularSave(string root, string source) =>
        StageRegularSaveCore(root, source, SuperMetroidAddressSpace.CreateWithoutCartridge());

    /// <summary>Stages a save for an explicit cartridge-backed diagnostic session.</summary>
    public static string StageRegularSave(string root, string romPath, string source)
    {
        return StageRegularSaveCore(root, source, SuperMetroidAddressSpace.LoadRetailRom(romPath));
    }

    private static string StageRegularSaveCore(
        string root,
        string source,
        SuperMetroidAddressSpace bus)
    {
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
