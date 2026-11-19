using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One tube the collapse about Mother Brain's tank drops in her fake death (#2258): the blocks clearing its place leaves,
/// and the wait after the clear before the next tube falls. What falls is the drop's own kind.
/// </summary>
/// <param name="Clears">The room mutation clearing the tube's place, and the block it is spawned at.</param>
/// <param name="WaitAfterUpdates">
/// The updates the collapse waits after the clear before the next tube falls, the last being the update it falls on; none
/// for the last tube, which no tube follows.
/// </param>
internal abstract record MotherBrainTubeDrop(RoomObjectSpawnRequest<MotherBrainRoomMutation> Clears, int? WaitAfterUpdates);
