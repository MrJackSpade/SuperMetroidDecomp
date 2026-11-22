using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A tube another tube follows: after clearing its place the collapse waits before the next tube falls.</summary>
/// <param name="Clears">The room mutation clearing the tube's place, and the block it is spawned at.</param>
/// <param name="WaitAfterUpdates">
/// The updates the collapse waits after the clear before the next tube falls, the last being the update it falls on.
/// </param>
internal abstract partial record MotherBrainFollowedTubeDrop(RoomObjectSpawnRequest<MotherBrainRoomMutation> Clears, int WaitAfterUpdates)
    : MotherBrainTubeDrop(Clears);
