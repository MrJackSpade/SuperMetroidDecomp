using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Exact room-argument dispatcher table used by PLM header <c>$84:DB44</c>.
/// </summary>
/// <remarks>
/// The room argument is a byte offset into thirteen consecutive words at
/// <c>$84:DB28-$DB40</c>, not an event number. The first nine entries deliberately point to
/// distinct one-byte RTS addresses; the final four select the kill-quota observers that mark
/// events $10 through $13. Calculated native identities and strict bounds prevent an odd or out-of-range room
/// argument from accidentally acquiring a plausible meaning.
/// </remarks>
internal static class MetroidsClearedPlmRomData
{

    /// <summary>
    /// Resolves the event written after the enemy death quota is met, or null for one of the
    /// cartridge's nine intentional no-op argument entries.
    /// </summary>
    public static EventNumber? ResolveEvent(ushort roomArgument)
    {
        int wordIndex = ValidateAndGetWordIndex(roomArgument);
        return wordIndex < 9 ? null : QuotaObserverEvents[wordIndex - 9];
    }

    /// <summary>Events marked by the four quota observers at $84:DADE, $DAEE, $DAFE and $DB0E, in table order.</summary>
    private static readonly EventNumber[] QuotaObserverEvents =
    [
        EventNumber.FirstMetroidHallCleared,
        EventNumber.FirstMetroidShaftCleared,
        EventNumber.SecondMetroidHallCleared,
        EventNumber.SecondMetroidShaftCleared,
    ];

    /// <summary>Converts a room argument into one of thirteen native table entries, rejecting odd or out-of-range byte offsets.</summary>
    private static int ValidateAndGetWordIndex(ushort roomArgument)
    {
        if ((roomArgument & 1) != 0 || roomArgument / 2 >= 13)
        {
            throw new InvalidDataException(
                $"Metroids-cleared PLM room argument ${roomArgument:X4} is not an even " +
                "offset into cartridge table $84:DB28-$DB40.");
        }

        return roomArgument / 2;
    }
}
