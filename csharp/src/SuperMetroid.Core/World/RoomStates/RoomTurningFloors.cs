namespace SuperMetroid.Core.World.RoomStates;

/// <summary>Reads what a turning floor does to its room's screen (<see cref="RoomTurningFloorDefinitions"/>).</summary>
public static partial class RoomTurningFloors
{
    /// <summary>What a turning floor does to its room's screen.</summary>
    extension(RoomTurningFloor floor)
    {
        /// <summary>Whether the floor is its screen alone, drawing none of its level's blocks.</summary>
        public bool HidesLevelBlocks => RoomTurningFloorDefinitions.HidingLevelBlocks.Contains(floor);
    }
}
