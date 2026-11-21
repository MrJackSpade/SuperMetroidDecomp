using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A tube hanging from the ceiling above her tank that falls as an enemy projectile.</summary>
/// <param name="Kind">The falling tube projectile.</param>
/// <param name="Segment">How the falling tube shows.</param>
/// <param name="Start">Where the tube starts falling.</param>
/// <param name="Clears">The room mutation clearing the tube's place, and the block it is spawned at.</param>
/// <param name="WaitAfterUpdates">The updates the collapse waits after the clear before the next tube falls.</param>
internal sealed partial record MotherBrainTopTubeDrop(
    RoomEnemyProjectileKind Kind,
    MotherBrainTopTubeSegment Segment,
    Position16<RoomPixels> Start,
    RoomObjectSpawnRequest<MotherBrainRoomMutation> Clears,
    int WaitAfterUpdates) : MotherBrainFollowedTubeDrop(Clears, WaitAfterUpdates);
