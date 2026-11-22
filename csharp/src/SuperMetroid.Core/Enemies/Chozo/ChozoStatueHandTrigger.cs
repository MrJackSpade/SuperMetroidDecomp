using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// What triggering a Chozo statue's hand wakes: the statue that spawned the hand, the first enemy its room lists (#2554). The hand's spawn carries it to the room's statue objects, which hold it while the
/// hand's block can be touched.
/// </summary>
internal sealed partial class ChozoStatueHandTrigger
{
    /// <summary>The Chozo statue species owner, which runs the hand's activation.</summary>
    private readonly ChozoStatueBehavior _statue;

    /// <summary>The state of the statue the hand belongs to.</summary>
    private readonly ChozoStatueState _state;

    /// <summary>
    /// Holds the statue the hand wakes. An ordinary constructor, not a primary one, so a restore of the game's state makes
    /// the trigger before it reads the statue's species owner, whose room's statue objects hold the trigger (#2554).
    /// </summary>
    /// <param name="statue">The Chozo statue species owner, which runs the hand's activation.</param>
    /// <param name="state">The state of the statue the hand belongs to.</param>
    internal ChozoStatueHandTrigger(ChozoStatueBehavior statue, ChozoStatueState state)
    {
        _statue = Ensure.NotNull(statue);
        _state = Ensure.NotNull(state);
    }

    /// <summary>The site of the statue the hand belongs to.</summary>
    internal ChozoStatueSite Site => _state.Site;

    /// <summary>Wakes the statue; returns the room objects it asks for at once, which the statue's room objects spawn.</summary>
    internal IReadOnlyList<RoomObjectSpawnRequest<ChozoStatueRoomObject>> Activate() => _statue.ActivateChozoStatueHandTrigger(_state);
}
