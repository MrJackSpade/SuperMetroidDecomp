namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What a door's setup does to the room it leads into.</summary>
public static class DoorSetups
{
    /// <summary>The setup a door runs as Samus passes it.</summary>
    extension(DoorDefinition door)
    {
        /// <summary>
        /// True only for the door into the Ceres elevator shaft, whose setup shows the room as a turning floor,
        /// unrotated, about room point (128, 1008).
        /// </summary>
        public bool EntersCeresElevatorShaft => door.Setup.RoomEvent == DoorRoomEvent.ShowsTurningFloor;
    }
}
