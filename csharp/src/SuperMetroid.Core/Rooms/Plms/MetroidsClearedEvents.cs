using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The event the metroids-cleared PLM marks once a room's metroids are all killed, selected by its table entry.
/// </summary>
/// <remarks>
/// The entry selects one of thirteen, not an event number. The first nine entries are deliberate no-ops; the final four
/// mark the four metroid rooms' cleared events. An out-of-range entry is rejected so it cannot acquire a plausible
/// meaning.
/// </remarks>
internal static class MetroidsClearedEvents
{
    /// <summary>The table's entries.</summary>
    private const int EntryCount = 13;

    /// <summary>The leading entries that mark nothing.</summary>
    private const int NoOpEntryCount = 9;

    /// <summary>
    /// Resolves the event written after the enemy death quota is met, or null for one of the nine intentional no-op
    /// entries.
    /// </summary>
    public static EventNumber? ResolveEvent(int entry)
    {
        ValidateEntry(entry);
        return entry < NoOpEntryCount ? null : QuotaObserverEvents[entry - NoOpEntryCount];
    }

    /// <summary>The events the four quota observers mark, in entry order.</summary>
    private static readonly EventNumber[] QuotaObserverEvents =
    [
        EventNumber.FirstMetroidHallCleared,
        EventNumber.FirstMetroidShaftCleared,
        EventNumber.SecondMetroidHallCleared,
        EventNumber.SecondMetroidShaftCleared,
    ];

    /// <summary>Rejects an entry outside the thirteen.</summary>
    private static void ValidateEntry(int entry)
    {
        if ((uint)entry >= EntryCount)
            throw new InvalidDataException($"Metroids-cleared entry {entry} is not one of its {EntryCount} entries.");
    }
}
