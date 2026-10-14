namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Retail door headers that verification scenarios enter rooms through.</summary>
internal static class DoorPointers
{
    /// <summary>Door header $A678; enters the Maridia elevatube from its southern room.</summary>
    public const ushort MaridiaElevatubeFromSouth = 0xa678;
    /// <summary>Door header $A5AC; enters the Maridia elevatube from its northern room.</summary>
    public const ushort MaridiaElevatubeFromNorth = 0xa5ac;
    /// <summary>Door header $8B3E; connects the Climb to the Parlor.</summary>
    public const ushort ParlorFromClimb = 0x8b3e;
    /// <summary>Door header $8EB6; connects the Morph Ball room to the Blue Brinstar elevator.</summary>
    public const ushort BlueBrinstarElevatorFromMorphBall = 0x8eb6;
    /// <summary>Door header $8FA6; enters Construction Zone from the first missile room.</summary>
    public const ushort ConstructionZoneFromFirstMissile = 0x8fa6;
    /// <summary>Door header $AB94; enters Ceres' final hallway from the dead scientist room.</summary>
    public const ushort CeresFinalHallwayFromDeadScientist = 0xab94;
    /// <summary>Door header $ABA0; returns to the dead scientist room from Ceres' final hallway.</summary>
    public const ushort CeresDeadScientistFromFinalHallway = 0xaba0;
    /// <summary>Door header $AB4C; leaves the Ceres elevator shaft for verification scenarios.</summary>
    public const ushort FromCeresElevatorShaft = 0xab4c;
    /// <summary>Door header $AB58; enters the Ceres elevator shaft for verification scenarios.</summary>
    public const ushort ToCeresElevatorShaft = 0xab58;
}
