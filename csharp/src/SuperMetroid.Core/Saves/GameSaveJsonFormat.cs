namespace SuperMetroid.Core.Saves;

/// <summary>Stable identifiers of the human-readable game-save envelope.</summary>
public static class GameSaveJsonFormat
{
    /// <summary>
    /// Schema fifteen names every door for its room, whichever of the room's states places it (#2501); schema fourteen
    /// names every item location for its room, whichever of the room's states places it (#2491); schema
    /// thirteen keeps no reserve-missile count, which nothing in the game reads (#2364); schema twelve keeps a power-on that found no valid save beside completion (#2140); schema eleven names item locations and doors by their rooms' spelled-out names (#2103); schema ten lists no item for the equipment flag no item records (#1778); schema nine names a slot's checkpoint and used stations by their load stations' names (#1723); schema eight keeps
    /// no count of item set-ups, which nothing in the game reads (#1691); schema seven named each slot's HUD selection and reserve-tank mode and keeps no setting the game never reads
    /// (#1605); schema six lists the story events, item locations and doors by name and keeps no Chozo orbs (#1553);
    /// schema five kept the Zebetite count as a count and named the Baby Metroid-approach event (#1527); schema four named
    /// each slot's startup route (#1523); schema three listed each area's defeated bosses, used stations and acquired
    /// map by area (#1475). The hosts' legacy importer upgrades schemas one through fourteen.
    /// </summary>
    public const int SchemaVersion = 15;

    /// <summary>The file extension of a game save.</summary>
    public const string FileExtension = ".save.json";
}
