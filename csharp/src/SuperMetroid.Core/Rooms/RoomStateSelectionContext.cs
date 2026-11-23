using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>Facts the game's room-state conditions inspect.</summary>
/// <remarks>
/// A reference type whose constructor requires the story events, so no context exists without them: a selection can
/// never read the events of a context nobody built.
/// </remarks>
/// <param name="Events">A detached copy of the story events that have happened.</param>
/// <param name="DefeatedBosses">The bosses defeated in the room's area.</param>
/// <param name="HasMorphBallAndMissiles">Whether Samus carries the morph ball and missiles.</param>
/// <param name="HasPowerBombs">Whether Samus carries power bombs.</param>
/// <exception cref="ArgumentNullException"><paramref name="Events"/> is null.</exception>
public sealed partial record RoomStateSelectionContext(
    ProgressSet<StoryEvent> Events,
    DefeatedBosses DefeatedBosses,
    bool HasMorphBallAndMissiles,
    bool HasPowerBombs)
{
    /// <summary>A detached copy of the story events that have happened.</summary>
    public ProgressSet<StoryEvent> Events { get; } = Ensure.NotNull(Events);

    /// <summary>Tests whether <paramref name="storyEvent"/> has happened.</summary>
    /// <param name="storyEvent">The story event.</param>
    /// <returns>True when the event has happened.</returns>
    public bool IsEventSet(StoryEvent storyEvent) => Events.Contains(storyEvent);

    /// <summary>Tests whether any of <paramref name="bosses"/> is defeated in the selected area.</summary>
    /// <param name="bosses">The bosses asked for; none is allowed.</param>
    /// <returns>True when at least one of them is defeated; asking for none returns false.</returns>
    public bool IsBossDead(DefeatedBosses bosses) => (DefeatedBosses & bosses) != Progression.DefeatedBosses.None;
}
