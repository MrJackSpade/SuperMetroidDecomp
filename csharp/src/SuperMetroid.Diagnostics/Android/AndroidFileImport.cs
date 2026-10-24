using SuperMetroid.Diagnostics.Saves;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Diagnostics.SaveStates;

namespace SuperMetroid.Diagnostics.Android;

/// <summary>Validated local file imports; replacement always preserves a unique recovery copy.</summary>
internal static class AndroidFileImport
{
    /// <summary>Imports a state into an installed game without opening its private ROM.</summary>
    public static string ImportState(string root, string source, int slot)
    {
        if (slot == DebuggerStateFormat.AutomaticSlot)
            return "The auto slot is reserved for door transitions. Choose a numbered slot (0-9) to import a state.";
        var installation = new SuperMetroid.AssetExtraction.Installation.GameInstallation(root);
        var maps = installation.LoadMaps();
        var projectiles = installation.LoadProjectiles();
        var contentIdentity = SuperMetroid.AssetExtraction.Installation.GameContentIdentity.Create(
            installation.LoadAudioClips(),
            maps,
            projectiles,
            SuperMetroid.AssetExtraction.Installation.InstalledArtworkIdentity.Create(installation.LoadGameArtwork(maps, projectiles)));
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

    /// <summary>Validates a debugger state in a temporary store before replacing its destination slot.</summary>
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
        StageRegularSaveCore(root, source, new SuperMetroid.AssetExtraction.Installation.GameInstallation(root).LoadMaps());

    /// <summary>Builds the saves an imported file describes before publishing a pending file.</summary>
    private static string StageRegularSaveCore(
        string root,
        string source,
        AreaMapPresentationCatalog maps)
    {
        // Validate the full schema and its translation to saves before publishing a pending
        // import. A separate file prevents ongoing gameplay persistence overwriting it.
        _ = GameSaveJsonCodec.ToStore(LegacyGameSaveJson.Deserialize(File.ReadAllText(source), source, out _), maps);
        ReplaceWithBackup(root, source, PendingPath(root));
        return "Regular save validated and staged for next app launch. Current gameplay is unchanged. Previous save will be backed up before activation.";
    }

    /// <summary>
    /// Validates and atomically activates a staged save, retaining a backup of the previous save.
    /// </summary>
    /// <returns>The activated saves, or null when no save is staged.</returns>
    public static SaveSlotStore? ActivatePendingSave(string root, string savePath, AreaMapPresentationCatalog maps)
    {
        string pending = PendingPath(root);
        if (!File.Exists(pending)) return null;
        SaveSlotStore saves = GameSaveJsonCodec.ToStore(LegacyGameSaveJson.Deserialize(File.ReadAllText(pending), pending, out _), maps);
        if (File.Exists(savePath)) Backup(root, savePath);
        GameSaveFileStore.WriteAtomic(saves, savePath);
        File.Delete(pending);
        return saves;
    }

    /// <summary>Locates the save staged for activation on the next Android launch.</summary>
    private static string PendingPath(string root) => Path.Combine(root, "SuperMetroid.import.save.json");

    /// <summary>Copies through a unique temporary path and backs up an existing destination before replacement.</summary>
    private static void ReplaceWithBackup(string root, string source, string destination)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination) ??
            throw new ArgumentException($"{destination} has no containing directory.", nameof(destination)));
        string temporary = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.Copy(source, temporary);
            if (File.Exists(destination)) Backup(root, destination);
            File.Move(temporary, destination, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    /// <summary>Copies a replaced user file to a uniquely named recovery path.</summary>
    private static void Backup(string root, string source)
    {
        string directory = Path.Combine(root, "import-backups");
        Directory.CreateDirectory(directory);
        File.Copy(source, Path.Combine(directory, Path.GetFileName(source) + "." + Guid.NewGuid().ToString("N") + ".bak"));
    }
}
