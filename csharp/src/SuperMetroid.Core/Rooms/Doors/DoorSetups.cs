namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What a door's setup routine does to the room it leads into.</summary>
public static class DoorSetups
{
    /// <summary>The setup routine a door runs as Samus passes it.</summary>
    extension(DoorDefinition door)
    {
        /// <summary>
        /// True only for the door into the Ceres elevator shaft, whose setup routine shows the room as a turning floor,
        /// unrotated, about room point (128, 1008).
        /// </summary>
        public bool EntersCeresElevatorShaft => door.SetupCode == DoorSetupCode.DoorASM_ToCeresElevatorShaft;
    }
}
