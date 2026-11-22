using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The Ceres escape's room objects: the seal that shuts the elevator room's door once Ceres Ridley is dead (#2494).</summary>
[RoomLifetime]
internal sealed partial class CeresEscapeObjects
{
    /// <summary>The room's room objects, which the door's seal joins.</summary>
    private readonly RoomObjectPlaceList _places;

    /// <summary>Binds the Ceres escape's room objects to the room's room-object places.</summary>
    internal CeresEscapeObjects(RoomObjectPlaceList places) => _places = Ensure.NotNull(places);

    /// <summary>
    /// Runs the room setup that spawns the elevator door's seal at the door's top block, whose setup turns that block and
    /// each below it down the door solid. Only each block's collision changes: its picture, what it shows and its behaviour
    /// stay, so the door looks the same and stops Samus instead of leading out.
    /// </summary>
    public void SealElevatorDoor(RoomLevelData level)
    {
        Ensure.NotNull(level);
        Position16<RoomBlocks> top = level.GridBlock(CeresElevatorDoorSealDefinitions.DoorTop);
        if (_places.Spawn() is not { } place)
            return;

        place.Block = top;
        for (int row = 0; row < CeresElevatorDoorSealDefinitions.HeightInBlocks; row++)
            level.SetCollision(top + new Offset16<RoomBlocks>(0, row), CeresElevatorDoorSealDefinitions.SealedCollision);
        place.Become(new CeresElevatorDoorSealObject());
    }
}
