using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One change Mother Brain's fight asks of her room in a pass (#2258), spawned in the order asked: one of her fixed
/// mutations at a block (<see cref="MotherBrainMutationRequest"/>), or the reveal of a row of the mural behind her tank at
/// the row's own block (<see cref="MotherBrainMuralRowReveal"/>).
/// </summary>
/// <param name="Block">The room block the change is spawned at.</param>
public abstract partial record MotherBrainRoomChangeRequest(Position16<RoomBlocks> Block) : EnemyPassEvent
{
    /// <summary>The terrain change the spawned room object makes.</summary>
    internal abstract TerrainChange Change { get; }
}
