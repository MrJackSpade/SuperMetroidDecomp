using System.Text;
using SuperMetroid.Core;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Diagnostics.Saves;

/// <summary>Atomic JSON persistence and one-time emulator-SRAM migration for normal saves.</summary>
public static class GameSaveFileStore
{
    /// <summary>
    /// Loads JSON when present. Otherwise imports a legacy 8-KiB <c>.srm</c>, writes its JSON
    /// successor atomically, and deliberately leaves the original file untouched as backup.
    /// </summary>
    /// <param name="jsonPath">Preferred JSON path, returned as supplied; existing JSON is authoritative and is never bypassed because parsing fails.</param>
    /// <param name="legacySramPath">Fallback emulator-SRAM path, considered only when JSON is absent; a present file must contain exactly 8192 bytes.</param>
    /// <param name="maps">Installed map definitions used by the save codec to translate persistent exploration data.</param>
    /// <returns>The requested JSON path, whether a legacy SRAM file was imported, and the loaded saves; if neither file exists, the saves are empty and no file is created.</returns>
    /// <remarks>Older supported JSON is rewritten in the current schema. Filesystem failures propagate.</remarks>
    /// <exception cref="InvalidDataException">JSON is invalid or unsupported, legacy SRAM has the wrong length, or persistent save data cannot be translated.</exception>
    public static GameSaveLoadResult LoadOrMigrate(
        string jsonPath,
        string legacySramPath, IAreaMapSource maps)
    {
        Ensure.NotNull(maps);
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(legacySramPath);
        if (File.Exists(jsonPath))
        {
            GameSaveJsonDocument document = LegacyGameSaveJson.Deserialize(
                File.ReadAllText(jsonPath),
                jsonPath,
                out bool migrated);
            SaveSlotStore loaded = GameSaveJsonCodec.ToStore(document, maps);
            if (migrated)
                WriteAtomic(loaded, jsonPath);
            return new GameSaveLoadResult(jsonPath, MigratedLegacySram: false, loaded);
        }
        if (!File.Exists(legacySramPath))
            return new GameSaveLoadResult(jsonPath, MigratedLegacySram: false, new SaveSlotStore(maps));

        byte[] legacy = File.ReadAllBytes(legacySramPath);
        if (legacy.Length != SaveRamLayout.ImageByteCount)
        {
            throw new InvalidDataException(
                $"Legacy save RAM '{legacySramPath}' contains {legacy.Length} bytes; " +
                $"Super Metroid requires exactly {SaveRamLayout.ImageByteCount} bytes.");
        }
        SaveSlotStore imported = SaveRamCodec.Decode(legacy, maps);
        WriteAtomic(imported, jsonPath);
        return new GameSaveLoadResult(jsonPath, MigratedLegacySram: true, imported);
    }

    /// <summary>Writes a deterministic UTF-8 JSON replacement without exposing a partial file.</summary>
    /// <param name="saves">The saves to capture as named persistent state; this method does not create a gameplay checkpoint or change them.</param>
    /// <param name="jsonPath">Destination path whose parent directories are created; an existing destination is replaced with its previous contents saved at the same path plus .bak.</param>
    /// <remarks>Writes BOM-free UTF-8 to a unique adjacent .tmp file, flushes it to disk, then replaces or moves it into place. Remaining temporary files are deleted in a finally block; filesystem errors propagate.</remarks>
    public static void WriteAtomic(SaveSlotStore saves, string jsonPath)
    {
        Ensure.NotNull(saves);
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonPath);
        string fullPath = Path.GetFullPath(jsonPath);
        string? directory = Path.GetDirectoryName(fullPath);
        if (directory is null)
            throw new InvalidDataException($"Game save path '{jsonPath}' has no parent directory.");
        Directory.CreateDirectory(directory);
        string temporaryPath = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        string backupPath = fullPath + ".bak";
        try
        {
            string json = GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Capture(saves));
            using (var stream = new FileStream(
                       temporaryPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       bufferSize: 4096,
                       FileOptions.WriteThrough))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                writer.Write(json);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }

            if (File.Exists(fullPath))
                File.Replace(temporaryPath, fullPath, backupPath, ignoreMetadataErrors: true);
            else
                File.Move(temporaryPath, fullPath);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }
}
