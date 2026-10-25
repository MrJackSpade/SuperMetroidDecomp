using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The event the metroids-cleared PLM marks once a room's metroids are all killed, selected by its room argument.
/// </summary>
/// <remarks>
/// The room argument is an even selector over thirteen entries, not an event number. The first nine entries are
/// deliberate no-ops; the final four mark the four metroid rooms' cleared events. An odd or out-of-range argument is
/// rejected so it cannot acquire a plausible meaning.
/// </remarks>
internal static class MetroidsClearedEvents
{
    /// <summary>The room argument's selectable entries.</summary>
    private const int EntryCount = 13;

    /// <summary>The leading entries that mark nothing.</summary>
    private const int NoOpEntryCount = 9;

    /// <summary>
    /// Resolves the event written after the enemy death quota is met, or null for one of the nine intentional no-op
    /// entries.
    /// </summary>
    public static EventNumber? ResolveEvent(int roomArgument)
    {
        int entry = ValidateAndGetEntry(roomArgument);
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

    /// <summary>Converts a room argument into one of the thirteen entries, rejecting odd or out-of-range selectors.</summary>
    private static int ValidateAndGetEntry(int roomArgument)
    {
        if ((roomArgument & 1) != 0 || roomArgument / 2 >= EntryCount)
        {
            throw new InvalidDataException(
                $"Metroids-cleared PLM room argument {roomArgument} is not an even selector of one of its {EntryCount} entries.");
        }

        return roomArgument / 2;
    }
}
