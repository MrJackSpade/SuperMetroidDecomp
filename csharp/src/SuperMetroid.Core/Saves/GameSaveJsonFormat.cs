namespace SuperMetroid.Core.Saves;

/// <summary>Stable identifiers and geometry for the human-readable game-save envelope.</summary>
public static class GameSaveJsonFormat
{
    /// <summary>
    /// Schema six lists only the story events the game keeps, by name (#1553); schema five kept the Zebetite count as a
    /// count and named the Shitroid-approach event (#1527); schema four named each slot's startup route (#1523); schema
    /// three listed each area's defeated bosses, used stations and acquired map by area (#1475). The hosts' legacy
    /// importer upgrades schemas one through five.
    /// </summary>
    public const int SchemaVersion = 6;

    /// <summary>Bytes per legacy schema-one import page; never emitted by schemas two and three.</summary>
    public const int PreservationPageByteCount = 256;

    /// <summary>Extension replacing the emulator-oriented <c>.srm</c> host file.</summary>
    public const string FileExtension = ".save.json";
}
