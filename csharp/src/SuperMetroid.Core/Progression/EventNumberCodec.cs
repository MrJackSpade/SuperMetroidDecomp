namespace SuperMetroid.Core.Progression;

/// <summary>
/// The single conversion between a named <see cref="EventNumber"/> and the number the game gives that story event, which
/// room data and saves use. Nothing else turns an event into its number.
/// </summary>
[EncodingOwner("story event numbering")]
public static class EventNumberCodec
{
    /// <summary>
    /// The named events in the order the game numbers them, from zero; null where a number belongs to no story event.
    /// Numbers three through five are such numbers: the game keeps the Zebetite count
    /// (<see cref="PersistentProgress.ZebetitesDestroyed"/>) there, and only the stored-progress codecs at the save and
    /// import boundaries place it in them.
    /// </summary>
    private static readonly EventNumber?[] Numbered =
    [
        EventNumber.ZebesAwake, EventNumber.ShitroidAteSidehopper, EventNumber.MotherBrainGlassDestroyed,
        null, null, null,
        EventNumber.PhantoonStatueGrey, EventNumber.RidleyStatueGrey, EventNumber.DraygonStatueGrey,
        EventNumber.KraidStatueGrey, EventNumber.TourianUnlocked, EventNumber.MaridiaNoobTubeBroken,
        EventNumber.LowerNorfairChozoLoweredAcid, EventNumber.ShaktoolClearedPath, EventNumber.ZebesTimebombSet,
        EventNumber.CrittersEscaped, EventNumber.FirstMetroidHallCleared, EventNumber.FirstMetroidShaftCleared,
        EventNumber.SecondMetroidHallCleared, EventNumber.SecondMetroidShaftCleared, EventNumber.ShitroidApproachCalmed,
        EventNumber.OutranSpeedBoosterLavaquake,
    ];

    /// <summary>The number the game gives <paramref name="eventNumber"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="eventNumber"/> names no retail event.</exception>
    public static int ToEventNumber(this EventNumber eventNumber) =>
        Array.IndexOf(Numbered, eventNumber) is var number and >= 0
            ? number
            : throw new ArgumentOutOfRangeException(nameof(eventNumber), eventNumber, "The event has no proven retail EventNumber identity.");

    /// <summary>The named event the game numbers <paramref name="number"/>, when the retail game names one.</summary>
    public static bool TryFromEventNumber(int number, out EventNumber eventNumber)
    {
        EventNumber? named = (uint)number < (uint)Numbered.Length ? Numbered[number] : null;
        eventNumber = named ?? default;
        return named is not null;
    }

    /// <summary>
    /// Whether <paramref name="number"/> numbers a story event, named or not: every number of the game's event range
    /// except the ones that hold the Zebetite count.
    /// </summary>
    public static bool IsStoryEventNumber(int number) =>
        number is >= 0 and < PersistentProgress.EventCount && (number >= Numbered.Length || Numbered[number] is not null);
}
