namespace SuperMetroid.Core.Saves;

/// <summary>Stable identifiers of the human-readable game-save envelope.</summary>
public static class GameSaveJsonFormat
{
    /// <summary>
    /// Schema seventeen keys the slots and the selected slot by the slot's name, not by its place in the file select's list
    /// (#2565); schema sixteen names the save file's history as one value (#2560); schema fifteen names every door for its
    /// room, whichever of the room's states places it (#2501); schema fourteen
    /// names every item location for its room, whichever of the room's states places it (#2491); schema
    /// thirteen keeps no reserve-missile count, which nothing in the game reads (#2364); schema twelve keeps a power-on that found no valid save beside completion (#2140); schema eleven names item locations and doors by their rooms' spelled-out names (#2103); schema ten lists no item for the equipment flag no item records (#1778); schema nine names a slot's checkpoint and used stations by their load stations' names (#1723); schema eight keeps
    /// no count of item set-ups, which nothing in the game reads (#1691); schema seven named each slot's HUD selection and reserve-tank mode and keeps no setting the game never reads
    /// (#1605); schema six lists the story events, item locations and doors by name and keeps no Chozo orbs (#1553);
    /// schema five kept the Zebetite count as a count and named the Baby Metroid-approach event (#1527); schema four named
    /// each slot's startup route (#1523); schema three listed each area's defeated bosses, used stations and acquired
    /// map by area (#1475). The hosts' legacy importer upgrades schemas one through sixteen.
    /// </summary>
    public const int SchemaVersion = 17;

    /// <summary>
    /// The name a game save gives <paramref name="slot"/>, which keys the slot and names the selected one (schema seventeen,
    /// #2565): its installed key.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="slot"/> is no save slot.</exception>
    public static string SlotKey(SaveSlot slot) => SaveSlotKeys.Installed.Of(slot);

    /// <summary>The save slot a game save names <paramref name="key"/>.</summary>
    /// <exception cref="InvalidDataException">No slot has that name.</exception>
    public static SaveSlot SlotNamed(string? key) => SaveSlotKeys.Installed.TryParse(key, out SaveSlot slot)
        ? slot
        : throw new InvalidDataException($"'{key}' names no save slot; a slot is named A, B or C");

    /// <summary>The file extension of a game save.</summary>
    public const string FileExtension = ".save.json";
}
