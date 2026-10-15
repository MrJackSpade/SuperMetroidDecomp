namespace SuperMetroid.Core.Saves;

/// <summary>Stable identifiers and geometry for the human-readable game-save envelope.</summary>
public static class GameSaveJsonFormat
{
    /// <summary>Named-only schema; version one is supported by the legacy importer.</summary>
    public const int SchemaVersion = 2;

    /// <summary>Bytes per legacy schema-one import page; never emitted by schema two.</summary>
    public const int PreservationPageByteCount = 256;

    /// <summary>Extension replacing the emulator-oriented <c>.srm</c> host file.</summary>
    public const string FileExtension = ".save.json";
}
