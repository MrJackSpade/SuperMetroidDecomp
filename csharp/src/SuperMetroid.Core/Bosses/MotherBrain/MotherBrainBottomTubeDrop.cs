using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A tube standing about her tank that falls as one of her falling tube pieces.</summary>
/// <param name="Piece">The falling tube piece that spawns.</param>
/// <param name="Clears">The room mutation clearing the tube's place, and the block it is spawned at.</param>
/// <param name="WaitAfterUpdates">The updates the collapse waits after the clear before the next tube falls.</param>
internal sealed record MotherBrainBottomTubeDrop(
    MotherBrainTubePiece Piece,
    RoomObjectSpawnRequest<MotherBrainRoomMutation> Clears,
    int WaitAfterUpdates) : MotherBrainTubeDrop(Clears, WaitAfterUpdates);
