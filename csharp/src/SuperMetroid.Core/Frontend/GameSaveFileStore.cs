using SuperMetroid.Core.Assets;
using System.Text;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Atomic JSON persistence and one-time emulator-SRAM migration for normal saves.</summary>
public static class GameSaveFileStore
{
    /// <summary>
    /// Loads JSON when present. Otherwise imports a legacy 8-KiB <c>.srm</c>, writes its JSON
    /// successor atomically, and deliberately leaves the original file untouched as backup.
    /// </summary>
    /// <param name="addressSpace">Owner of live SRAM replaced by validated JSON or the exact legacy image; no gameplay state is loaded here.</param>
    /// <param name="jsonPath">Preferred JSON path, returned as supplied; existing JSON is authoritative and is never bypassed because parsing fails.</param>
    /// <param name="legacySramPath">Fallback emulator-SRAM path, considered only when JSON is absent; a present file must contain exactly 8192 bytes.</param>
    /// <param name="maps">Installed map definitions used by the save codec to translate persistent exploration data.</param>
    /// <returns>The requested JSON path and whether a legacy SRAM file was imported; if neither file exists, SRAM is unchanged and no file is created.</returns>
    /// <remarks>Older supported JSON is rewritten in the current schema. Filesystem failures propagate; a failed rewrite does not roll back SRAM already applied or imported.</remarks>
    /// <exception cref="InvalidDataException">JSON is invalid or unsupported, legacy SRAM has the wrong length, or persistent save data cannot be translated.</exception>
    public static GameSaveLoadResult LoadOrMigrate(
        SuperMetroidAddressSpace addressSpace,
        string jsonPath,
        string legacySramPath, AreaMapPresentationCatalog maps)
    {
        ArgumentNullException.ThrowIfNull(addressSpace);
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(legacySramPath);
        if (File.Exists(jsonPath))
        {
            GameSaveJsonDocument document = GameSaveJsonCodec.Deserialize(
                File.ReadAllText(jsonPath),
                jsonPath);
            GameSaveJsonCodec.Apply(document, addressSpace, maps);
            if (document.SourceSchemaVersion < GameSaveJsonFormat.SchemaVersion)
                WriteAtomic(addressSpace, jsonPath, maps);
            return new GameSaveLoadResult(jsonPath, MigratedLegacySram: false);
        }
        if (!File.Exists(legacySramPath))
            return new GameSaveLoadResult(jsonPath, MigratedLegacySram: false);

        byte[] legacy = File.ReadAllBytes(legacySramPath);
        if (legacy.Length != SuperMetroidAddressSpace.SaveRamByteCount)
        {
            throw new InvalidDataException(
                $"Legacy save RAM '{legacySramPath}' contains {legacy.Length} bytes; " +
                $"Super Metroid requires exactly {SuperMetroidAddressSpace.SaveRamByteCount} bytes.");
        }
        legacy.CopyTo(addressSpace.SaveRam);
        WriteAtomic(addressSpace, jsonPath, maps);
        return new GameSaveLoadResult(jsonPath, MigratedLegacySram: true);
    }

    /// <summary>Writes a deterministic UTF-8 JSON replacement without exposing a partial file.</summary>
    /// <param name="addressSpace">Live SRAM to capture as named persistent state; this method does not create a gameplay checkpoint or mutate SRAM.</param>
    /// <param name="jsonPath">Destination path whose parent directories are created; an existing destination is replaced with its previous contents saved at the same path plus .bak.</param>
    /// <param name="maps">Installed map definitions used to decode SRAM exploration data for JSON.</param>
    /// <remarks>Writes BOM-free UTF-8 to a unique adjacent .tmp file, flushes it to disk, then replaces or moves it into place. Remaining temporary files are deleted in a finally block; filesystem errors propagate.</remarks>
    public static void WriteAtomic(SuperMetroidAddressSpace addressSpace, string jsonPath, AreaMapPresentationCatalog maps)
    {
        ArgumentNullException.ThrowIfNull(addressSpace);
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
            string json = GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Capture(addressSpace, maps));
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

/// <summary>Outcome of choosing JSON persistence or importing legacy SRAM, without claiming that a JSON file exists or reporting a gameplay-load result.</summary>
/// <param name="JsonPath">The requested JSON path, preserved exactly as supplied rather than normalized to an absolute path.</param>
/// <param name="MigratedLegacySram">True only when an absent JSON file led to importing legacy SRAM and successfully writing its successor; false also covers no files present and older-JSON upgrades.</param>
public readonly record struct GameSaveLoadResult(
    string JsonPath,
    bool MigratedLegacySram);
