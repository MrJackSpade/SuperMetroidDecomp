using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Desktop;

namespace SuperMetroid.Android;

/// <summary>Validated local file imports; replacement always preserves a unique recovery copy.</summary>
internal static class AndroidFileImport
{
    /// <summary>Imports a state into an installed game without opening its private ROM.</summary>
    /// <param name="root">Installed game directory used to resolve content identity and save-state paths.</param>
    /// <param name="source">User-selected save-state file.</param>
    /// <param name="slot">Numbered destination slot; the automatic slot is reserved.</param>
    /// <returns>User-facing import result, including backup and migration warnings.</returns>
    public static string ImportState(string root, string source, int slot)
    {
        if (slot == DebuggerStateFormat.AutomaticSlot)
            return "The auto slot is reserved for door transitions. Choose a numbered slot (0-9) to import a state.";
        var installation = new SuperMetroid.AssetExtraction.GameInstallation(root);
        var contentIdentity = SuperMetroid.AssetExtraction.GameContentIdentity.Create(
            installation.LoadAudio(),
            installation.LoadMaps(),
            installation.LoadProjectiles(),
            SuperMetroid.AssetExtraction.RoomPresentationIdentity.Create(
                installation.LoadRoomCharacters(), installation.LoadRoomPalettes(),
                installation.LoadRoomMetatiles(), installation.LoadRoomBackgroundTilemaps(),
                installation.LoadRoomSkyTilemaps(), installation.LoadRoomVisualLayouts())
                .Append(KeyValuePair.Create(SuperMetroid.AssetExtraction.GameInstallationLayout.SamusBodyDirectoryName,
                    installation.LoadSamusBodyArt().ContentIdentity))
                .Concat(SuperMetroid.AssetExtraction.RoomPlmPresentationIdentity.Load(installation))
                .Concat(SuperMetroid.AssetExtraction.GameplayPresentationIdentity.Load(installation))
                .Concat(SuperMetroid.AssetExtraction.EndingPresentationIdentity.Load(installation))
                .Append(KeyValuePair.Create(SuperMetroid.AssetExtraction.GameInstallationLayout.IntroCinematicDirectoryName,
                    installation.LoadIntroCinematicArt().ContentIdentity))
                .Append(KeyValuePair.Create(SuperMetroid.AssetExtraction.GameInstallationLayout.EnemyTileDirectoryName,
                    installation.LoadEnemyTiles().ContentIdentity)));
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

    /// <summary>Validates an imported state in a temporary store before replacing the selected slot.</summary>
    /// <param name="root">Installed game directory containing destination and backup paths.</param>
    /// <param name="source">Source state file.</param>
    /// <param name="slot">Destination numbered slot.</param>
    /// <param name="createStore">Factory for stores bound to the destination installation.</param>
    /// <returns>Import result with any state migration warnings.</returns>
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
    /// <param name="root">Installed game directory receiving the pending save.</param>
    /// <param name="source">Regular save JSON to validate and stage.</param>
    /// <returns>User-facing staging result.</returns>
    public static string StageRegularSave(string root, string source) =>
        StageRegularSaveCore(root, source, SuperMetroidAddressSpace.CreateWithoutCartridge(),
            new SuperMetroid.AssetExtraction.GameInstallation(root).LoadMaps());

    /// <summary>Validates the regular-save schema and SRAM application before staging it.</summary>
    /// <param name="root">Installed game directory used for pending and backup paths.</param>
    /// <param name="source">Regular save JSON to validate.</param>
    /// <param name="bus">Cartridge-free address space on which the save is validated.</param>
    /// <param name="maps">Installed area map catalog required by save application.</param>
    /// <returns>User-facing staging result.</returns>
    private static string StageRegularSaveCore(
        string root,
        string source,
        SuperMetroidAddressSpace bus, AreaMapPresentationCatalog maps)
    {
        // Validate the full schema and its SRAM application before publishing a pending
        // import. A separate file prevents ongoing gameplay persistence overwriting it.
        GameSaveJsonCodec.Apply(GameSaveJsonCodec.Deserialize(File.ReadAllText(source), source), bus, maps);
        ReplaceWithBackup(root, source, PendingPath(root));
        return "Regular save validated and staged for next app launch. Current gameplay is unchanged. Previous save will be backed up before activation.";
    }

    /// <summary>Applies a previously staged save at startup, backing up the current save before replacement.</summary>
    /// <param name="root">Installed game directory containing the pending and current save files.</param>
    /// <param name="bus">Runtime address space receiving the save's SRAM state.</param>
    /// <param name="savePath">Destination path for the active regular save.</param>
    /// <param name="maps">Installed area map catalog required by save application.</param>
    public static void ActivatePendingSave(string root, SuperMetroidAddressSpace bus, string savePath, AreaMapPresentationCatalog maps)
    {
        string pending = PendingPath(root);
        if (!File.Exists(pending)) return;
        GameSaveJsonCodec.Apply(GameSaveJsonCodec.Deserialize(File.ReadAllText(pending), pending), bus, maps);
        if (File.Exists(savePath)) Backup(root, savePath);
        GameSaveFileStore.WriteAtomic(bus, savePath, maps);
        File.Delete(pending);
    }

    /// <summary>Gets the fixed pending-save path inside an installation directory.</summary>
    /// <param name="root">Installed game directory.</param>
    /// <returns>Path of the staged save awaiting startup activation.</returns>
    private static string PendingPath(string root) => Path.Combine(root, "SuperMetroid.import.save.json");

    /// <summary>Copies a replacement through a temporary sibling and preserves an existing destination.</summary>
    /// <param name="root">Installation root containing the recovery-backup directory.</param>
    /// <param name="source">Validated source file to install.</param>
    /// <param name="destination">Target path to replace atomically after copying.</param>
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

    /// <summary>Copies an existing save or state into the installation's uniquely named backup directory.</summary>
    /// <param name="root">Installed game directory.</param>
    /// <param name="source">Existing file whose contents must be retained.</param>
    private static void Backup(string root, string source)
    {
        string directory = Path.Combine(root, "import-backups");
        Directory.CreateDirectory(directory);
        File.Copy(source, Path.Combine(directory, Path.GetFileName(source) + "." + Guid.NewGuid().ToString("N") + ".bak"));
    }
}
