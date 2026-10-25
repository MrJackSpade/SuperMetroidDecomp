using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>Facts inspected by the cartridge's room-state selector functions.</summary>
[IndependentFlag("independent equipment facts the room-state tests read")]
public readonly record struct RoomStateSelectionContext(
    ReadOnlyMemory<byte> Events,
    BossBits BossBits,
    bool HasMorphBallAndMissiles,
    bool HasPowerBombs)
{
    /// <summary>Tests one native event bit, treating indices beyond the supplied event memory as clear.</summary>
    /// <param name="eventIndex">Zero-based bit index into the event byte sequence.</param>
    /// <returns>True when the corresponding supplied bit is set; false when clear or absent.</returns>
    public bool IsEventSet(int eventIndex)
    {
        Ensure.AtLeastZero(eventIndex);
        ReadOnlySpan<byte> events = Events.Span;
        int byteIndex = eventIndex >> 3;
        return byteIndex < events.Length && (events[byteIndex] & (1 << (eventIndex & 7))) != 0;
    }

    /// <summary>Tests whether the named persistent event <paramref name="eventNumber"/> is set.</summary>
    public bool IsEventSet(EventNumber eventNumber) => IsEventSet(eventNumber.ToEventBit());

    /// <summary>Tests whether the selected area's boss-state word contains any bit from a validated known mask.</summary>
    /// <param name="mask">A mask composed only of established <see cref="BossBits"/> values; zero is allowed.</param>
    /// <returns>True when at least one selected boss-defeat bit is set; a zero mask returns false.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The mask contains an unproven boss-state bit.</exception>
    public bool IsBossDead(BossBits mask)
    {
        BossBitMasks.Validate(mask, "Room-state boss selector");
        return (BossBits & mask) != BossBits.None;
    }
}
