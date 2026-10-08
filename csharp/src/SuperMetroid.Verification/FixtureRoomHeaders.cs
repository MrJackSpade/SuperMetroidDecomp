/// <summary>
/// Bank-$8F room-header pointers that only regression fixtures load. Production reaches
/// these rooms through door data, so the names live with the tests rather than in Core.
/// </summary>
internal static class FixtureRoomHeaders
{
    /// <summary>RoomHeader_RisingTide at $8F:AFA3, Norfair's rising-lava corridor.</summary>
    public const ushort RisingTide = 0xafa3;

    /// <summary>RoomHeader_Pillar at $8F:B457, Lower Norfair's Puromi (fire arc) room.</summary>
    public const ushort LowerNorfairPillar = 0xb457;

    /// <summary>RoomHeader_MetalPirates at $8F:B62B, Lower Norfair's Gold Ninja Pirate room.</summary>
    public const ushort MetalPirates = 0xb62b;

    /// <summary>RoomHeader_ThreeMusketeers at $8F:B656, Lower Norfair's shootable-shutter corridor.</summary>
    public const ushort ThreeMusketeers = 0xb656;

    /// <summary>RoomHeader_LNFireflea at $8F:B6EE, Lower Norfair's Fireflea room.</summary>
    public const ushort LowerNorfairFireflea = 0xb6ee;

    /// <summary>RoomHeader_RedFish at $8F:D104, Maridia's sloped Red Fish room.</summary>
    public const ushort RedFish = 0xd104;

    /// <summary>RoomHeader_Metroids1 at $8F:DAE1, Tourian's first Metroid room.</summary>
    public const ushort TourianMetroids1 = 0xdae1;
}
