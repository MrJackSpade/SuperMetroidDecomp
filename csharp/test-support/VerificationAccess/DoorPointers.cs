namespace SuperMetroid.Core.Rooms;

/// <summary>Retail door headers that verification scenarios enter rooms through.</summary>
internal static class DoorPointers
{
    /// <summary>Stores maridia elevatube from south.</summary>
    public const ushort MaridiaElevatubeFromSouth = 0xa678;
    /// <summary>Stores maridia elevatube from north.</summary>
    public const ushort MaridiaElevatubeFromNorth = 0xa5ac;
    /// <summary>Stores parlor from climb.</summary>
    public const ushort ParlorFromClimb = 0x8b3e;
    /// <summary>Stores blue brinstar elevator from morph ball.</summary>
    public const ushort BlueBrinstarElevatorFromMorphBall = 0x8eb6;
    /// <summary>Stores construction zone from first missile.</summary>
    public const ushort ConstructionZoneFromFirstMissile = 0x8fa6;
    /// <summary>Stores ceres final hallway from dead scientist.</summary>
    public const ushort CeresFinalHallwayFromDeadScientist = 0xab94;
    /// <summary>Stores ceres dead scientist from final hallway.</summary>
    public const ushort CeresDeadScientistFromFinalHallway = 0xaba0;
    /// <summary>Stores from ceres elevator shaft.</summary>
    public const ushort FromCeresElevatorShaft = 0xab4c;
    /// <summary>Stores to ceres elevator shaft.</summary>
    public const ushort ToCeresElevatorShaft = 0xab58;
}
