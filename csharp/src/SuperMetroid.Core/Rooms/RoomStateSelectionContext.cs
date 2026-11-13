using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>Facts the game's room-state conditions inspect.</summary>
/// <param name="Events">A detached copy of the story events that have happened; null when none has.</param>
/// <param name="BossBits">The bosses defeated in the room's area.</param>
/// <param name="HasMorphBallAndMissiles">Whether Samus carries the morph ball and missiles.</param>
/// <param name="HasPowerBombs">Whether Samus carries power bombs.</param>
[IndependentFlag("independent equipment facts the room-state tests read")]
public readonly record struct RoomStateSelectionContext(
    ProgressSet<StoryEvent>? Events,
    BossBits BossBits,
    bool HasMorphBallAndMissiles,
    bool HasPowerBombs)
{
    /// <summary>Tests whether <paramref name="storyEvent"/> has happened.</summary>
    /// <param name="storyEvent">The story event.</param>
    /// <returns>True when the event has happened; false when it has not or no events were supplied.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="storyEvent"/> is not a defined story event.</exception>
    public bool IsEventSet(StoryEvent storyEvent) =>
        Enum.IsDefined(storyEvent)
            ? Events?.Contains(storyEvent) == true
            : throw new ArgumentOutOfRangeException(nameof(storyEvent), storyEvent, "Undefined story event.");

    /// <summary>Tests whether the selected area's boss-defeat flags contain any of a validated known mask.</summary>
    /// <param name="mask">A mask composed only of established <see cref="BossBits"/> values; zero is allowed.</param>
    /// <returns>True when at least one selected boss-defeat bit is set; a zero mask returns false.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The mask contains an unproven boss-state bit.</exception>
    public bool IsBossDead(BossBits mask)
    {
        ProvenBossBits.Validate(mask, "Room-state boss selector");
        return (BossBits & mask) != BossBits.None;
    }
}
