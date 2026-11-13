using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>Facts the game's room-state conditions inspect.</summary>
/// <param name="Events">A detached copy of the story events that have happened; null when none has.</param>
/// <param name="DefeatedBosses">The bosses defeated in the room's area.</param>
/// <param name="HasMorphBallAndMissiles">Whether Samus carries the morph ball and missiles.</param>
/// <param name="HasPowerBombs">Whether Samus carries power bombs.</param>
[IndependentFlag("independent equipment facts the room-state tests read")]
public readonly record struct RoomStateSelectionContext(
    ProgressSet<StoryEvent>? Events,
    DefeatedBosses DefeatedBosses,
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

    /// <summary>Tests whether any of <paramref name="bosses"/> is defeated in the selected area.</summary>
    /// <param name="bosses">Bosses of the proven kinds of <see cref="Progression.DefeatedBosses"/>; none is allowed.</param>
    /// <returns>True when at least one of them is defeated; asking for none returns false.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bosses"/> names an unproven kind of boss.</exception>
    public bool IsBossDead(DefeatedBosses bosses)
    {
        ProvenDefeatedBosses.Validate(bosses, "Room-state boss selector");
        return (DefeatedBosses & bosses) != Progression.DefeatedBosses.None;
    }
}
