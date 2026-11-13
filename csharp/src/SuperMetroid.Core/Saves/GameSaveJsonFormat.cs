namespace SuperMetroid.Core.Saves;

/// <summary>Stable identifiers of the human-readable game-save envelope.</summary>
public static class GameSaveJsonFormat
{
    /// <summary>
    /// Schema nine names a slot's checkpoint and used stations by their load stations' names (#1723); schema eight keeps
    /// no count of item set-ups, which nothing in the game reads (#1691); schema seven named each slot's HUD selection and reserve-tank mode and keeps no setting the game never reads
    /// (#1605); schema six lists the story events, item locations and doors by name and keeps no Chozo orbs (#1553);
    /// schema five kept the Zebetite count as a count and named the Shitroid-approach event (#1527); schema four named
    /// each slot's startup route (#1523); schema three listed each area's defeated bosses, used stations and acquired
    /// map by area (#1475). The hosts' legacy importer upgrades schemas one through eight.
    /// </summary>
    public const int SchemaVersion = 9;

    /// <summary>Extension replacing the emulator-oriented <c>.srm</c> host file.</summary>
    public const string FileExtension = ".save.json";
}
