namespace SuperMetroid.Core.Rooms;

/// <summary>Retail door headers that verification scenarios enter rooms through.</summary>
internal static class DoorPointers
{
    public const ushort MaridiaElevatubeFromSouth = 0xa678;
    public const ushort MaridiaElevatubeFromNorth = 0xa5ac;
    public const ushort ParlorFromClimb = 0x8b3e;
    public const ushort BlueBrinstarElevatorFromMorphBall = 0x8eb6;
    public const ushort ConstructionZoneFromFirstMissile = 0x8fa6;
    public const ushort CeresFinalHallwayFromDeadScientist = 0xab94;
    public const ushort CeresDeadScientistFromFinalHallway = 0xaba0;
    public const ushort FromCeresElevatorShaft = 0xab4c;
    public const ushort ToCeresElevatorShaft = 0xab58;
}
