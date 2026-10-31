namespace SuperMetroid.Core.Progression;

/// <summary>
/// The single conversion between a named <see cref="EventNumber"/> and the number the game gives that story event, which
/// room data and saves use. Nothing else turns an event into its number.
/// </summary>
[EncodingOwner("story event numbering")]
public static class EventNumberCodec
{
    /// <summary>The named events in the order the game numbers them, from zero.</summary>
    private static readonly EventNumber[] Numbered =
    [
        EventNumber.ZebesAwake, EventNumber.ShitroidAteSidehopper, EventNumber.MotherBrainGlassDestroyed,
        EventNumber.ZebetiteDestroyedBit0, EventNumber.ZebetiteDestroyedBit1, EventNumber.ZebetiteDestroyedBit2,
        EventNumber.PhantoonStatueGrey, EventNumber.RidleyStatueGrey, EventNumber.DraygonStatueGrey,
        EventNumber.KraidStatueGrey, EventNumber.TourianUnlocked, EventNumber.MaridiaNoobTubeBroken,
        EventNumber.LowerNorfairChozoLoweredAcid, EventNumber.ShaktoolClearedPath, EventNumber.ZebesTimebombSet,
        EventNumber.CrittersEscaped, EventNumber.FirstMetroidHallCleared, EventNumber.FirstMetroidShaftCleared,
        EventNumber.SecondMetroidHallCleared, EventNumber.SecondMetroidShaftCleared, EventNumber.Unused14,
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
        bool named = (uint)number < (uint)Numbered.Length;
        eventNumber = named ? Numbered[number] : default;
        return named;
    }
}
