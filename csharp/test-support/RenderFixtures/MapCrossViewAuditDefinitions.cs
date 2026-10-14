/// <summary>Representative retail rooms used to compare both map presentation paths.</summary>
internal static class MapCrossViewAuditDefinitions
{
    /// <summary>Landing Site, a left-page Crateria room at <c>$8F:91F8</c>.</summary>
    public const ushort LandingSiteRoom = 0x91f8;
    /// <summary>Water room <c>$01/$27</c>, on Brinstar's right map page at <c>$8F:A3DD</c>.</summary>
    public const ushort WaterRoom = 0xa3dd;
    /// <summary>Crocomire room, a left-page Norfair room at <c>$8F:A98D</c>.</summary>
    public const ushort CrocomireRoom = 0xa98d;
    /// <summary>Botwoon room, a left-page Maridia room at <c>$8F:D95E</c>.</summary>
    public const ushort BotwoonRoom = 0xd95e;

    /// <summary>One retail room per selected area, including a right-page Brinstar room.</summary>
    public static readonly ushort[] RepresentativeRooms =
    [
        LandingSiteRoom,
        WaterRoom,
        CrocomireRoom,
        BotwoonRoom,
    ];
}
