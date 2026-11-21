using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The seal of the Ceres elevator room's door during the escape (#2494): where the room object that seals it stands and
/// what its setup makes of the door's blocks.
/// </summary>
internal static partial class CeresElevatorDoorSealDefinitions
{
    /// <summary>The door's top block, which the seal stands at; the door's blocks run down from it.</summary>
    internal static readonly Position16<RoomBlocks> DoorTop = new(15, 38);

    /// <summary>The door's height in blocks: its top block and the blocks below it, each of which the seal turns.</summary>
    internal const int HeightInBlocks = 4;

    /// <summary>The collision kind the seal gives each of the door's blocks, which then stop Samus as a wall does.</summary>
    internal const RoomCollisionType SealedCollision = RoomCollisionType.SolidBlock;
}
