namespace SuperMetroid.Core.Progression;

/// <summary>
/// The single conversion between a named <see cref="EventNumber"/> and the number the game gives that story event, which
/// room data and saves use. Nothing else turns an event into its number.
/// </summary>
[EncodingOwner("story event numbering")]
public static class EventNumberCodec
{
    /// <summary>The number the game gives <paramref name="eventNumber"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="eventNumber"/> names no retail event.</exception>
    public static int ToEventNumber(this EventNumber eventNumber) =>
        Enum.IsDefined(eventNumber)
            ? (int)eventNumber
            : throw new ArgumentOutOfRangeException(nameof(eventNumber), eventNumber, "The event has no proven retail EventNumber identity.");

    /// <summary>The named event the game numbers <paramref name="number"/>, when the retail game names one.</summary>
    public static bool TryFromEventNumber(int number, out EventNumber eventNumber)
    {
        bool named = Enum.IsDefined((EventNumber)number);
        eventNumber = named ? (EventNumber)number : default;
        return named;
    }
}
