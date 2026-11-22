using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// What triggering a Chozo statue's hand wakes: the statue that spawned the hand, the first enemy its room lists (#2554). The hand's spawn carries it to the room's statue objects, which hold it while the
/// hand's block can be touched.
/// </summary>
/// <param name="statue">The Chozo statue species owner, which runs the hand's activation.</param>
/// <param name="state">The state of the statue the hand belongs to.</param>
[RoomLifetime]
internal sealed partial class ChozoStatueHandTrigger(ChozoStatueBehavior statue, ChozoStatueState state)
{
    /// <summary>The site of the statue the hand belongs to.</summary>
    internal ChozoStatueSite Site => state.Site;

    /// <summary>Wakes the statue; returns the room objects it asks for at once, which the statue's room objects spawn.</summary>
    internal IReadOnlyList<RoomObjectSpawnRequest<ChozoStatueRoomObject>> Activate() => statue.ActivateChozoStatueHandTrigger(state);
}
