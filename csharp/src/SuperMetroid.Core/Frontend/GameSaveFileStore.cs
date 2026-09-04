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
    public static GameSaveLoadResult LoadOrMigrate(
        SuperMetroidAddressSpace addressSpace,
        string jsonPath,
        string legacySramPath)
    {
        ArgumentNullException.ThrowIfNull(addressSpace);
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(legacySramPath);
        if (File.Exists(jsonPath))
        {
            GameSaveJsonDocument document = GameSaveJsonCodec.Deserialize(
                File.ReadAllText(jsonPath),
                jsonPath);
            GameSaveJsonCodec.Apply(document, addressSpace);
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
        WriteAtomic(addressSpace, jsonPath);
        return new GameSaveLoadResult(jsonPath, MigratedLegacySram: true);
    }

    /// <summary>Writes a deterministic UTF-8 JSON replacement without exposing a partial file.</summary>
    public static void WriteAtomic(SuperMetroidAddressSpace addressSpace, string jsonPath)
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
            string json = GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Capture(addressSpace));
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

public readonly record struct GameSaveLoadResult(
    string JsonPath,
    bool MigratedLegacySram);
