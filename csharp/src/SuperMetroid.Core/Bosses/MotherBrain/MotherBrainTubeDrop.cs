using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One tube the collapse about Mother Brain's tank drops in her fake death (#2258): the blocks clearing its place leaves.
/// What falls, and whether a tube follows it, is the drop's own kind.
/// </summary>
/// <param name="Clears">The room mutation clearing the tube's place, and the block it is spawned at.</param>
internal abstract record MotherBrainTubeDrop(RoomObjectSpawnRequest<MotherBrainRoomMutation> Clears);
