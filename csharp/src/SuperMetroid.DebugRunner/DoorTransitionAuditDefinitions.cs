/// <summary>
/// Named world positions used to put Samus immediately before a real cartridge doorway.
/// These positions replace only the controller approach; the production collision and door
/// transition code still discover and execute the authored bank-$83 door record.
/// </summary>
internal static class DoorTransitionAuditPositions
{
    /// <summary>World origin used by one-screen room viewports.</summary>
    public const ushort Origin = 0x0000;

    /// <summary>Viewport origin for the second horizontal room screen.</summary>
    public const ushort SecondScreenCameraX = 0x0100;

    /// <summary>First Missile is one screen wide, so its viewport begins at world X zero.</summary>
    public const ushort FirstMissileCameraX = Origin;

    /// <summary>First Missile is one screen tall, so its viewport begins at world Y zero.</summary>
    public const ushort FirstMissileCameraY = Origin;

    /// <summary>Standing X immediately inside First Missile's right-hand door.</summary>
    public const uint FirstMissileSamusXFixed = 0x00ed_0000;

    /// <summary>Standing Y on First Missile's authored door floor.</summary>
    public const uint FirstMissileSamusYFixed = 0x008b_0000;

    /// <summary>Lower viewport of the two-screen-tall Ceres magnet-stairs room.</summary>
    public const ushort CeresMagnetStairsCameraY = 0x0100;

    /// <summary>Standing X immediately inside a room's right-hand horizontal door.</summary>
    public const uint RightDoorSamusXFixed = 0x00ed_0000;

    /// <summary>Standing X immediately inside a two-screen room's right-hand horizontal door.</summary>
    public const uint SecondScreenRightDoorSamusXFixed = 0x01ed_0000;

    /// <summary>Standing X immediately inside a room's left-hand horizontal door.</summary>
    public const uint LeftDoorSamusXFixed = 0x0014_0000;

    /// <summary>Standing Y on the Ceres magnet-stairs room's lower door floor.</summary>
    public const uint CeresMagnetStairsDoorSamusYFixed = 0x018b_0000;

    /// <summary>Standing Y on the single-screen Ceres corridor door floor.</summary>
    public const uint CeresCorridorDoorSamusYFixed = 0x008b_0000;
}

/// <summary>VRAM regions inspected by the door-transition visual regression.</summary>
internal static class DoorTransitionAuditVram
{
    /// <summary>First word of the live two-page BG1 room tilemap at VRAM $5000.</summary>
    public const ushort Bg1TilemapFirstWord = 0x5000;

    /// <summary>Two 32x32 BG1 tilemap pages, expressed as 16-bit words.</summary>
    public const ushort Bg1TilemapWordCount = 0x0800;
}
