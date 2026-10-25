namespace SuperMetroid.Core.Progression;

/// <summary>
/// The single conversion between a named <see cref="EventNumber"/> and its bit in the persistent event table that save
/// slots, the shared progression state and room-state selection read. Nothing else turns an event into a bit index.
/// </summary>
[SnesEncodingOwner]
public static class EventNumberCodec
{
    /// <summary>The bit of the event table that records <paramref name="eventNumber"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="eventNumber"/> names no retail event.</exception>
    public static int ToEventBit(this EventNumber eventNumber) =>
        Enum.IsDefined(eventNumber)
            ? (int)eventNumber
            : throw new ArgumentOutOfRangeException(nameof(eventNumber), eventNumber, "The event has no proven retail EventNumber identity.");

    /// <summary>The named event that event-table bit <paramref name="bit"/> records, when the retail game names one.</summary>
    public static bool TryFromEventBit(int bit, out EventNumber eventNumber)
    {
        eventNumber = (EventNumber)bit;
        return Enum.IsDefined(eventNumber);
    }
}
