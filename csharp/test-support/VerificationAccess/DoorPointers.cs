namespace SuperMetroid.Core.Rooms;

/// <summary>Retail door headers that verification scenarios enter rooms through.</summary>
internal static class DoorPointers
{
    /// <summary>Door-header pointer for entering the Maridia Elevatube through its south end and traveling north.</summary>
    public const ushort MaridiaElevatubeFromSouth = 0xa678;

    /// <summary>Door-header pointer for entering the Maridia Elevatube through its north end and traveling south.</summary>
    public const ushort MaridiaElevatubeFromNorth = 0xa5ac;

    /// <summary>Parlor door-header pointer used to verify the incoming scroll setup after returning from Climb.</summary>
    public const ushort ParlorFromClimb = 0x8b3e;

    /// <summary>Blue Brinstar elevator-room door header used for the upward return from Morph Ball Room.</summary>
    public const ushort BlueBrinstarElevatorFromMorphBall = 0x8eb6;

    /// <summary>Construction Zone door-header pointer for the return from First Missile.</summary>
    public const ushort ConstructionZoneFromFirstMissile = 0x8fa6;

    /// <summary>Door header entering Ceres Final Hallway from the Dead Scientist room.</summary>
    public const ushort CeresFinalHallwayFromDeadScientist = 0xab94;

    /// <summary>Door header entering the Ceres Dead Scientist room from Final Hallway.</summary>
    public const ushort CeresDeadScientistFromFinalHallway = 0xaba0;

    /// <summary>Door header for leaving the Ceres Mode 7 elevator shaft into the ordinary room route.</summary>
    public const ushort FromCeresElevatorShaft = 0xab4c;

    /// <summary>Door header for entering the Ceres Mode 7 elevator shaft from the adjacent room.</summary>
    public const ushort ToCeresElevatorShaft = 0xab58;
}
