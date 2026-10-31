using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>Facts inspected by the cartridge's room-state selector functions.</summary>
/// <param name="Events">A detached copy of the story events that have happened, by event number; null when none has.</param>
/// <param name="BossBits">The bosses defeated in the room's area.</param>
/// <param name="HasMorphBallAndMissiles">Whether Samus carries the morph ball and missiles.</param>
/// <param name="HasPowerBombs">Whether Samus carries power bombs.</param>
[IndependentFlag("independent equipment facts the room-state tests read")]
public readonly record struct RoomStateSelectionContext(
    NumberedFlags? Events,
    BossBits BossBits,
    bool HasMorphBallAndMissiles,
    bool HasPowerBombs)
{
    /// <summary>Tests whether the story event the game numbers <paramref name="eventNumber"/> has happened.</summary>
    /// <param name="eventNumber">The event's number, 0 through 63.</param>
    /// <returns>True when the event has happened; false when it has not or no events were supplied.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The number is outside the game's event numbers.</exception>
    public bool IsEventSet(int eventNumber)
    {
        Ensure.BetweenInclusive(0, PersistentProgress.EventCount - 1, eventNumber);
        return Events?.Contains(eventNumber) == true;
    }

    /// <summary>Tests whether the named persistent event <paramref name="eventNumber"/> is set.</summary>
    public bool IsEventSet(EventNumber eventNumber) => IsEventSet(eventNumber.ToEventNumber());

    /// <summary>Tests whether the selected area's boss-state word contains any bit from a validated known mask.</summary>
    /// <param name="mask">A mask composed only of established <see cref="BossBits"/> values; zero is allowed.</param>
    /// <returns>True when at least one selected boss-defeat bit is set; a zero mask returns false.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The mask contains an unproven boss-state bit.</exception>
    public bool IsBossDead(BossBits mask)
    {
        ProvenBossBits.Validate(mask, "Room-state boss selector");
        return (BossBits & mask) != BossBits.None;
    }
}
